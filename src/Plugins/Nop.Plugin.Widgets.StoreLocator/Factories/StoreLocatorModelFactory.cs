using Nop.Plugin.Widgets.StoreLocator.Models;
using Nop.Plugin.Widgets.StoreLocator.Services;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Widgets.StoreLocator.Domain;
using Nop.Services.Media;
using Nop.Services.Helpers;
using DocumentFormat.OpenXml.EMMA;
using Org.BouncyCastle.Asn1.Pkcs;

namespace Nop.Plugin.Widgets.StoreLocator.Factories
{
    public interface IStoreLocatorModelFactory
    {
        Task<StoreLocationModel> PrepareStoreLocationModelFactory(StoreLocationModel storeLocationModel, StoreLocation value);
        Task<StoreLocationListModel> PrepareStoreLocatorListModel(StoreLocationSearchModel searchModel);
    }
    public class StoreLocatorModelFactory : IStoreLocatorModelFactory
    {
        private readonly IStoreLocatorService _storeLocatorService;
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public StoreLocatorModelFactory(IStoreLocatorService storeLocatorService,
            IPictureService pictureService,
            IDateTimeHelper dateTimeHelper)
        {
            _storeLocatorService = storeLocatorService;
            _pictureService = pictureService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<StoreLocationModel> PrepareStoreLocationModelFactory(StoreLocationModel storeLocationModel, StoreLocation storeLocation)
        {
            if (storeLocation != null)
            {
                StoreLocationModel mm = null;
                if (storeLocationModel != null)
                {
                    mm = storeLocation.ToModel(storeLocationModel);
                }
                else
                {
                    mm = storeLocation.ToModel<StoreLocationModel>();
                }
                mm.SelectedColosedDays = storeLocation.ColosedDays != null ? storeLocation.ColosedDays.Split(",").Select(int.Parse).ToList() : new List<int>();
                return await PrepareTiming(mm, storeLocation.OpeningTimeUtc, storeLocation.ClosingTimeUtc);
            }
            return storeLocationModel;
        }

        private async Task<StoreLocationModel> PrepareTiming(StoreLocationModel storeLocationModel, DateTime openTime, DateTime closingTime)
        {
            storeLocationModel.OpeningTime = await _dateTimeHelper.ConvertToUserTimeAsync((openTime), DateTimeKind.Utc);
            storeLocationModel.ClosingTime = await _dateTimeHelper.ConvertToUserTimeAsync((closingTime), DateTimeKind.Utc);
            return storeLocationModel;
        }



        public async Task<StoreLocationListModel> PrepareStoreLocatorListModel(StoreLocationSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            var storeLocators = await _storeLocatorService.GetAllStoreLocatorsAsync(searchModel.PageSize, searchModel.Page);

            //prepare list model
            var model = await new StoreLocationListModel().PrepareToGridAsync(searchModel, storeLocators, () =>
            {
                return storeLocators.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<StoreLocationModel>();
                    productModel.PictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(product.PictureId, 75);

                    return productModel;
                });
            });

            return model;
        }
    }
}
