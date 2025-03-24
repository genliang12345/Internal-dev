using Nop.Web.Framework.Models;

namespace Nop.Plugin.Pickup.PickupInStore.Models
{
    public record PublicStoreModel : BaseNopEntityModel
    {
        public PublicStoreModel()
        {
            SelectedColosedDays = new List<int>();
        }

        public string Address { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal PickupFee { get; set; }

        public string OpeningHours { get; set; }

        public int DisplayOrder { get; set; }

        public int StoreId { get; set; }
        public string StoreName { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public int? TransitDays { get; set; }

        public int PictureId { get; set; }

        public string CloseStatus { get; set; }
        public string Timing { get;  set; }
        public string CloseClass { get;  set; }
        public string PictureThumbnailUrl { get; set; }
        public string ClosedDays { get; set; }


        public IList<int> SelectedColosedDays { get; set; }

        public bool IsClosedToday(DayOfWeek dayOfWeek)
        {
            if (!string.IsNullOrEmpty(this.ClosedDays))
            {
                return ClosedDays
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse).Contains((int)dayOfWeek);
            }
            return false;
        }
    }
}
