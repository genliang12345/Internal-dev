using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Pickup.PickupInStore.Components
{
    public class HeaderLinkViewComponent : NopViewComponent
    {
        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            return Task.FromResult<IViewComponentResult>(View("~/Plugins/Pickup.PickupInStore/Views/Shared/Components/HeaderLink/Default.cshtml"));
        }

        #endregion
    }
}
