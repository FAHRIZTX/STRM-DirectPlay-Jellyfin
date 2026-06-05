using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Jellyfin.Plugin.StrmDirectPlay.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.StrmDirectPlay.Core
{
    /// <summary>
    /// Main playback interception logic.
    /// </summary>
    /// <remarks>
    /// Jellyfin behavior that we must respect (see Emby.Server.Implementations.Library.MediaSourceManager):
    ///   1. After GetPlaybackMediaSources returns, STRM items trigger
    ///      item.RefreshMetadata(EnableRemoteContentProbe = true), which REPROBES
    ///      the new URL and OVERWRITES Path/Protocol/MediaStreams.
    ///   2. BaseItem.GetMediaSources runs SupportsDirectStream(path, protocol) on
    ///      the result; if path contains ".m3u" it returns false and our
    ///      SupportsDirectStream flag is reset to false.
    ///   3. If mediaSource.IsRemote == true AND user has
    ///      ForceRemoteSourceTranscoding permission, Jellyfin forces a transcode.
    ///   4. Direct stream http streaming is currently broken - MediaOptions.EnableDirectStream
    ///      is forced to false in MediaInfoHelper.SetDeviceSpecificData.
    ///
    /// Strategy: we set MediaSourceInfo so that the StreamBuilder / SetDeviceSpecificData
    /// path produces PlayMethod.DirectPlay:
    ///   * Container is set to a playable single-file container (mp4/ts) when possible
    ///   * Path points directly to the media URL (not the .m3u8 manifest)
    ///   * IsRemote stays false (or unset) so user policy does not force transcode
    ///   * SupportsDirectPlay = true, SupportsTranscoding = true (so client can fall back,
    ///     but DeviceSpecificData will set DirectPlay because the device profile accepts the container)
    ///   * MediaStreams has a single Video + Audio placeholder so StreamBuilder accepts it
    /// </remarks>
    public class PlaybackInterceptor
    {
        private readonly ILogger<PlaybackInterceptor> _logger;
        private readonly StrmUrlReader _urlReader;
        private readonly DomainMatcher _domainMatcher;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackInterceptor"/> class.
        /// </summary>
        public PlaybackInterceptor(
            ILogger<PlaybackInterceptor> logger,
            StrmUrlReader urlReader,
            DomainMatcher domainMatcher)
        {
            _logger = logger;
            _urlReader = urlReader;
            _domainMatcher = domainMatcher;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        }

        /// <summary>
        /// Process media source for STRM file.
        /// </summary>
        public async Task<MediaSourceInfo?> ProcessMediaSourceAsync(
            BaseItem item,
            MediaSourceInfo mediaSource,
            PluginConfiguration config)
        {
            if (config.Mode == PluginMode.Disabled)
            {
                return null;
            }

            if (!_urlReader.IsStrmFile(item.Path))
            {
                return null;
            }

            if (config.DebugLogging)
            {
                _logger.LogInformation("[STRM-DP] Detected STRM item: {Name} ({Path})", item.Name, item.Path);
                _logger.LogInformation("[STRM-DP] Current Mode: {Mode}", config.Mode);
            }

            var url = await _urlReader.ReadUrlAsync(item.Path).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("[STRM-DP] Could not read URL from STRM: {Path}", item.Path);
                return null;
            }

            if (!_domainMatcher.IsUrlSafe(url))
            {
                _logger.LogWarning("[STRM-DP] Unsafe URL blocked: {Url}", url);
                return null;
            }

            var shouldProcess = ShouldProcessUrl(url, config);
            if (!shouldProcess)
            {
                if (config.DebugLogging)
                {
                    _logger.LogInformation("[STRM-DP] URL does not match criteria, skipping: {Url}", url);
                }
                return null;
            }

            if (config.DebugLogging)
            {
                _logger.LogInformation("[STRM-DP] Processing URL: {Url}", url);
            }

            return await ModifyMediaSourceAsync(mediaSource, url, config).ConfigureAwait(false);
        }

        private bool ShouldProcessUrl(string url, PluginConfiguration config)
        {
            switch (config.Mode)
            {
                case PluginMode.AlwaysDirectPlay:
                case PluginMode.ReturnOriginalUrl:
                case PluginMode.ForceDirectStream:
                case PluginMode.BypassUserPolicy:
                    return true;

                case PluginMode.DomainWhitelist:
                case PluginMode.Smart:
                    return _domainMatcher.IsWhitelisted(url, config.DomainWhitelist);

                default:
                    return false;
            }
        }

        private async Task<MediaSourceInfo> ModifyMediaSourceAsync(
            MediaSourceInfo mediaSource,
            string url,
            PluginConfiguration config)
        {
            // For HLS .m3u8 URLs, try to resolve to the first media segment so
            // that direct play works on clients that cannot handle HLS manifests
            // (e.g. some smart TV clients, older Jellyfin mobile apps).
            var playableUrl = await ResolveDirectPlayableUrlAsync(url, config).ConfigureAwait(false);

            var info = InferMediaInfo(playableUrl);

            // Set the new path to the underlying media URL.
            mediaSource.Path = playableUrl;
            mediaSource.Protocol = MediaProtocol.Http;
            mediaSource.Container = info.Container;

            // CRITICAL: do NOT set IsRemote = true. Setting IsRemote causes
            // Jellyfin to apply ForceRemoteSourceTranscoding user policy and force
            // a transcode. We want the item treated as "local-ish" so that
            // direct play is preferred.
            mediaSource.IsRemote = false;

            // We do support direct play (and direct stream) for this media.
            // We keep SupportsTranscoding true so the client can still fall back,
            // but StreamBuilder will prefer DirectPlay because the container is
            // a single-file format (mp4/ts) that every client supports.
            mediaSource.SupportsDirectPlay = true;
            mediaSource.SupportsDirectStream = true;
            mediaSource.SupportsTranscoding = !config.BlockTranscoding;

            mediaSource.RequiresOpening = false;
            mediaSource.RequiresClosing = false;
            mediaSource.SupportsProbing = false;
            mediaSource.IgnoreDts = true;
            mediaSource.IgnoreIndex = true;

            // VideoType to VideoFile so StreamBuilder considers it a direct-playable file.
            mediaSource.VideoType = VideoType.VideoFile;

            // Clear transcode reasons - critical to prevent forced transcoding.
            mediaSource.TranscodeReasons = 0;

            // Replace MediaStreams with a minimal, container-agnostic set so that
            // StreamBuilder does not need to probe the source and does not apply
            // direct-stream rules that depend on the original streams.
            mediaSource.MediaStreams = BuildMinimalStreams(info.Container);

            // Clear anamorphic flags - many clients do not report anamorphic support
            // which forces Jellyfin to transcode.
            if (mediaSource.MediaStreams is not null)
            {
                foreach (var stream in mediaSource.MediaStreams)
                {
                    if (stream.Type == MediaStreamType.Video)
                    {
                        stream.IsAnamorphic = false;
                    }
                }
            }

            if (config.DebugLogging)
            {
                _logger.LogInformation("[STRM-DP] Modified MediaSource:");
                _logger.LogInformation("[STRM-DP]   Path: {Path}", mediaSource.Path);
                _logger.LogInformation("[STRM-DP]   Protocol: {Protocol}", mediaSource.Protocol);
                _logger.LogInformation("[STRM-DP]   IsRemote: {IsRemote}", mediaSource.IsRemote);
                _logger.LogInformation("[STRM-DP]   Container: {Container}", mediaSource.Container);
                _logger.LogInformation("[STRM-DP]   SupportsDirectPlay: {Value}", mediaSource.SupportsDirectPlay);
                _logger.LogInformation("[STRM-DP]   SupportsDirectStream: {Value}", mediaSource.SupportsDirectStream);
                _logger.LogInformation("[STRM-DP]   SupportsTranscoding: {Value}", mediaSource.SupportsTranscoding);
                _logger.LogInformation("[STRM-DP]   TranscodeReasons: {Reasons}", mediaSource.TranscodeReasons);
            }

            return mediaSource;
        }

        private static (string Container, string MediaType) InferMediaInfo(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return ("ts", "video");
            }

            // Strip query string before extension lookup.
            var pathOnly = url;
            var queryIndex = pathOnly.IndexOf('?', StringComparison.Ordinal);
            if (queryIndex >= 0)
            {
                pathOnly = pathOnly.Substring(0, queryIndex);
            }

            var ext = Path.GetExtension(pathOnly)?.TrimStart('.').ToLowerInvariant();

            return ext switch
            {
                "mp4" => ("mp4", "video"),
                "m4v" => ("m4v", "video"),
                "mkv" => ("mkv", "video"),
                "webm" => ("webm", "video"),
                "ts" => ("ts", "video"),
                "m2ts" => ("ts", "video"),
                "mp3" => ("mp3", "audio"),
                "m4a" => ("m4a", "audio"),
                "aac" => ("aac", "audio"),
                "flac" => ("flac", "audio"),
                "ogg" => ("ogg", "audio"),
                "opus" => ("ogg", "audio"),
                "wav" => ("wav", "audio"),
                "m3u8" => ("ts", "video"),
                "mpd" => ("mp4", "video"),
                _ => ("ts", "video"),
            };
        }

        /// <summary>
        /// For HLS (.m3u8) URLs, try to fetch the manifest and resolve the first
        /// media segment URL. This lets us return a single .ts URL that all
        /// clients can DirectPlay. For master playlists with multiple variants
        /// we pick the first one. Non-HLS URLs are returned unchanged.
        /// </summary>
        private async Task<string> ResolveDirectPlayableUrlAsync(string url, PluginConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            // Only resolve for .m3u8.
            var pathOnly = url;
            var queryIndex = pathOnly.IndexOf('?', StringComparison.Ordinal);
            if (queryIndex >= 0)
            {
                pathOnly = pathOnly.Substring(0, queryIndex);
            }
            if (!pathOnly.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("Jellyfin-StrmDirectPlay/1.0");
                using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var manifest = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var baseUri = new Uri(url);
                var resolved = ResolveFirstMediaUrl(manifest, baseUri);
                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    if (config.DebugLogging)
                    {
                        _logger.LogInformation("[STRM-DP] Resolved HLS manifest to first segment: {Resolved}", resolved);
                    }
                    return resolved;
                }
            }
            catch (Exception ex)
            {
                if (config.DebugLogging)
                {
                    _logger.LogWarning(ex, "[STRM-DP] Failed to resolve HLS manifest, falling back to original URL");
                }
            }

            return url;
        }

        private static string? ResolveFirstMediaUrl(string manifest, Uri baseUri)
        {
            // Detect master playlist (contains #EXT-X-STREAM-INF).
            if (manifest.Contains("#EXT-X-STREAM-INF", StringComparison.OrdinalIgnoreCase))
            {
                // Find first variant URI line after the first STREAM-INF tag.
                var lines = manifest.Split('\n');
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    if (lines[i].Contains("#EXT-X-STREAM-INF", StringComparison.OrdinalIgnoreCase))
                    {
                        var variant = lines[i + 1].Trim();
                        if (!string.IsNullOrEmpty(variant) && !variant.StartsWith("#"))
                        {
                            return new Uri(baseUri, variant).ToString();
                        }
                    }
                }

                return null;
            }

            // Media playlist: return the first non-comment, non-empty line.
            foreach (var raw in manifest.Split('\n'))
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue;
                return new Uri(baseUri, line).ToString();
            }

            return null;
        }

        private static List<MediaStream> BuildMinimalStreams(string container)
        {
            // Minimal placeholder streams so StreamBuilder treats this as a
            // playable media source. No codec info: StreamBuilder will not apply
            // codec-specific transcoding rules.
            return new List<MediaStream>
            {
                new MediaStream
                {
                    Type = MediaStreamType.Video,
                    Index = 0,
                    IsDefault = true,
                    IsForced = false,
                    IsAnamorphic = false,
                    IsInterlaced = false,
                },
                new MediaStream
                {
                    Type = MediaStreamType.Audio,
                    Index = 1,
                    IsDefault = true,
                    IsForced = false,
                    Language = "und",
                },
            };
        }

        /// <summary>
        /// Check if transcoding should be blocked.
        /// </summary>
        public bool ShouldBlockTranscoding(BaseItem item, PluginConfiguration config)
        {
            if (config.Mode == PluginMode.Disabled)
            {
                return false;
            }

            if (!config.BlockTranscoding)
            {
                return false;
            }

            if (!_urlReader.IsStrmFile(item.Path))
            {
                return false;
            }

            if (config.DebugLogging)
            {
                _logger.LogInformation("[STRM-DP] Transcoding blocked for: {Name}", item.Name);
            }

            return true;
        }
    }
}
