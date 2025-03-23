using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.StoreLocator.Components;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.StoreLocator;

/// <summary>
/// Represents swiper widget
/// </summary>
public class StoreLocatorPlugin : BasePlugin, IWidgetPlugin
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INopFileProvider _fileProvider;
    protected readonly IPictureService _pictureService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public StoreLocatorPlugin(ILocalizationService localizationService,
        INopFileProvider fileProvider,
        IPictureService pictureService,
        ISettingService settingService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _fileProvider = fileProvider;
        _pictureService = pictureService;
        _settingService = settingService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> {
            //PublicWidgetZones.HeaderLinksBefore 

            PublicWidgetZones.HeaderLinksBefore
        });
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/StoreLocator/List";
    }

    /// <summary>
    /// Gets a name of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component name</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(HeaderLinkViewComponent);
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //pictures
        var sampleImagesPath = _fileProvider.MapPath("~/Plugins/Widgets.StoreLocator/Content/sample-images/");

        //settings

        

       
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Widgets.StoreLocator.Admin.PageTitle"] = "Store Locations",
            ["Plugins.Widgets.StoreLocator.Name.Required"] = "Name is required.",
            ["Plugins.Widgets.StoreLocator.Picture.Required"] = "Picture is required.",
            ["Plugins.Widgets.StoreLocator.Lat.Required"] = "Latitude is required.",
            ["Plugins.Widgets.StoreLocator.Lon.Required"] = "Longitude is required.",

            ["Plugins.Widgets.StoreLocator.Public.Required"] = "No store found.",
            ["Plugins.Widgets.StoreLocator.Public.Direction"] = "Direction",
            ["Plugins.Widgets.StoreLocator.Public.BrowseMenu"] = "Browse Menu",

            ["Plugins.Widgets.StoreLocator.Admin.Name"] = "Store Name",
            ["Plugins.Widgets.StoreLocator.Admin.Address"] = "Store Address",
            ["Plugins.Widgets.StoreLocator.Admin.Latitude"] = "Latitude",
            ["Plugins.Widgets.StoreLocator.Admin.Longitude"] = "Longitude",
            ["Plugins.Widgets.StoreLocator.Admin.PictureId"] = "Store Picture",
            ["Plugins.Widgets.StoreLocator.Admin.SelectedClosedDays"] = "Closed Days",
            ["Plugins.Widgets.StoreLocator.Admin.PictureThumbnailUrl"] = "Picture",
            ["Plugins.Widgets.StoreLocator.Admin.OpeningTime"] = "Opening Time",
            ["Plugins.Widgets.StoreLocator.Admin.ClosingTime"] = "Closing Time",
            ["Plugins.Widgets.StoreLocator.Admin.IsActive"] = "Is Active"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDescriptor.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.StoreLocator");

        await base.UninstallAsync();
    }


    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;

    #endregion
}