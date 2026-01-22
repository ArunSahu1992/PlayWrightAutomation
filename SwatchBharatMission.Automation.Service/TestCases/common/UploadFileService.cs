using Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestCases.common
{
    public class UploadFileService
    {
        private readonly HttpClient _httpClient;
        private readonly UploadAutomationSettings _settings;

        public UploadFileService(UploadAutomationSettings options)
        {
            _httpClient = new HttpClient();
            _settings = options;
        }
        public async Task<(string,HttpStatusCode)> UploadFiles(string token, IEnumerable<string> filesToUpload)
        {
            var res = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, _settings.TestCases.FirstOrDefault(x => x.Key == _settings.TestCaseName).Value.UploadApiUrl);
                    request.Headers.Add("tenant-code", _settings.TenantCode);
                    request.Headers.Add("Authorization", $"bearer {token}");
                    var content = new MultipartFormDataContent();
                    foreach (var item in filesToUpload)
                    {
                        content.Add(new StreamContent(File.OpenRead($"{item}")), "files", $"{item}");
                    }
                    request.Content = content;
                    var response = await _httpClient.SendAsync(request);

                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception("Upload Service failed");

                    return (await response.Content.ReadAsStringAsync(), response.StatusCode);
                }
            );
            return res;
        }
    }
}
