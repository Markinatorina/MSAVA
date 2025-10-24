using System;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace MSAVA_App.Services.Storage;

internal class LocalStorageService
{
    public static class Keys
    {
        public const string AccessToken = "accessToken";
        public const string RefreshToken = "refreshToken";
    }

    private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    public Task<bool> ContainsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required", nameof(key));
        return Task.FromResult(_settings.Values.ContainsKey(key));
    }

    public Task SetAsync(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required", nameof(key));
        if (value is null)
        {
            _settings.Values.Remove(key);
        }
        else
        {
            _settings.Values[key] = value;
        }
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required", nameof(key));
        return Task.FromResult(_settings.Values.TryGetValue(key, out var obj) ? obj as string : null);
    }

    public Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required", nameof(key));
        _settings.Values.Remove(key);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _settings.Values.Clear();
        return Task.CompletedTask;
    }

    public async Task SetObjectAsync<T>(string key, T? value, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required", nameof(key));
        if (value is null)
        {
            await RemoveAsync(key).ConfigureAwait(false);
            return;
        }
        var json = JsonSerializer.Serialize(value, options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await SetAsync(key, json).ConfigureAwait(false);
    }

    public async Task<T?> GetObjectAsync<T>(string key, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required", nameof(key));
        var json = await GetAsync(key).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json, options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch
        {
            // Data might be corrupted or of different shape — remove to avoid repeated failures
            _settings.Values.Remove(key);
            return default;
        }
    }
}
