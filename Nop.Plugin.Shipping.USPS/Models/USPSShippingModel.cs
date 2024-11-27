using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.USPS.Models;

public record USPSShippingModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.Url")]
    public string Url { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.Username")]
    public string Username { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.Password")]

    [NoTrim]
    public string Password { get; set; }

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.AdditionalHandlingCharge")]
    public decimal AdditionalHandlingCharge { get; set; }

    public List<string> CarrierServicesOfferedDomestic { get; set; } = new();

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.AvailableCarrierServicesDomestic")]
    public List<string> AvailableCarrierServicesDomestic { get; set; } = new();

    public string[] CheckedCarrierServicesDomestic { get; set; }

    public List<string> CarrierServicesOfferedInternational { get; set; } = new();

    [NopResourceDisplayName("Plugins.Shipping.USPS.Fields.AvailableCarrierServicesInternational")]
    public List<string> AvailableCarrierServicesInternational { get; set; } = new();

    public string[] CheckedCarrierServicesInternational { get; set; }
}