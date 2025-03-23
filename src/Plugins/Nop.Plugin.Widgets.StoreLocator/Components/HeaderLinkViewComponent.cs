using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.StoreLocator.Components
{
    public class HeaderLinkViewComponent : NopViewComponent
    {
        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            return Task.FromResult<IViewComponentResult>(View("~/Plugins/Widgets.StoreLocator/Views/Shared/Components/Default.cshtml"));
        }

        #endregion
    }
}
