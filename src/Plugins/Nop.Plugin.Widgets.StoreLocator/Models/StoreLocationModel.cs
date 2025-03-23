using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.StoreLocator.Models;

/// <summary>
/// Represents a slide model on the site
/// </summary>
public record StoreLocationModel : BaseNopEntityModel
{
    public StoreLocationModel()
    {
        SelectedColosedDays = new List<int>();
    }
    #region Properties
    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.Address")]
    public string Address { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.Latitude")]
    public decimal? Latitude { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.Longitude")]
    public decimal? Longitude { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.PictureId")]
    [UIHint("Picture")]
    public int PictureId { get; set; }


    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.PictureThumbnailUrl")]
    public string PictureThumbnailUrl { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.OpeningTime")]
    public DateTime OpeningTime { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.ClosingTime")]
    public DateTime ClosingTime { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.IsActive")]

    public bool IsActive { get; set; }
    [NopResourceDisplayName("Plugins.Widgets.StoreLocator.Admin.SelectedClosedDays")]
    public IList<int> SelectedColosedDays { get; set; }



    #endregion
}
