using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using VirtoCommerce.OrderBot.AutoRestClients.CartModuleApi;
using VirtoCommerce.OrderBot.Extensions;
using VirtoCommerce.OrderBot.Infrastructure;
using VirtoCommerce.OrderBot.Infrastructure.Autorest;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static void AddPlatformEndpoint(this IServiceCollection services)
        {
            var httpHandlerWithCompression = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            services.AddSingleton<VirtoCommerceApiRequestHandler>();

            services.AddSingleton<ICartModule>(provider => new CartModule(new VirtoCommerceCartRESTAPIdocumentation(provider.GetService<IOptions<PlatformEndpointOptions>>().Value.Url, provider.GetService<VirtoCommerceApiRequestHandler>(), httpHandlerWithCompression).DisableRetries().WithTimeout(provider.GetService<IOptions<PlatformEndpointOptions>>().Value.RequestTimeout)));
        }
    }
}
