using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using M_SAVA_Core.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace M_SAVA_WASM.Services
{
    public class UploadService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        public UploadService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        private async Task AddAuthHeaderAsync(HttpRequestMessage request)
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "login_token");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UploadFileAsync(SaveFileFromFormFileDTO dto, IBrowserFile file)
        {
            if (file == null)
                return (false, "Please select a file.");
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.FileName), nameof(dto.FileName));
            content.Add(new StringContent(dto.FileExtension), nameof(dto.FileExtension));
            content.Add(new StringContent(dto.Description ?? string.Empty), nameof(dto.Description));
            content.Add(new StringContent(dto.AccessGroupId.ToString()), nameof(dto.AccessGroupId));
            content.Add(new StringContent(dto.PublicViewing.ToString()), nameof(dto.PublicViewing));
            content.Add(new StringContent(dto.PublicDownload.ToString()), nameof(dto.PublicDownload));
            if (dto.Tags != null)
                foreach (var tag in dto.Tags)
                    content.Add(new StringContent(tag), "Tags");
            if (dto.Categories != null)
                foreach (var cat in dto.Categories)
                    content.Add(new StringContent(cat), "Categories");
            var fileContent = new StreamContent(file.OpenReadStream(file.Size));
            content.Add(fileContent, nameof(dto.FormFile), file.Name);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44395/api/files/store/formfile")
            {
                Content = content
            };
            await AddAuthHeaderAsync(request);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return (true, string.Empty);
            return (false, "File upload failed.");
        }

        public async Task<(bool Success, string ErrorMessage)> UploadUrlAsync(SaveFileFromUrlDTO dto)
        {
            var dict = new Dictionary<string, string>
            {
                { nameof(dto.FileUrl), dto.FileUrl },
                { nameof(dto.FileName), dto.FileName },
                { nameof(dto.FileExtension), dto.FileExtension },
                { nameof(dto.Description), dto.Description ?? string.Empty },
                { nameof(dto.AccessGroupId), dto.AccessGroupId.ToString() },
                { nameof(dto.PublicViewing), dto.PublicViewing.ToString() },
                { nameof(dto.PublicDownload), dto.PublicDownload.ToString() }
            };
            if (dto.Tags != null)
                foreach (var tag in dto.Tags)
                    dict.Add("Tags", tag);
            if (dto.Categories != null)
                foreach (var cat in dto.Categories)
                    dict.Add("Categories", cat);
            var content = new FormUrlEncodedContent(dict);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44395/api/files/store/url")
            {
                Content = content
            };
            await AddAuthHeaderAsync(request);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return (true, string.Empty);
            return (false, "File upload failed.");
        }
    }
}
