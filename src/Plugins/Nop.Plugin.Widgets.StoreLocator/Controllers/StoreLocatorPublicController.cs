using System.Globalization;
using System.Threading.Tasks;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.StoreLocator.Models;
using Nop.Plugin.Widgets.StoreLocator.Services;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Controllers;

namespace Nop.Plugin.Widgets.StoreLocator.Controllers
{
    public class StoreLocatorPublicController : BasePublicController
    {
        private readonly IStoreLocatorService _storeLocatorService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;

        public StoreLocatorPublicController(IStoreLocatorService storeLocatorService,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            IPictureService pictureService)
        {
            _storeLocatorService = storeLocatorService;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
            _pictureService = pictureService;
        }
        public async Task<IActionResult> PublicView()
        {

            var customerTimezone = await _dateTimeHelper.GetCustomerTimeZoneAsync(await _workContext.GetCurrentCustomerAsync());

            var utcNow = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);

            var stores = await _storeLocatorService.GetPublicStoreLocations();
            var storesModel = await stores.SelectAwait(async x =>
              {
                  var y = x.ToModel<PublicStoreModel>();
                  y.CloseStatus = y.IsClosedToday(utcNow.DayOfWeek) ? "Closed" : "Open";
                  y.CloseClass = y.IsClosedToday(utcNow.DayOfWeek) ? "closed" : "open";
                  y.OpeningTimeUtc = _dateTimeHelper.ConvertToUserTime(x.OpeningTimeUtc,
                      TimeZoneInfo.Utc, customerTimezone);
                  y.ClosingTimeUtc = _dateTimeHelper.ConvertToUserTime(x.ClosingTimeUtc,
                      TimeZoneInfo.Utc, customerTimezone);
                  y.PictureUrl = await _pictureService.GetPictureUrlAsync(y.PictureId);

                  y.Timing = y.OpeningTimeUtc.ToString("hh:mm tt") + " - " + y.ClosingTimeUtc.ToString("hh:mm tt");
                  if (y.IsClosedToday(utcNow.DayOfWeek))
                  {
                      var values = Enum.GetValues<DayOfWeek>().Cast<int>().ToList();
                      var closedDay = y.ColosedDays.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
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
                  return y;
              }).ToListAsync();

            return View("~/Plugins/Widgets.StoreLocator/Views/StoreLocatorPublic/PublicView.cshtml", storesModel);
        }

        static int GetNextAvailableDay(int currentDay, List<int> availableDays)
        {
            for (int i = 1; i <= 7; i++) // Loop through the next 7 days to prevent infinite loops
            {
                int nextDay = (currentDay + i) % 7; // Move to the next day and loop back after Sunday
                if (availableDays.Contains(nextDay))
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
    }
}
