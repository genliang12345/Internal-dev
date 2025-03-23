using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.StoreLocator.Models
{
    public record PublicStoreModel : BaseNopEntityModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int PictureId { get; set; }
        public string PictureUrl { get; set; }
        public bool IsActive { get; set; }
        public string ColosedDays { get; set; }

        // Store open and close times as TimeSpan in UTC
        public DateTime OpeningTimeUtc { get; set; }
        public DateTime ClosingTimeUtc { get; set; }
        public string CloseStatus { get; set; }
        public string Timing { get;  set; }
        public string CloseClass { get; internal set; }

        public bool IsClosedToday(DayOfWeek dayOfWeek)
        {
            if (!string.IsNullOrEmpty(this.ColosedDays))
            {
                return ColosedDays
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse).Contains((int)dayOfWeek);
            }
            return false;
        }
    }
}
