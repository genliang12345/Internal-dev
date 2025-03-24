using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Plugin.Pickup.PickupInStore.Models;
using Nop.Plugin.Pickup.PickupInStore.Services;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Controllers;

namespace Nop.Plugin.Pickup.PickupInStore.Controllers
{
    public class StoreLocatorPublicController : BasePublicController
    {
        private readonly IStorePickupPointService _storePickupPointService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly IStoreContext _storeContext;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;

        public StoreLocatorPublicController(IStorePickupPointService storePickupPointService,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            IPictureService pictureService,
            IStoreContext storeContext,
            IAddressService addressService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService)
        {
            _storePickupPointService = storePickupPointService;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
            _pictureService = pictureService;
            _storeContext = storeContext;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
        }
        public async Task<IActionResult> PublicView()
        {
            var utcNow = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);

            var stores = await _storePickupPointService.GetAllStorePickupPointsAsync((await _storeContext.GetCurrentStoreAsync()).Id, 0, int.MaxValue);
            var storesModel = await stores.SelectAwait(async x =>
              {
                  var y = x.ToModel<PublicStoreModel>();
                  y.CloseStatus = y.IsClosedToday(utcNow.DayOfWeek) ? "Closed" : "Open";
                  y.CloseClass = y.IsClosedToday(utcNow.DayOfWeek) ? "closed" : "open";
                  y.PictureThumbnailUrl = await _pictureService.GetPictureUrlAsync(y.PictureId);

                  y.Timing = x.OpeningHours;
                  if (y.IsClosedToday(utcNow.DayOfWeek))
                  {
                      var values = Enum.GetValues<DayOfWeek>().Cast<int>().ToList();
                      var closedDay = y.ClosedDays.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                      var nextDay = GetNextAvailableDay((int)utcNow.DayOfWeek, closedDay);
                      if (nextDay > -1)
                      {
                          y.Timing = "Opens " + GetDayAbbreviation(nextDay) + " " + y.Timing;
                      }
                      else
                      {
                          y.Timing = "";
                      }
                  }
                  var address = await _addressService.GetAddressByIdAsync(x.AddressId);
                  if (address != null)
                      y.Address = await GetAddress(address);

                  return y;
              }).ToListAsync();

            return View("~/Plugins/Pickup.PickupInStore/Views/StoreLocatorPublic/PublicView.cshtml", storesModel);
        }



        static int GetNextAvailableDay(int currentDay, List<int> availableDays)
        {
            for (int i = 1; i <= 7; i++) // Loop through the next 7 days to prevent infinite loops
            {
                int nextDay = (currentDay + i) % 7; // Move to the next day and loop back after Sunday
                if (!availableDays.Contains(nextDay))
                {
                    return nextDay;
                }
            }
            return -1; // Return -1 if no available day is found (shouldn't happen in normal cases)
        }
        static string GetDayAbbreviation(int dayInt)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek)dayInt);
        }

        private async Task<string> GetAddress(Address address)
        {
            var province = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0);
            var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
            var aa = new List<string>
            {
                 address.Address1,
                address.City,
             address.County,
                address.ZipPostalCode,
                province?.Name,
                country?.Name,
            };

            var ss = new StringBuilder();
            foreach (var item in aa)
            {
                if (!string.IsNullOrEmpty(item))
                    ss.Append(item.Trim() + " ");
            }
            return ss.ToString();
        }
    }
}
