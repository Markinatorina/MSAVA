using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MSAVA_App.Services.Api;
internal class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiService> _logger;

    public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Centralized API constants
    public const string BaseAddress = "https://localhost:7029/"; // Dev API base URL

    public static class Routes
    {
        public const string AuthLogin = "api/auth/login";
        public const string AuthRegister = "api/auth/register";
        public const string UsersMe = "api/users/me";
        public const string UsersClaims = "api/users/claims";
    }

    public HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        if (client.BaseAddress == null)
        {
            client.BaseAddress = new Uri(BaseAddress);
        }
        // Default to JSON
        if (!client.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")))
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        return client;
    }

    public HttpRequestMessage CreateJsonRequest(HttpMethod method, string relativeUrl, object? body = null, string? bearerToken = null)
    {
        var request = new HttpRequestMessage(method, relativeUrl);
        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        return request;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        return await client.SendAsync(message, cancellationToken);
    }

    public async Task<T?> SendForAsync<T>(HttpMethod method, string relativeUrl, object? body = null, string? bearerToken = null, CancellationToken cancellationToken = default)
    {
        using var msg = CreateJsonRequest(method, relativeUrl, body, bearerToken);
        using var resp = await SendAsync(msg, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("API call {Method} {Url} failed with status {Status}", method, relativeUrl, resp.StatusCode);
            return default;
        }
        try
        {
            return await resp.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize API response for {Url}", relativeUrl);
            return default;
        }
    }
}
