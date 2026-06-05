using System;
using System.Linq;
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
            // Decorate IMediaSourceManager with our STRM-aware implementation.
            // Do this manually instead of using Scrutor so the plugin does not
            // require Scrutor.dll to be present in Jellyfin's plugin directory.
            var descriptor = serviceCollection.LastOrDefault(d => d.ServiceType == typeof(IMediaSourceManager));
            if (descriptor == null)
            {
                return;
            }

            serviceCollection.Remove(descriptor);
            serviceCollection.Add(new ServiceDescriptor(
                typeof(IMediaSourceManager),
                serviceProvider =>
                {
                    var inner = CreateInnerMediaSourceManager(serviceProvider, descriptor);
                    return ActivatorUtilities.CreateInstance<StrmMediaSourceManager>(serviceProvider, inner);
                },
                descriptor.Lifetime));
        }

        private static IMediaSourceManager CreateInnerMediaSourceManager(IServiceProvider serviceProvider, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance is IMediaSourceManager instance)
            {
                return instance;
            }

            if (descriptor.ImplementationFactory != null)
            {
                return (IMediaSourceManager)descriptor.ImplementationFactory(serviceProvider);
            }

            if (descriptor.ImplementationType != null)
            {
                return (IMediaSourceManager)ActivatorUtilities.CreateInstance(serviceProvider, descriptor.ImplementationType);
            }

            throw new InvalidOperationException("Unable to decorate IMediaSourceManager: unsupported service descriptor.");
        }
    }
}
