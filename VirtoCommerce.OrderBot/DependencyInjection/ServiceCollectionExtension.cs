using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using VirtoCommerce.OrderBot.AutoRestClients.CartModuleApi;
using VirtoCommerce.OrderBot.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.OrderBot.AutoRestClients.CustomerModuleApi;
using VirtoCommerce.OrderBot.Bots.Dialogs;
using VirtoCommerce.OrderBot.Bots.Dialogs.DialogInjector;
using VirtoCommerce.OrderBot.Bots.Dialogs.DialogInjector.Handlers;
using VirtoCommerce.OrderBot.Bots.Middlewares;
using VirtoCommerce.OrderBot.Bots.Middlewares.Injector;
using VirtoCommerce.OrderBot.Extensions;
using VirtoCommerce.OrderBot.Infrastructure;
using VirtoCommerce.OrderBot.Infrastructure.Autorest;
using VirtoCommerce.OrderBot.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static void AddAutoRestClients(this IServiceCollection services)
        {
            var httpHandlerWithCompression = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            services.AddSingleton<VirtoCommerceApiRequestHandler>();

            services.AddSingleton<ICartModule>(provider => new CartModule(new VirtoCommerceCartRESTAPIdocumentation(provider.GetService<IOptions<PlatformEndpointOptions>>().Value.Url, provider.GetService<VirtoCommerceApiRequestHandler>(), httpHandlerWithCompression).DisableRetries().WithTimeout(provider.GetService<IOptions<PlatformEndpointOptions>>().Value.RequestTimeout)));
            services.AddSingleton<ICustomerModule>(provider => new CustomerModule(new VirtoCommerceCustomerRESTAPIdocumentation(provider.GetService<IOptions<PlatformEndpointOptions>>().Value.Url, provider.GetService<VirtoCommerceApiRequestHandler>(), httpHandlerWithCompression).DisableRetries().WithTimeout(provider.GetService<IOptions<PlatformEndpointOptions>>().Value.RequestTimeout)));
            services.AddSingleton<ICatalogModuleSearch>(provider => new CatalogModuleSearch(new VirtoCommerceCatalogRESTAPIdocumentation(provider.GetService<IOptions<PlatformEndpointOptions>>().Value.Url, provider.GetService<VirtoCommerceApiRequestHandler>(), httpHandlerWithCompression).DisableRetries().WithTimeout(provider.GetService<IOptions<PlatformEndpointOptions>>().Value.RequestTimeout)));
        }

        public static IServiceCollection AddBotServices(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<IUserAuthState>(provider => provider.GetService<IAuthorizationService>());
            services.AddSingleton<IMessageInterceptor, MessageInterceptor>();

            services.AddSingleton<AddToCartMessageHandler>();

            services.AddSingleton<MessageHandlerKeeper>();
            services.AddSingleton<IMessageHandlerReciever>(provider => provider.GetService<MessageHandlerKeeper>());
            services.AddSingleton<IMessageHandlerStorage>(provider => provider.GetService<MessageHandlerKeeper>());

            services.AddSingleton<MainDialog>();
            services.AddSingleton<AuthDialog>();
            services.AddSingleton<CatalogDialog>();
            services.AddSingleton<SearchDialog>();
            services.AddSingleton<AddToCartDialog>();

            return services;
        }

        public static IServiceCollection AddMiddlewares(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<SampleMiddleware>();

            return serviceCollection;
        }

        public static IApplicationBuilder AddMiddlewares(this IApplicationBuilder appBuilder)
        {
            var serviceLocator = appBuilder.ApplicationServices;

            serviceLocator
                .GetService<IMiddlewareInjector>()
                .AddMiddleware(serviceLocator.GetService<SampleMiddleware>());

            return appBuilder;
        }

        public static IApplicationBuilder AddInterceptors(this IApplicationBuilder appBuilder)
        {
            var serviceLocator = appBuilder.ApplicationServices;

            serviceLocator
                .GetService<IMessageHandlerStorage>()
                .AddHandler(serviceLocator.GetService<AddToCartMessageHandler>());

            return appBuilder;
        }
    }
}
