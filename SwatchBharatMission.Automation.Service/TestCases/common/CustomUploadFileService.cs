using Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestCases.common
{
    public class CustomUploadFileService
    {
        private readonly HttpClient _httpClient;
        private readonly UploadAutomationSettings _settings;

        public CustomUploadFileService(UploadAutomationSettings options)
        {
            _httpClient = new HttpClient();
            _settings = options;
        }
        public async Task<(string,HttpStatusCode)> UploadFiles(string token, Dictionary<string, List<string>> filesToUpload)
        {
            var response = await RetryHelper.ExecuteAsync(
                      async () =>
                      {
                          var client = new HttpClient();
                          var request = new HttpRequestMessage(HttpMethod.Post, _settings.TestCases.FirstOrDefault(x => x.Key == _settings.TestCaseName).Value.UploadApiUrl);
                          request.Headers.Add("tenant-code", _settings.TenantCode);
                          request.Headers.Add("Authorization", $"bearer {token}");
                          var content = new MultipartFormDataContent();
                          foreach (var key in filesToUpload.Keys)
                          {
                              foreach (var item in filesToUpload[key])
                              {
                                  content.Add(new StreamContent(File.OpenRead($"{item}")), key, $"{item}");
                              }
                          }
                          request.Content = content;
                          using var response = await _httpClient.SendAsync(request);
                          response.EnsureSuccessStatusCode();

                          if (response.StatusCode != HttpStatusCode.OK)
                              throw new Exception("Upload Service failed");

                          return (await response.Content.ReadAsStringAsync(), response.StatusCode);
                      }
                  );
            return response;
        }
    }
}
