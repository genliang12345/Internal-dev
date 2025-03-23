﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Widgets.StoreLocator.Infrastructure
{
    public class PluginRouterProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => 1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
           
            endpointRouteBuilder.MapControllerRoute(name: "Nop.Plugin.Widgets.StoreLocator",
          pattern: "store-locations",
          defaults: new { controller = "StoreLocatorPublic", action = "PublicView" });


        }
    }
}
