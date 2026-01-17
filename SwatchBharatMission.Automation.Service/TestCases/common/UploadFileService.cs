using Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestCases.common
{
    public class UploadFileService
    {
        private readonly HttpClient _httpClient;
        private readonly AutomationSettings _settings;

        public UploadFileService(IOptions<AutomationSettings> options)
        {
            _httpClient = new HttpClient();
            _settings = options.Value;
        }
        public async Task<(string,HttpStatusCode)> UploadFiles(string token, IEnumerable<string> filesToUpload)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _settings.UploadApiUrl);
            request.Headers.Add("tenant-code", _settings.TenantCode);
            request.Headers.Add("Authorization", $"bearer {token}");
            var content = new MultipartFormDataContent();
            foreach (var item in filesToUpload)
            {
                content.Add(new StreamContent(File.OpenRead($"{item}")), "files", $"{item}");
            }
            request.Content = content;
            var response = await client.SendAsync(request);
            return (await response.Content.ReadAsStringAsync(), response.StatusCode);
        }
    }
}
