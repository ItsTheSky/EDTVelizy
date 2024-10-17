using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EDTVelizy.Core;

/// <summary>
/// Utility methods for handling HTTP requests. You can
/// specify a custom HTTP client to use for requests here.
/// </summary>
public static class RequestUtils
{
    
    private static HttpClient _client = new ();
    
    /// <summary>
    /// Change the HTTP client used for requests, for example
    /// to use a custom client with a specific timeout.
    /// </summary>
    /// <param name="client"></param>
    public static void SetHttpClient(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Send a POST request to the specified endpoint.
    /// The actual URL will be built using <see cref="Constants.BaseUrl"/>.
    /// </summary>
    /// <param name="endpoint">The endpoint to send the request to.</param>
    /// <param name="content">The content to send in the request body. The given object must be JSON-serializable</param>
    /// <returns>The response from the server.</returns>
    public static async Task<HttpResponseMessage> PostAsync(string endpoint, object content, bool useDefaultHeaders = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Constants.BaseUrl + endpoint);

        if (useDefaultHeaders)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.01));
        
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));
        
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr-FR", 1));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr", 0.8));   
        }

        var parameters = JsonUtils.ToDictionary(content);
        request.Content = new FormUrlEncodedContent(parameters);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        return await _client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> GetAsync(string endpoint, object? parameters, bool useDefaultHeaders = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, Constants.BaseUrl + endpoint);
        
        if (useDefaultHeaders)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.01));
        
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));
        
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr-FR", 1));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr", 0.8));   
        }

        var query = new StringBuilder();
        foreach (var (key, value) in JsonUtils.ToDictionary(parameters))
        {
            query.Append($"{key}={value}&");
        }
        request.RequestUri = new Uri(request.RequestUri + "?" + query.ToString().TrimEnd('&'));

        return await _client.SendAsync(request);
    }
    
}