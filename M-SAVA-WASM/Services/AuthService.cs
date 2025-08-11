using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using M_SAVA_Core.Models;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace M_SAVA_WASM.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "login_token");
            return !string.IsNullOrEmpty(token);
        }

        private bool IsAuthPage(string uri, NavigationManager navManager)
        {
            var relativePath = navManager.ToBaseRelativePath(uri).ToLower();
            return relativePath.StartsWith("auth/");
        }

        public async Task CheckAndRedirectIfNotLoggedInAsync(NavigationManager navManager)
        {
            var uri = navManager.Uri.ToLower();
            if (!await IsLoggedInAsync() && !IsAuthPage(uri, navManager))
            {
                navManager.NavigateTo("/auth/login", true);
            }
        }

        public class AuthResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
        }

        public async Task<AuthResult> LoginAsync(LoginRequestDTO loginRequest)
        {
            var result = new AuthResult();
            if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                result.ErrorMessage = "Username and password are required.";
                return result;
            }
            var response = await _httpClient.PostAsJsonAsync("https://localhost:44395/api/auth/login", loginRequest);
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                if (loginResponse != null)
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "login_token", loginResponse.Token);
                    var session = ParseSessionFromJwt(loginResponse.Token);
                    var sessionJson = JsonSerializer.Serialize(session);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "session_dto", sessionJson);
                    result.Success = true;
                }
                else
                {
                    result.ErrorMessage = "Invalid response from server.";
                }
            }
            else
            {
                result.ErrorMessage = "Login failed. Please check your credentials.";
            }
            return result;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequestDTO registerRequest)
        {
            var result = new AuthResult();
            if (string.IsNullOrWhiteSpace(registerRequest.Username) || string.IsNullOrWhiteSpace(registerRequest.Password))
            {
                result.ErrorMessage = "Username and password are required.";
                return result;
            }
            if (registerRequest.InviteCode == Guid.Empty)
            {
                result.ErrorMessage = "Invite code must be a valid GUID.";
                return result;
            }
            var response = await _httpClient.PostAsJsonAsync("https://localhost:44395/api/auth/register", registerRequest);
            if (response.IsSuccessStatusCode)
            {
                result.Success = true;
            }
            else
            {
                result.ErrorMessage = "Registration failed. Please check your details and invite code.";
            }
            return result;
        }

        private SessionDTO ParseSessionFromJwt(string jwt)
        {
            var session = new SessionDTO();
            if (string.IsNullOrEmpty(jwt)) return session;
            var parts = jwt.Split('.');
            if (parts.Length != 3) return session;
            var payload = parts[1];
            var json = Encoding.UTF8.GetString(PadBase64(payload));
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            session.LoggedIn = true;
            session.Username = root.TryGetProperty("unique_name", out var uname) ? uname.GetString() ?? string.Empty : root.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty;
            session.UserId = root.TryGetProperty("sub", out var sub) && Guid.TryParse(sub.GetString(), out var guid) ? guid : Guid.Empty;
            session.IssuedAt = root.TryGetProperty("iat", out var iat) && long.TryParse(iat.GetRawText(), out var iatLong) ? DateTimeOffset.FromUnixTimeSeconds(iatLong).UtcDateTime : DateTime.MinValue;
            session.ExpiresAt = root.TryGetProperty("exp", out var exp) && long.TryParse(exp.GetRawText(), out var expLong) ? DateTimeOffset.FromUnixTimeSeconds(expLong).UtcDateTime : DateTime.MinValue;
            session.Roles = new List<string>();
            if (root.TryGetProperty("role", out var rolesProp))
            {
                if (rolesProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in rolesProp.EnumerateArray())
                        session.Roles.Add(r.GetString() ?? string.Empty);
                }
                else
                {
                    session.Roles.Add(rolesProp.GetString() ?? string.Empty);
                }
            }
            session.IsAdmin = session.Roles.Contains("Admin");
            session.IsBanned = session.Roles.Contains("Banned");
            session.IsWhitelisted = session.Roles.Contains("Whitelisted");
            session.Claims = new Dictionary<string, List<string>>();
            foreach (var prop in root.EnumerateObject())
            {
                if (!session.Claims.ContainsKey(prop.Name))
                    session.Claims[prop.Name] = new List<string>();
                session.Claims[prop.Name].Add(prop.Value.ToString());
            }
            if (root.TryGetProperty("accessGroups", out var agProp))
            {
                session.AccessGroups = new List<Guid>();
                var agStr = agProp.GetString();
                if (!string.IsNullOrEmpty(agStr))
                {
                    foreach (var g in agStr.Split(','))
                        if (Guid.TryParse(g, out var gid))
                            session.AccessGroups.Add(gid);
                }
            }
            return session;
        }

        private static byte[] PadBase64(string base64)
        {
            base64 = base64.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
