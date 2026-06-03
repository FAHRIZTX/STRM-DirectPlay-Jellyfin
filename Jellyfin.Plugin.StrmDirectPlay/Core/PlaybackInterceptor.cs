using System;
using System.Threading.Tasks;
using Jellyfin.Plugin.StrmDirectPlay.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.StrmDirectPlay.Core
{
    /// <summary>
    /// Main playback interception logic.
    /// </summary>
    public class PlaybackInterceptor
    {
        private readonly ILogger<PlaybackInterceptor> _logger;
        private readonly StrmUrlReader _urlReader;
        private readonly DomainMatcher _domainMatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackInterceptor"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="urlReader">STRM URL reader.</param>
        /// <param name="domainMatcher">Domain matcher.</param>
        public PlaybackInterceptor(
            ILogger<PlaybackInterceptor> logger,
            StrmUrlReader urlReader,
            DomainMatcher domainMatcher)
        {
            _logger = logger;
            _urlReader = urlReader;
            _domainMatcher = domainMatcher;
        }

        /// <summary>
        /// Process media source for STRM file.
        /// </summary>
        /// <param name="item">Media item.</param>
        /// <param name="mediaSource">Media source.</param>
        /// <param name="config">Plugin configuration.</param>
        /// <returns>Modified media source or null if no changes.</returns>
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

            return ModifyMediaSource(mediaSource, url, config);
        }

        private bool ShouldProcessUrl(string url, PluginConfiguration config)
        {
            switch (config.Mode)
            {
                case PluginMode.AlwaysDirectPlay:
                case PluginMode.ReturnOriginalUrl:
                    return true;

                case PluginMode.DomainWhitelist:
                case PluginMode.Smart:
                    return _domainMatcher.IsWhitelisted(url, config.DomainWhitelist);

                default:
                    return false;
            }
        }

        private MediaSourceInfo ModifyMediaSource(
            MediaSourceInfo mediaSource,
            string url,
            PluginConfiguration config)
        {
            var modified = new MediaSourceInfo
            {
                Id = mediaSource.Id,
                Path = url,
                Protocol = MediaBrowser.Model.MediaInfo.MediaProtocol.Http,
                SupportsDirectPlay = true,
                SupportsDirectStream = true,
                SupportsTranscoding = !config.BlockTranscoding,
                IsRemote = true,
                IsInfiniteStream = false,
                RequiresOpening = false,
                RequiresClosing = false,
                BufferMs = mediaSource.BufferMs,
                SupportsProbing = false,
                VideoType = mediaSource.VideoType,
                Container = mediaSource.Container,
                Name = mediaSource.Name,
                RunTimeTicks = mediaSource.RunTimeTicks
            };

            // Copy media streams if available
            if (mediaSource.MediaStreams != null)
            {
                modified.MediaStreams = mediaSource.MediaStreams;
            }

            if (config.DebugLogging)
            {
                _logger.LogInformation("[STRM-DP] Modified MediaSource:");
                _logger.LogInformation("[STRM-DP]   Path: {Path}", modified.Path);
                _logger.LogInformation("[STRM-DP]   SupportsDirectPlay: {Value}", modified.SupportsDirectPlay);
                _logger.LogInformation("[STRM-DP]   SupportsTranscoding: {Value}", modified.SupportsTranscoding);
            }

            return modified;
        }

        /// <summary>
        /// Check if transcoding should be blocked.
        /// </summary>
        /// <param name="item">Media item.</param>
        /// <param name="config">Plugin configuration.</param>
        /// <returns>True if transcoding should be blocked.</returns>
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
