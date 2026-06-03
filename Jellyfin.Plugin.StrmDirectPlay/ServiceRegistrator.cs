using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jellyfin.Plugin.StrmDirectPlay
{
    /// <summary>
    /// Service registrator for STRM DirectPlay plugin.
    /// Decorates IMediaSourceManager to intercept STRM file playback.
    /// </summary>
    public class ServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            // Decorate IMediaSourceManager with our STRM-aware implementation
            // This intercepts GetPlaybackMediaSources calls for .strm files
            serviceCollection.Decorate<IMediaSourceManager, StrmMediaSourceManager>();
        }
    }
}
