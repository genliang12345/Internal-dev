using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Widgets.StoreLocator.EventHandlers
{
    public class AddAdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
    {
        private readonly IPermissionService _permissionService;

        public AddAdminMenuEventConsumer(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS))
                return;

            eventMessage.RootMenuItem.InsertBefore("Local plugins",
                new AdminMenuItem
                {
                    SystemName = "Widget.StoreLocator.Menu",
                    Title = "Store Locators",
                    Url = eventMessage.GetMenuItemUrl("StoreLocator", "List"),
                    IconClass = "far fa-dot-circle",
                    Visible = true,
                });

        }
    }
}
