using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.StoreLocator.Factories;
using Nop.Plugin.Widgets.StoreLocator.Services;

namespace Nop.Plugin.Widgets.StoreLocator.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public int Order => 1;

        public void Configure(IApplicationBuilder application)
        {

        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IStoreLocatorService, StoreLocatorService>();
            services.AddScoped<IStoreLocatorModelFactory, StoreLocatorModelFactory>();
        }
    }
}
