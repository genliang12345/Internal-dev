using System.Globalization;
using System.Text;
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
    [AutoValidateAntiforgeryToken]
    public class StoreLocatorPublicController : BasePublicController
    {
        private readonly IStorePickupPointService _storePickupPointService;
        private readonly IDateTimeHelper _dateTimeHelper;
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
            _pictureService = pictureService;
            _storeContext = storeContext;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
        }
        [HttpGet]
        public async Task<IActionResult> PublicView()
        {

            return View("~/Plugins/Pickup.PickupInStore/Views/StoreLocatorPublic/PublicView.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> FetchPickups(string q, double? latitude, double? longitude)
        {
            var utcNow = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);

            var stores = await _storePickupPointService.GetAllStorePickupPointsAsync((await _storeContext.GetCurrentStoreAsync()).Id, 0, int.MaxValue, q: q);
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

            if (latitude.HasValue && longitude.HasValue)
            {
                storesModel = storesModel.Select(p =>
                {
                    if (!p.Latitude.HasValue || !p.Longitude.HasValue)
                    {
                        p.Latitude = 0;
                        p.Longitude = 0;
                    }
                    return new
                    {
                        PickupPoint = p,
                        Distance = CalculateDistance(latitude.Value, longitude.Value, p.Latitude.Value, p.Longitude.Value)
                    };
                })
                .OrderBy(p => p.Distance)
                .Select(p => p.PickupPoint).ToList();
            }

            return PartialView("~/Plugins/Pickup.PickupInStore/Views/StoreLocatorPublic/_PublicPickupPointsCards.cshtml", storesModel);
        }



        private static int GetNextAvailableDay(int currentDay, List<int> availableDays)
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
        private static string GetDayAbbreviation(int dayInt)
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
        private double CalculateDistance(double lat1, double lon1, decimal lat2, decimal lon2)
        {
            const double earthRadiusKm = 6371; // Earth's radius in km
            double lat2D = Convert.ToDouble(lat2);
            double lon2D = Convert.ToDouble(lon2);
            double dLat = ToRadians(lat2D - lat1);
            double dLon = ToRadians(lon2D - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2D)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c; // Distance in kilometers
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
