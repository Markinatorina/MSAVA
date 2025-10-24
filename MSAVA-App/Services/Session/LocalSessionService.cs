using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSAVA_Shared.Models;
using MSAVA_App.Services.Api;

namespace MSAVA_App.Services.Session;
public class LocalSessionService
{
    private readonly ILogger<LocalSessionService> _logger;
    private readonly ApiService _api;

    private string? _accessToken;
    private SessionDTO? _session;

    public event EventHandler? LoggedOut;

    public LocalSessionService(ILogger<LocalSessionService> logger, ApiService api)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public string? AccessToken => _accessToken;
    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(_accessToken);
    public SessionDTO? CurrentSession => _session;

    public async Task<string?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required", nameof(username));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required", nameof(password));

        var http = _api.CreateClient();

        var requestBody = new LoginRequestDTO
        {
            Username = username,
            Password = password
        };

        using var msg = _api.CreateJsonRequest(HttpMethod.Post, ApiService.Routes.AuthLogin, requestBody, anonymous: true);

        try
        {
            using var resp = await http.SendAsync(msg, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Login failed with status code {StatusCode}", resp.StatusCode);
                return null;
            }

            var payload = await resp.Content.ReadFromJsonAsync<LoginResponseDTO>(cancellationToken: cancellationToken);
            var token = payload?.Token;
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Login response did not contain a token");
                return null;
            }

            _accessToken = token;
            _api.SetAccessToken(token);
            _session = ParseSessionFromToken(token);
            if (_session != null)
            {
                _session.LoggedIn = true;
            }
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call authentication API");
            return null;
        }
    }

    public void Logout()
    {
        _accessToken = null;
        _session = null;
        _api.ClearAccessToken();
        LoggedOut?.Invoke(this, EventArgs.Empty);
    }

    private SessionDTO? ParseSessionFromToken(string jwt)
    {
        try
        {
            // JWT format: header.payload.signature (Base64Url)
            var parts = jwt.Split('.');
            if (parts.Length < 2)
            {
                _logger.LogWarning("Invalid JWT format (missing payload)");
                return null;
            }

            var payloadJson = Base64UrlDecodeToString(parts[1]);
            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                _logger.LogWarning("Unable to decode JWT payload");
                return null;
            }

            using var doc = JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            var session = new SessionDTO();

            // Collect all claims in a dictionary<string, List<string>> for general access
            var allClaims = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in root.EnumerateObject())
            {
                static List<string> ToStringList(JsonElement e)
                {
                    var list = new List<string>();
                    switch (e.ValueKind)
                    {
                        case JsonValueKind.Array:
                            foreach (var item in e.EnumerateArray())
                            {
                                list.Add(item.ToString());
                            }
                            break;
                        case JsonValueKind.Object:
                            // Store compact string for objects
                            list.Add(e.GetRawText());
                            break;
                        case JsonValueKind.Undefined:
                        case JsonValueKind.Null:
                            break;
                        default:
                            list.Add(e.ToString());
                            break;
                    }
                    return list;
                }

                allClaims[prop.Name] = ToStringList(prop.Value);
            }
            session.Claims = allClaims;

            // Standard claims
            if (root.TryGetProperty("sub", out var subEl) && Guid.TryParse(subEl.GetString(), out var uid))
                session.UserId = uid;

            if (root.TryGetProperty("unique_name", out var unameEl))
                session.Username = unameEl.GetString() ?? string.Empty;

            // Roles: could be "role": string|array or the role claim URI
            var roles = new List<string>();
            if (root.TryGetProperty("role", out var roleEl))
            {
                if (roleEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in roleEl.EnumerateArray())
                        roles.Add(r.GetString() ?? string.Empty);
                }
                else
                {
                    roles.Add(roleEl.GetString() ?? string.Empty);
                }
            }
            else if (root.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleUriEl))
            {
                if (roleUriEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in roleUriEl.EnumerateArray())
                        roles.Add(r.GetString() ?? string.Empty);
                }
                else
                {
                    roles.Add(roleUriEl.GetString() ?? string.Empty);
                }
            }
            roles.RemoveAll(string.IsNullOrWhiteSpace);
            session.Roles = roles;

            // Flags derived from roles
            session.IsAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
            session.IsBanned = roles.Contains("Banned", StringComparer.OrdinalIgnoreCase);
            session.IsWhitelisted = roles.Contains("Whitelisted", StringComparer.OrdinalIgnoreCase);

            // AccessGroups: stored as comma-separated GUIDs in claim "accessGroups"
            var accessGroups = new List<Guid>();
            if (root.TryGetProperty("accessGroups", out var agEl))
            {
                var agStr = agEl.GetString();
                if (!string.IsNullOrWhiteSpace(agStr))
                {
                    foreach (var part in agStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        if (Guid.TryParse(part, out var g))
                            accessGroups.Add(g);
                    }
                }
            }
            session.AccessGroups = accessGroups;

            // Times: prefer iat for IssuedAt, fallback to nbf; exp for ExpiresAt
            DateTime issuedAt = default;
            DateTime expiresAt = default;
            if (TryReadEpochClaim(root, "iat", out var iat))
                issuedAt = iat;
            else if (TryReadEpochClaim(root, "nbf", out var nbf))
                issuedAt = nbf;

            if (TryReadEpochClaim(root, "exp", out var exp))
                expiresAt = exp;

            session.IssuedAt = issuedAt == default ? DateTime.MinValue : issuedAt;
            session.ExpiresAt = expiresAt == default ? DateTime.MinValue : expiresAt;

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse JWT for session claims");
            return null;
        }
    }

    private static string Base64UrlDecodeToString(string base64Url)
    {
        try
        {
            string s = base64Url.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
                case 0: break;
                default: s += new string('=', 4 - (s.Length % 4)); break;
            }
            var bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool TryReadEpochClaim(JsonElement root, string name, out DateTime value)
    {
        value = default;
        if (!root.TryGetProperty(name, out var el))
            return false;

        try
        {
            long seconds;
            switch (el.ValueKind)
            {
                case JsonValueKind.Number:
                    if (!el.TryGetInt64(out seconds)) return false;
                    break;
                case JsonValueKind.String:
                    var s = el.GetString();
                    if (!long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds)) return false;
                    break;
                default:
                    return false;
            }
            value = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
