using Nop.Core;

namespace Nop.Plugin.Widgets.StoreLocator.Domain;


public class StoreLocation : BaseEntity
{
    public string Name { get; set; }
    public string Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int PictureId { get; set; }
    public bool IsActive { get; set; }
    public string ColosedDays { get; set; }

    // Store open and close times as TimeSpan in UTC
    public DateTime OpeningTimeUtc { get; set; }
    public DateTime ClosingTimeUtc { get; set; }
}
