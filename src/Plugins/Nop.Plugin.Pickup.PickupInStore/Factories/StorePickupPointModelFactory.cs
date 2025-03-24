using Nop.Core.Domain.Shipping;
using Nop.Plugin.Pickup.PickupInStore.Domain;
using Nop.Plugin.Pickup.PickupInStore.Models;
using Nop.Plugin.Pickup.PickupInStore.Services;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Pickup.PickupInStore.Factories;

/// <summary>
/// Represents store pickup point models factory implementation
/// </summary>
public class StorePickupPointModelFactory : IStorePickupPointModelFactory
{
    #region Fields

    protected readonly IStorePickupPointService _storePickupPointService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IStoreService _storeService;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    public StorePickupPointModelFactory(IStorePickupPointService storePickupPointService,
        ILocalizationService localizationService, IStoreService storeService,
        IPictureService pictureService)
    {
        _storePickupPointService = storePickupPointService;
        _localizationService = localizationService;
        _storeService = storeService;
        _pictureService = pictureService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare store pickup point list model
    /// </summary>
    /// <param name="searchModel">Store pickup point search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the store pickup point list model
    /// </returns>
    public async Task<StorePickupPointListModel> PrepareStorePickupPointListModelAsync(StorePickupPointSearchModel searchModel)
    {
        var pickupPoints = await _storePickupPointService.GetAllStorePickupPointsAsync(pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);
        var model = await new StorePickupPointListModel().PrepareToGridAsync(searchModel, pickupPoints, () =>
        {
            return pickupPoints.SelectAwait(async point =>
            {
                var store = await _storeService.GetStoreByIdAsync(point.StoreId);

                return new StorePickupPointModel
                {
                    Id = point.Id,
                    Name = point.Name,
                    OpeningHours = point.OpeningHours,
                    PickupFee = point.PickupFee,
                    DisplayOrder = point.DisplayOrder,

                    StoreName = store?.Name
                                ?? (point.StoreId == 0 ? (await _localizationService.GetResourceAsync("Admin.Configuration.Settings.StoreScope.AllStores")) : string.Empty),
                    PictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(point.PictureId, 75)


                };
            });
        });

        return model;
    }

    /// <summary>
    /// Prepare store pickup point search model
    /// </summary>
    /// <param name="searchModel">Store pickup point search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the store pickup point search model
    /// </returns>
    public Task<StorePickupPointSearchModel> PrepareStorePickupPointSearchModelAsync(StorePickupPointSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    public async Task PrepareStorePickupPointModelAsync(StorePickupPointModel storeLocationModel, StorePickupPoint storeLocation)
    {
        if (storeLocation != null && storeLocationModel != null)
        {
            storeLocationModel.SelectedColosedDays = storeLocation.ClosedDays != null ? storeLocation.ClosedDays.Split(",").Select(int.Parse).ToList() : new List<int>();
            storeLocationModel.PictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(storeLocation.PictureId);
        }
    }

    #endregion
}