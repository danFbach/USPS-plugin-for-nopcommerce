using System.Xml.Linq;
using Nop.Plugin.Shipping.USPS.Domain.Extensions;

namespace Nop.Plugin.Shipping.USPS.Domain;

public class TransitResponse
{
    #region Ctor

    public TransitResponse() { }

    public TransitResponse(XElement transit)
    {
        if (transit == null)
            return;

        OriginZip = transit.GetValueOfXMLElement("OriginZip");
        DestinationZip = transit.GetValueOfXMLElement("DestinationZip");
        Days = transit.GetValueOfXMLElement<int>("Days");
        IsGuaranteed = transit.GetValueOfXMLElement("IsGuaranteed");
        Message = transit.GetValueOfXMLElement("Message");
        EffectiveAcceptanceDate = transit.GetValueOfXMLElement("EffectiveAcceptanceDate");
        ScheduledDeliveryDate = transit.GetValueOfXMLElement("ScheduledDeliveryDate");
    }

    #endregion

    #region Methods

    /// <summary>
    /// Load rates from the passed stream
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="isDomestic">Value indicating whether the information is domestic package</param>
    /// <returns>The asynchronous task whose result contains the RSS feed</returns>
    public static async Task<TransitResponse> LoadAsync(Stream stream)
    {
        var response = new TransitResponse();

        try
        {
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, default);

            if (document?.Root is null)
                return response;

            response = new TransitResponse(document?.Root);

            return response;
        }
        catch
        {
            return response;
        }
    }

    #endregion

    #region Properties

    public string OriginZip { get; set; }
    public string DestinationZip { get; set; }
    public int? Days { get; set; }
    public string IsGuaranteed { get; set; }
    public string Message { get; set; }
    public string EffectiveAcceptanceDate { get; set; }
    public string ScheduledDeliveryDate { get; set; }

    #endregion
}
