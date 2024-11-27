using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.USPS.Domain;
using Nop.Plugin.Shipping.USPS.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.USPS.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class ShippingUSPSController : BasePluginController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly USPSSettings _uspsSettings;

    public static readonly IReadOnlyDictionary<string, string> _domesticServices = new Dictionary<string, string>()
    {
        ["NONE (disable all domestic services)"] = "NONE",
        ["Ground Advantage"] = "1058",
        ["First-Class Mail Letter"] = "letter",
        ["Priority Mail Express Sunday/Holiday Guarantee"] = "23",
        ["Priority Mail Express Flat-Rate Envelope Sunday/Holiday Guarantee"] = "25",
        ["Priority Mail Express Hold For Pickup"] = "2",
        ["Priority Mail Express Flat Rate Envelope Hold For Pickup"] = "27",
        ["Priority Mail Express"] = "3",
        ["Priority Mail Express Flat Rate Envelope"] = "13",
        ["Priority Mail"] = "1",
        ["Priority Mail Flat Rate Envelope"] = "16",
        ["Priority Mail Small Flat Rate Box"] = "28",
        ["Priority Mail Medium Flat Rate Box"] = "17",
        ["Priority Mail Large Flat Rate Box"] = "22",
        ["Standard Post"] = "4",
        ["Bound Printed Matter"] = "5",
        ["Media Mail Parcel"] = "6",
        ["Library Mail Parcel"] = "7"
    };

    public static readonly IReadOnlyDictionary<string, string> _internationalServices = new Dictionary<string, string>()
    {
        ["NONE (disable all international services)"] = "NONE",
        ["Global Express Guaranteed (GXG)"] = "4",
        ["USPS GXG Envelopes"] = "12",
        ["Priority Mail Express International Flat Rate Envelope"] = "10",
        ["Priority Mail International"] = "2",
        ["Priority Mail International Large Flat Rate Box"] = "11",
        ["Priority Mail International Medium Flat Rate Box"] = "9",
        ["Priority Mail International Small Flat Rate Box"] = "16",
        ["First-Class Mail International Large Envelope"] = "14",
        ["Priority Mail Express International"] = "1",
        ["Priority Mail International Flat Rate Envelope"] = "8",
        ["First-Class Package International Service"] = "15"
    };

    #endregion

    #region Ctor

    public ShippingUSPSController(ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        USPSSettings uspsSettings)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _uspsSettings = uspsSettings;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Orders.SHIPMENTS_VIEW)]
    public async Task<IActionResult> Configure()
    {
        var model = new USPSShippingModel
        {
            Url = _uspsSettings.Url,
            Username = _uspsSettings.Username,
            Password = _uspsSettings.Password,
            AdditionalHandlingCharge = _uspsSettings.AdditionalHandlingCharge
        };

        // Load Domestic service names
        var carrierServicesOfferedDomestic = _uspsSettings.CarrierServicesOfferedDomestic;

        model.AvailableCarrierServicesDomestic.AddRange(_domesticServices.Keys);

        if (!string.IsNullOrEmpty(carrierServicesOfferedDomestic))
        {
            foreach (var (service, serviceId) in _domesticServices)
            {
                // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                if (carrierServicesOfferedDomestic.Contains($"[{serviceId}]"))
                    model.CarrierServicesOfferedDomestic.Add(service);
            }
        }

        // Load Internation service names
        var carrierServicesOfferedInternational = _uspsSettings.CarrierServicesOfferedInternational;

        model.AvailableCarrierServicesInternational.AddRange(_internationalServices.Keys);

        if (!string.IsNullOrEmpty(carrierServicesOfferedInternational))
        {
            foreach (var (service, serviceId) in _internationalServices)
            {
                // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                if (carrierServicesOfferedInternational.Contains($"[{serviceId}]"))
                    model.CarrierServicesOfferedInternational.Add(service);
            }
        }

        return View("~/Plugins/Shipping.USPS/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Orders.SHIPMENTS_CREATE_EDIT_DELETE)]
    public async Task<IActionResult> Configure(USPSShippingModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        //save settings
        _uspsSettings.Url = model.Url;
        _uspsSettings.Username = model.Username;
        _uspsSettings.Password = model.Password;
        _uspsSettings.AdditionalHandlingCharge = model.AdditionalHandlingCharge;

        // Save selected Domestic services
        var carrierServicesOfferedDomestic = new StringBuilder();
        var carrierServicesDomesticSelectedCount = 0;
        if (model.CheckedCarrierServicesDomestic != null)
        {
            foreach (var cs in model.CheckedCarrierServicesDomestic)
            {
                carrierServicesDomesticSelectedCount++;

                //unselect any other services if NONE is selected
                if (_domesticServices.TryGetValue(cs ?? string.Empty, out var serviceId) && !string.IsNullOrEmpty(serviceId) && serviceId.Equals("NONE"))
                {
                    carrierServicesOfferedDomestic.Clear().AppendFormat("[{0}]:", serviceId);
                    break;
                }

                if (!string.IsNullOrEmpty(serviceId))
                {
                    // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                    carrierServicesOfferedDomestic.AppendFormat("[{0}]:", serviceId);
                }
            }
        }
        // Add default options if no services were selected
        if (carrierServicesDomesticSelectedCount == 0)
            _uspsSettings.CarrierServicesOfferedDomestic = "[1]:[3]:[4]:";
        else
            _uspsSettings.CarrierServicesOfferedDomestic = carrierServicesOfferedDomestic.ToString();

        // Save selected International services
        var carrierServicesOfferedInternational = new StringBuilder();
        var carrierServicesInternationalSelectedCount = 0;
        if (model.CheckedCarrierServicesInternational != null)
        {
            foreach (var cs in model.CheckedCarrierServicesInternational)
            {
                carrierServicesInternationalSelectedCount++;

                // unselect other services if NONE is selected
                if (_internationalServices.TryGetValue(cs ?? string.Empty, out var serviceId) && !string.IsNullOrEmpty(serviceId) && serviceId.Equals("NONE"))
                {
                    carrierServicesOfferedInternational.Clear().AppendFormat("[{0}]:", serviceId);
                    break;
                }

                if (!string.IsNullOrEmpty(serviceId))
                {
                    // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                    carrierServicesOfferedInternational.AppendFormat("[{0}]:", serviceId);
                }
            }
        }
        // Add default options if no services were selected
        if (carrierServicesInternationalSelectedCount == 0)
            _uspsSettings.CarrierServicesOfferedInternational = "[2]:[15]:[1]:";
        else
            _uspsSettings.CarrierServicesOfferedInternational = carrierServicesOfferedInternational.ToString();

        await _settingService.SaveSettingAsync(_uspsSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}
