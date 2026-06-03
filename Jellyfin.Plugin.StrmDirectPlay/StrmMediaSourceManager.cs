using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.StrmDirectPlay.Core;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
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
        public async Task<List<MediaSourceInfo>> GetPlaybackMediaSources(BaseItem item, User user, bool allowMediaProbe, bool enablePathSubstitution, CancellationToken cancellationToken)
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

            // Process each source
            for (int i = 0; i < sources.Count; i++)
            {
                var modified = await _interceptor.ProcessMediaSourceAsync(item, sources[i], config).ConfigureAwait(false);
                if (modified != null)
                {
                    sources[i] = modified;
                }
            }

            return sources;
        }

        // Delegate all other methods to inner implementation
        /// <inheritdoc />
        public Task<MediaSourceInfo> GetMediaSource(BaseItem item, string mediaSourceId, bool enablePathSubstitution, CancellationToken cancellationToken)
            => _inner.GetMediaSource(item, mediaSourceId, enablePathSubstitution, cancellationToken);

        /// <inheritdoc />
        public Task<LiveStreamResponse> OpenLiveStream(LiveStreamRequest request, CancellationToken cancellationToken)
            => _inner.OpenLiveStream(request, cancellationToken);

        /// <inheritdoc />
        public Task<ILiveStream> GetLiveStream(string id, CancellationToken cancellationToken)
            => _inner.GetLiveStream(id, cancellationToken);

        /// <inheritdoc />
        public Task<Tuple<MediaSourceInfo, IDirectStreamProvider>> GetLiveStreamWithDirectStreamProvider(string id, CancellationToken cancellationToken)
            => _inner.GetLiveStreamWithDirectStreamProvider(id, cancellationToken);

        /// <inheritdoc />
        public Task CloseLiveStream(string id)
            => _inner.CloseLiveStream(id);
    }
}
