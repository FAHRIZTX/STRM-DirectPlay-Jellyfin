using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.StrmDirectPlay.Core;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.StrmDirectPlay
{
    /// <summary>
    /// Decorator for IMediaSourceManager to intercept media source requests for STRM files.
    /// </summary>
    public class StrmMediaSourceManager : IMediaSourceManager
    {
        private readonly IMediaSourceManager _inner;
        private readonly ILogger<StrmMediaSourceManager> _logger;
        private readonly PlaybackInterceptor _interceptor;
        private readonly StrmUrlReader _urlReader;
        private readonly DomainMatcher _domainMatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrmMediaSourceManager"/> class.
        /// </summary>
        /// <param name="inner">Inner media source manager.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public StrmMediaSourceManager(
            IMediaSourceManager inner,
            ILogger<StrmMediaSourceManager> logger,
            ILoggerFactory loggerFactory)
        {
            _inner = inner;
            _logger = logger;
            _urlReader = new StrmUrlReader(loggerFactory.CreateLogger<StrmUrlReader>());
            _domainMatcher = new DomainMatcher(loggerFactory.CreateLogger<DomainMatcher>());
            _interceptor = new PlaybackInterceptor(loggerFactory.CreateLogger<PlaybackInterceptor>(), _urlReader, _domainMatcher);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MediaSourceInfo>> GetPlaybackMediaSources(BaseItem item, User user, bool allowMediaProbe, bool enablePathSubstitution, CancellationToken cancellationToken)
        {
            var sources = await _inner.GetPlaybackMediaSources(item, user, allowMediaProbe, enablePathSubstitution, cancellationToken).ConfigureAwait(false);

            var config = Plugin.Instance?.Configuration;
            if (config == null || config.Mode == Configuration.PluginMode.Disabled)
            {
                return sources;
            }

            if (!_urlReader.IsStrmFile(item.Path))
            {
                return sources;
            }

            if (config.DebugLogging)
            {
                _logger.LogInformation("[STRM-DP] Intercepting GetPlaybackMediaSources for: {Name}", item.Name);
            }

            // Convert to list for modification
            var sourcesList = new List<MediaSourceInfo>(sources);

            // Process each source
            for (int i = 0; i < sourcesList.Count; i++)
            {
                var modified = await _interceptor.ProcessMediaSourceAsync(item, sourcesList[i], config).ConfigureAwait(false);
                if (modified != null)
                {
                    sourcesList[i] = modified;
                }
            }

            return sourcesList;
        }

        /// <inheritdoc />
        public void AddParts(IEnumerable<IMediaSourceProvider> providers)
            => _inner.AddParts(providers);

        /// <inheritdoc />
        public IReadOnlyList<MediaStream> GetMediaStreams(Guid itemId)
            => _inner.GetMediaStreams(itemId);

        /// <inheritdoc />
        public IReadOnlyList<MediaStream> GetMediaStreams(MediaStreamQuery query)
            => _inner.GetMediaStreams(query);

        /// <inheritdoc />
        public IReadOnlyList<MediaAttachment> GetMediaAttachments(Guid itemId)
            => _inner.GetMediaAttachments(itemId);

        /// <inheritdoc />
        public IReadOnlyList<MediaAttachment> GetMediaAttachments(MediaAttachmentQuery query)
            => _inner.GetMediaAttachments(query);

        /// <inheritdoc />
        public IReadOnlyList<MediaSourceInfo> GetStaticMediaSources(BaseItem item, bool enablePathSubstitution, User? user = null)
            => _inner.GetStaticMediaSources(item, enablePathSubstitution, user);

        /// <inheritdoc />
        public Task<MediaSourceInfo> GetMediaSource(BaseItem item, string mediaSourceId, string liveStreamId, bool enablePathSubstitution, CancellationToken cancellationToken)
            => _inner.GetMediaSource(item, mediaSourceId, liveStreamId, enablePathSubstitution, cancellationToken);

        /// <inheritdoc />
        public Task<LiveStreamResponse> OpenLiveStream(LiveStreamRequest request, CancellationToken cancellationToken)
            => _inner.OpenLiveStream(request, cancellationToken);

        /// <inheritdoc />
        public Task<Tuple<LiveStreamResponse, IDirectStreamProvider>> OpenLiveStreamInternal(LiveStreamRequest request, CancellationToken cancellationToken)
            => _inner.OpenLiveStreamInternal(request, cancellationToken);

        /// <inheritdoc />
        public Task<MediaSourceInfo> GetLiveStream(string id, CancellationToken cancellationToken)
            => _inner.GetLiveStream(id, cancellationToken);

        /// <inheritdoc />
        public ILiveStream? GetLiveStreamInfo(string id)
            => _inner.GetLiveStreamInfo(id);

        /// <inheritdoc />
        public ILiveStream? GetLiveStreamInfoByUniqueId(string uniqueId)
            => _inner.GetLiveStreamInfoByUniqueId(uniqueId);

        /// <inheritdoc />
        public Task<Tuple<MediaSourceInfo, IDirectStreamProvider>> GetLiveStreamWithDirectStreamProvider(string id, CancellationToken cancellationToken)
            => _inner.GetLiveStreamWithDirectStreamProvider(id, cancellationToken);

        /// <inheritdoc />
        public Task<IReadOnlyList<MediaSourceInfo>> GetRecordingStreamMediaSources(ActiveRecordingInfo info, CancellationToken cancellationToken)
            => _inner.GetRecordingStreamMediaSources(info, cancellationToken);

        /// <inheritdoc />
        public Task<MediaSourceInfo> GetLiveStreamMediaInfo(string id, CancellationToken cancellationToken)
            => _inner.GetLiveStreamMediaInfo(id, cancellationToken);

        /// <inheritdoc />
        public Task CloseLiveStream(string id)
            => _inner.CloseLiveStream(id);

        /// <inheritdoc />
        public bool SupportsDirectStream(string path, MediaProtocol protocol)
            => _inner.SupportsDirectStream(path, protocol);

        /// <inheritdoc />
        public MediaProtocol GetPathProtocol(string path)
            => _inner.GetPathProtocol(path);

        /// <inheritdoc />
        public void SetDefaultAudioAndSubtitleStreamIndices(BaseItem item, MediaSourceInfo source, User user)
            => _inner.SetDefaultAudioAndSubtitleStreamIndices(item, source, user);

        /// <inheritdoc />
        public Task AddMediaInfoWithProbe(MediaSourceInfo mediaSource, bool isAudio, string? cacheKey, bool addProbeDelay, bool isLiveStream, CancellationToken cancellationToken)
            => _inner.AddMediaInfoWithProbe(mediaSource, isAudio, cacheKey, addProbeDelay, isLiveStream, cancellationToken);
    }
}
