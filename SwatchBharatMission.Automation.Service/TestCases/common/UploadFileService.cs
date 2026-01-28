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
        private readonly AutomationContext _settings;

        public UploadFileService(AutomationContext options)
        {
            _httpClient = new HttpClient();
            _settings = options;
        }
        public async Task<(string, HttpStatusCode)> UploadFiles(string token, IEnumerable<string> filesToUpload, string testCase, Dictionary<string, string> additionalParams = null, MultipartFormDataContent sourceContent = null)
        {
            var res = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, _settings.Cities[_settings.automationFlowSettings.TenantCode].TestCases.FirstOrDefault(x => x.Key == testCase).Value.UploadApiUrl);
                    request.Headers.Add("tenant-code", _settings.automationFlowSettings.TenantCode);
                    request.Headers.Add("Authorization", $"bearer {token}");

                    var content = new MultipartFormDataContent();

                    if (sourceContent is not null)
                    {
                        content = sourceContent;
                    }
                    else
                    {
                        foreach (var item in filesToUpload)
                        {
                            content.Add(new StreamContent(File.OpenRead($"{item}")), "files", $"{item}");
                        }
                    }

                    if (additionalParams is not null)
                        foreach (var item in additionalParams)
                        {
                            content.Add(new StringContent(item.Value), item.Key);
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
