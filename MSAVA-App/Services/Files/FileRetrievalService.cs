    using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MSAVA_App.Services.Api;
using MSAVA_App.Services.Authentication;
using MSAVA_Shared.Models;

namespace MSAVA_App.Services.Files
{
    public class FileRetrievalService
    {
        private readonly ApiService _api;
        private readonly AuthenticationService _auth;

        public FileRetrievalService(ApiService api, AuthenticationService auth)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        public async Task<IReadOnlyList<SearchFileDataDTO>> GetAllFileMetadataAsync(CancellationToken cancellationToken = default)
        {
            var list = await _api.SendForAsync<List<SearchFileDataDTO>>(
                HttpMethod.Get,
                "api/files/retrieve/meta/all",
                bearerToken: _auth.AccessToken,
                cancellationToken: cancellationToken
            );

            return (IReadOnlyList<SearchFileDataDTO>)(list ?? new List<SearchFileDataDTO>());
        }
    }
}
