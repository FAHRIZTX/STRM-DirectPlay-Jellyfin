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
    /// Media source provider for STRM files.
    /// Implements IMediaSourceProvider to intercept playback for .strm files.
    /// </summary>
    public class MediaSourceInterceptor : IMediaSourceProvider
    {
        private readonly ILogger<MediaSourceInterceptor> _logger;
        private readonly PlaybackInterceptor _interceptor;
        private readonly StrmUrlReader _urlReader;
        private readonly DomainMatcher _domainMatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSourceInterceptor"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="loggerFactory">Logger factory for creating typed loggers.</param>
        public MediaSourceInterceptor(ILogger<MediaSourceInterceptor> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _urlReader = new StrmUrlReader(loggerFactory.CreateLogger<StrmUrlReader>());
            _domainMatcher = new DomainMatcher(loggerFactory.CreateLogger<DomainMatcher>());
            _interceptor = new PlaybackInterceptor(loggerFactory.CreateLogger<PlaybackInterceptor>(), _urlReader, _domainMatcher);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MediaSourceInfo>> GetMediaSources(BaseItem item, CancellationToken cancellationToken)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                return Enumerable.Empty<MediaSourceInfo>();
            }

            if (!_urlReader.IsStrmFile(item.Path))
            {
                return Enumerable.Empty<MediaSourceInfo>();
            }

            var originalSources = item.GetMediaSources(false);

            var result = new List<MediaSourceInfo>();
            foreach (var source in originalSources)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var modified = await _interceptor.ProcessMediaSourceAsync(item, source, config)
                    .ConfigureAwait(false);

                result.Add(modified ?? source);
            }

            return result;
        }

        /// <inheritdoc />
        public Task<ILiveStream> OpenMediaSource(string openToken, List<ILiveStream> currentLiveStreams, CancellationToken cancellationToken)
        {
            // STRM files are remote HTTP streams — no live stream session needed.
            throw new NotImplementedException("STRM DirectPlay does not support live stream opening.");
        }
    }
}

