using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Plugin.Shipping.USPS.Domain;

namespace Nop.Plugin.Shipping.USPS.Services;

public class USPSHttpClient
{
    #region Constants

    private const string RATES_API_KEY_INTERNATIONAL = "IntlRateV2";
    private const string RATES_API_KEY_DOMESTIC = "RateV4";

    #endregion

    #region Fields

    private readonly HttpClient _httpClient;

    #endregion

    #region Ctor

    public USPSHttpClient(HttpClient client, USPSSettings uspsSettings)
    {
        //configure client
        client.BaseAddress = new Uri(uspsSettings.Url ?? USPSShippingDefaults.DEFAULT_URL);
        client.Timeout = TimeSpan.FromSeconds(uspsSettings.ClientTimeout ?? 10);
        client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"nopCommerce-{NopVersion.CURRENT_VERSION}");
        client.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationXml);

        _httpClient = client;
    }

    #endregion

    #region Methods

    public async Task<TrackInfo> GetTrackEventsAsync(string requestString)
    {
        var stream = await _httpClient.GetStreamAsync($"?API=TrackV2&XML={requestString}");
        return await TrackInfo.LoadAsync(stream);
    }

    public async Task<RateResponse> GetRatesAsync(string requestString, bool isDomestic = true)
    {
        var apiKey = isDomestic ? RATES_API_KEY_DOMESTIC : RATES_API_KEY_INTERNATIONAL;

        var responseStream = await _httpClient.GetStreamAsync($"?API={apiKey}&XML={requestString}");

        return await RateResponse.LoadAsync(responseStream, isDomestic);
    }

    public async Task<TransitResponse> GetTransitTimeAsync(TransitDaysAPI api, string requestString)
    {
        var responseStream = await _httpClient.GetStreamAsync($"?API={api}&XML={requestString}");
        return await TransitResponse.LoadAsync(responseStream);
    }

    #endregion
}
