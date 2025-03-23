using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Core;
using Nop.Plugin.Widgets.StoreLocator.Factories;
using Nop.Plugin.Widgets.StoreLocator.Models;
using Nop.Plugin.Widgets.StoreLocator.Services;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Widgets.StoreLocator.Domain;
using DocumentFormat.OpenXml.Office2010.Excel;
using MimeKit.Cryptography;
using DocumentFormat.OpenXml.EMMA;
using Nop.Services.Helpers;

namespace Nop.Plugin.Widgets.StoreLocator.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class StoreLocatorController : BasePluginController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPictureService _pictureService;
    private readonly IStoreLocatorService _storeLocatorService;
    private readonly IStoreLocatorModelFactory _storeLocatorModelFactory;
    private readonly IDateTimeHelper _dateTimeHelper;

    #endregion

    #region Ctor

    public StoreLocatorController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPictureService pictureService,
        IStoreLocatorService storeLocatorService,
        IStoreLocatorModelFactory storeLocatorModelFactory,
        IDateTimeHelper dateTimeHelper
        )
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _pictureService = pictureService;
        _storeLocatorService = storeLocatorService;
        _storeLocatorModelFactory = storeLocatorModelFactory;
        _dateTimeHelper = dateTimeHelper;
    }

    #endregion

    #region Utilities

    public IActionResult Index()
    {
        return RedirectToAction("List");
    }

    #endregion

    #region Methods
    private string GetView(string v)
    {
        return $"~/Plugins/Widgets.StoreLocator/Views/StoreLocator/{v}.cshtml";
    }
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> List()
    {
        var model = new StoreLocationSearchModel();
        model.SetGridPageSize();
        return View(GetView("List"), model);
    }
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    [HttpPost]
    public async Task<IActionResult> List(StoreLocationSearchModel searchModel)
    {
        //prepare model
        var model = await _storeLocatorModelFactory.PrepareStoreLocatorListModel(searchModel);
        return Json(model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public virtual async Task<IActionResult> Create()
    {
        var model = await this._storeLocatorModelFactory.PrepareStoreLocationModelFactory(new StoreLocationModel(), null);
        return View(GetView("Create"), model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Create(StoreLocationModel model, bool continueEditing)
    {

        if (ModelState.IsValid)
        {
            //product
            var storeLocation = model.ToEntity<StoreLocation>();


            var newOpeningTime = new DateTime(2000, 1, 1, model.OpeningTime.Hour, model.OpeningTime.Minute, model.OpeningTime.Second);
            storeLocation.OpeningTimeUtc = _dateTimeHelper.ConvertToUtcTime(newOpeningTime, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var newClosingTime = new DateTime(2000, 1, 1, model.ClosingTime.Hour, model.ClosingTime.Minute, model.ClosingTime.Second);
            storeLocation.ClosingTimeUtc = _dateTimeHelper.ConvertToUtcTime(newClosingTime, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            storeLocation.ColosedDays = string.Join(",", model.SelectedColosedDays);
            await _storeLocatorService.InsertStoreLocation(storeLocation);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = storeLocation.Id });
        }

        //prepare model
        model = await this._storeLocatorModelFactory.PrepareStoreLocationModelFactory(model, null);

        //if we got this far, something failed, redisplay form
        return View(GetView("Create"), model);
    }

    [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        //try to get a product with the specified id
        var storeLocation = await _storeLocatorService.GetStoreLocationById(id);
        if (storeLocation == null)
            return RedirectToAction("List");

        var model = await _storeLocatorModelFactory.PrepareStoreLocationModelFactory(null, storeLocation);

        return View(GetView("Edit"), model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Edit(StoreLocationModel model, bool continueEditing)
    {
        //try to get a product with the specified id
        var storeLocation = await _storeLocatorService.GetStoreLocationById(model.Id);
        if (storeLocation == null)
            return RedirectToAction("List");


        if (ModelState.IsValid)
        {

            storeLocation = model.ToEntity(storeLocation);

            var newOpeningTime = new DateTime(2000, 1, 1, model.OpeningTime.Hour, model.OpeningTime.Minute, model.OpeningTime.Second);
            storeLocation.OpeningTimeUtc = _dateTimeHelper.ConvertToUtcTime(newOpeningTime, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var newClosingTime = new DateTime(2000, 1, 1, model.ClosingTime.Hour, model.ClosingTime.Minute, model.ClosingTime.Second);
            storeLocation.ClosingTimeUtc = _dateTimeHelper.ConvertToUtcTime(newClosingTime, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            storeLocation.ColosedDays = string.Join(",", model.SelectedColosedDays);
            await this._storeLocatorService.UpdateStoreLocationAsync(storeLocation);


            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = storeLocation.Id });
        }

        //prepare model
        model = await this._storeLocatorModelFactory.PrepareStoreLocationModelFactory(model, storeLocation);

        //if we got this far, something failed, redisplay form
        return View(GetView("Edit"), model);
    }


    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (selectedIds == null || !selectedIds.Any())
            return NoContent();

        await _storeLocatorService.DeleteStoreLocationAsync(selectedIds.ToArray());

        return Json(new { Result = true });
    }
    #endregion
}