using System;
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
        public MediaSourceInterceptor(ILogger<MediaSourceInterceptor> logger)
        {
            _logger = logger;
            _urlReader = new StrmUrlReader(logger);
            _domainMatcher = new DomainMatcher(logger);
            _interceptor = new PlaybackInterceptor(logger, _urlReader, _domainMatcher);
        }

        /// <inheritdoc />
        public async Task<MediaSourceInfo> GetMediaSource(BaseItem item, string mediaSourceId, CancellationToken cancellationToken)
        {
            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                throw new InvalidOperationException("Plugin configuration not available");
            }

            var mediaSources = item.GetMediaSources(false);
            var originalSource = mediaSources.FirstOrDefault(i => string.Equals(i.Id, mediaSourceId, StringComparison.OrdinalIgnoreCase));

            if (originalSource == null)
            {
                throw new InvalidOperationException($"Media source not found: {mediaSourceId}");
            }

            var modifiedSource = await _interceptor.ProcessMediaSourceAsync(item, originalSource, config)
                .ConfigureAwait(false);

            return modifiedSource ?? originalSource;
        }
    }
}
