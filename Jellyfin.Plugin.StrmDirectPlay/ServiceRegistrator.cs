using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.StrmDirectPlay
{
    /// <summary>
    /// Service registrator for STRM DirectPlay plugin.
    /// Registers the MediaSourceInterceptor with Jellyfin's DI container.
    /// </summary>
    public class ServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            // Register MediaSourceInterceptor as IMediaSourceProvider
            serviceCollection.AddSingleton<IMediaSourceProvider, MediaSourceInterceptor>();
        }
    }
}
