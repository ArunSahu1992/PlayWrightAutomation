using Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestCases.common
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AutomationSettings _settings;

        public AuthService(IOptions<AutomationSettings> options)
        {
            _httpClient = new HttpClient();
            _settings = options.Value;
        }
        public async Task<string> GetTokenAsync()
        {
            var request = new HttpRequestMessage(
            HttpMethod.Post,
            _settings.LoginEndpoint);

            request.Headers.Add("tenant-code", _settings.TenantCode);

            var login = new LoginRequest
            {
                Username = _settings.Username,
                Password = _settings.Password
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(login),
            Encoding.UTF8,
            "application/json");

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);
            string token = doc.RootElement
                             .GetProperty("token")
                             .GetString()!;

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token not returned from API");

            return token;
        }
    }
}
