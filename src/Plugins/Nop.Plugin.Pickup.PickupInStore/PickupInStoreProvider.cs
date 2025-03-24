﻿using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Pickup.PickupInStore.Components;
using Nop.Plugin.Pickup.PickupInStore.Domain;
using Nop.Plugin.Pickup.PickupInStore.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Shipping.Tracking;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Pickup.PickupInStore;

public class PickupInStoreProvider : BasePlugin, IPickupPointProvider, IWidgetPlugin
{
    #region Fields

    protected readonly IAddressService _addressService;
    protected readonly ICountryService _countryService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly IStoreContext _storeContext;
    protected readonly IStorePickupPointService _storePickupPointService;
    protected readonly IWebHelper _webHelper;
    private readonly WidgetSettings _widgetSettings;
    private readonly ISettingService _settingService;

    public bool HideInWidgetList => false;

    #endregion

    #region Ctor

    public PickupInStoreProvider(IAddressService addressService,
        ICountryService countryService,
        ILocalizationService localizationService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        IStorePickupPointService storePickupPointService,
        IWebHelper webHelper, 
        WidgetSettings widgetSettings, 
        ISettingService settingService)
    {
        _addressService = addressService;
        _countryService = countryService;
        _localizationService = localizationService;
        _stateProvinceService = stateProvinceService;
        _storeContext = storeContext;
        _storePickupPointService = storePickupPointService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get pickup points for the address
    /// </summary>
    /// <param name="cart">Shopping Cart</param>
    /// <param name="address">Address</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the represents a response of getting pickup points
    /// </returns>
    public async Task<GetPickupPointsResponse> GetPickupPointsAsync(IList<ShoppingCartItem> cart, Address address)
    {
        var result = new GetPickupPointsResponse();
        var store = await _storeContext.GetCurrentStoreAsync();

        foreach (var point in await _storePickupPointService.GetAllStorePickupPointsAsync(store.Id))
        {
            var pointAddress = await _addressService.GetAddressByIdAsync(point.AddressId);
            if (pointAddress == null)
                continue;

            result.PickupPoints.Add(new PickupPoint
            {
                Id = point.Id.ToString(),
                Name = point.Name,
                Description = point.Description,
                Address = pointAddress.Address1,
                City = pointAddress.City,
                County = pointAddress.County,
                StateAbbreviation = (await _stateProvinceService.GetStateProvinceByAddressAsync(pointAddress))?.Abbreviation ?? string.Empty,
                CountryCode = (await _countryService.GetCountryByAddressAsync(pointAddress))?.TwoLetterIsoCode ?? string.Empty,
                ZipPostalCode = pointAddress.ZipPostalCode,
                OpeningHours = point.OpeningHours,
                PickupFee = point.PickupFee,
                DisplayOrder = point.DisplayOrder,
                ProviderSystemName = PluginDescriptor.SystemName,
                Latitude = point.Latitude,
                Longitude = point.Longitude,
                TransitDays = point.TransitDays
            });
        }

        if (!result.PickupPoints.Any())
            result.AddError(await _localizationService.GetResourceAsync("Plugins.Pickup.PickupInStore.NoPickupPoints"));

        return result;
    }

    /// <summary>
    /// Get associated shipment tracker
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the shipment tracker
    /// </returns>
    public Task<IShipmentTracker> GetShipmentTrackerAsync()
    {
        return Task.FromResult<IShipmentTracker>(null);
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PickupInStore/Configure";
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //sample pickup point
        var country = await _countryService.GetCountryByThreeLetterIsoCodeAsync("USA");
        var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync("NY", country?.Id);

        var address = new Address
        {
            Address1 = "21 West 52nd Street",
            City = "New York",
            CountryId = country?.Id,
            StateProvinceId = state?.Id,
            ZipPostalCode = "10021",
            CreatedOnUtc = DateTime.UtcNow
        };
        await _addressService.InsertAddressAsync(address);

        var pickupPoint = new StorePickupPoint
        {
            Name = "New York store",
            AddressId = address.Id,
            OpeningHours = "10.00 - 19.00",
            PickupFee = 1.99m
        };
        await _storePickupPointService.InsertStorePickupPointAsync(pickupPoint);

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }
        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Pickup.PickupInStore.AddNew"] = "Add a new pickup point",
            ["Plugins.Pickup.PickupInStore.Fields.Description"] = "Description",
            ["Plugins.Pickup.PickupInStore.Fields.Description.Hint"] = "Specify a description of the pickup point.",
            ["Plugins.Pickup.PickupInStore.Fields.DisplayOrder"] = "Display order",
            ["Plugins.Pickup.PickupInStore.Fields.DisplayOrder.Hint"] = "Specify the pickup point display order.",
            ["Plugins.Pickup.PickupInStore.Fields.Latitude"] = "Latitude",
            ["Plugins.Pickup.PickupInStore.Fields.Latitude.Hint"] = "Specify a latitude (DD.dddddddd°).",
            ["Plugins.Pickup.PickupInStore.Fields.Latitude.InvalidPrecision"] = "Precision should be less then 8",
            ["Plugins.Pickup.PickupInStore.Fields.Latitude.InvalidRange"] = "Latitude should be in range -90 to 90",
            ["Plugins.Pickup.PickupInStore.Fields.Latitude.IsNullWhenLongitudeHasValue"] = "Latitude and Longitude should be specify together",
            ["Plugins.Pickup.PickupInStore.Fields.Longitude"] = "Longitude",
            ["Plugins.Pickup.PickupInStore.Fields.Longitude.Hint"] = "Specify a longitude (DD.dddddddd°).",
            ["Plugins.Pickup.PickupInStore.Fields.Longitude.InvalidPrecision"] = "Precision should be less then 8",
            ["Plugins.Pickup.PickupInStore.Fields.Longitude.InvalidRange"] = "Longitude should be in range -180 to 180",
            ["Plugins.Pickup.PickupInStore.Fields.Longitude.IsNullWhenLatitudeHasValue"] = "Latitude and Longitude should be specify together",
            ["Plugins.Pickup.PickupInStore.Fields.Name"] = "Name",
            ["Plugins.Pickup.PickupInStore.Fields.Name.Hint"] = "Specify a name of the pickup point.",
            ["Plugins.Pickup.PickupInStore.Fields.OpeningHours"] = "Opening hours",
            ["Plugins.Pickup.PickupInStore.Fields.OpeningHours.Hint"] = "Specify opening hours of the pickup point (Monday - Friday: 09:00 - 19:00 for example).",
            ["Plugins.Pickup.PickupInStore.Fields.PickupFee"] = "Pickup fee",
            ["Plugins.Pickup.PickupInStore.Fields.PickupFee.Hint"] = "Specify a fee for the shipping to the pickup point.",
            ["Plugins.Pickup.PickupInStore.Fields.Store"] = "Store",
            ["Plugins.Pickup.PickupInStore.Fields.Store.Hint"] = "A store name for which this pickup point will be available.",
            ["Plugins.Pickup.PickupInStore.Fields.TransitDays"] = "Transit days",
            ["Plugins.Pickup.PickupInStore.Fields.TransitDays.Hint"] = "The number of days of delivery of the goods to pickup point.",
            ["Plugins.Pickup.PickupInStore.NoPickupPoints"] = "No pickup points are available",
            ["Plugins.Pickup.PickupInStore.Fields.PictureThumbnailUrl"] = "Picture",
            ["Plugins.Pickup.PickupInStore.Fields.PictureId"] = "Picture",
            ["Plugins.Pickup.PickupInStore.Fields.PictureId.Hint"] = "Picture for store locator",
            ["Plugins.Pickup.PickupInStore.Fields.SelectedClosedDays"] = "Closed Days",
            ["Plugins.Pickup.PickupInStore.Fields.SelectedClosedDays.Hint"] = "Closed Days for store locator",
            ["Plugins.Pickup.PickupInStore.Public.NoStore"] = "No store found.",
            ["Plugins.Pickup.PickupInStore.Public.BrowseMenu"] = "Browse Menu",
            ["Plugins.Pickup.PickupInStore.Public.Direction"] = "Direction",
            ["Plugins.Pickup.PickupInStore.Public.PageTitle"] = "Store Locations",
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
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
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Pickup.PickupInStore");

        await base.UninstallAsync();
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> {
            PublicWidgetZones.HeaderLinksBefore
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(HeaderLinkViewComponent);
    }

    #endregion
}