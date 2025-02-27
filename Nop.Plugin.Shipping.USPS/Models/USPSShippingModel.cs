﻿using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.USPS.Models;

public record USPSShippingModel : BaseNopModel
{
    public USPSShippingModel()
    {
        CarrierServicesOfferedDomestic = new List<string>();
        AvailableCarrierServicesDomestic = new List<string>();
        CarrierServicesOfferedInternational = new List<string>();
        AvailableCarrierServicesInternational = new List<string>();
    }

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.Url")]
    public string Url { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.Username")]
    public string Username { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.Password")]

    [NoTrim]
    public string Password { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.AdditionalHandlingCharge")]
    public decimal AdditionalHandlingCharge { get; set; }

    public IList<string> CarrierServicesOfferedDomestic { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.AvailableCarrierServicesDomestic")]
    public IList<string> AvailableCarrierServicesDomestic { get; set; }

    public string[] CheckedCarrierServicesDomestic { get; set; }

    public IList<string> CarrierServicesOfferedInternational { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.AvailableCarrierServicesInternational")]
    public IList<string> AvailableCarrierServicesInternational { get; set; }

    public string[] CheckedCarrierServicesInternational { get; set; }
}