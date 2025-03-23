using FluentValidation;
using Nop.Plugin.Widgets.StoreLocator.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.StoreLocator.Validators;

/// <summary>
/// Represents slide model validator
/// </summary>
public class SlidePictureValidator : BaseNopValidator<StoreLocationModel>
{
    #region Ctor

    public SlidePictureValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.PictureId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.StoreLocator.Picture.Required"));
        
        RuleFor(model => model.Name)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.StoreLocator.Name.Required")); 
        
        RuleFor(model => model.Latitude)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.StoreLocator.Lat.Required")); 
        
        RuleFor(model => model.Longitude)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.StoreLocator.Lon.Required"));
    }

    #endregion
}
