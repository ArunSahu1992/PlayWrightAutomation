using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Configuration
{
    public class ConfigurationRegistry : IConfigurationRegistry
    {
        private readonly List<UploadAutomationSettings> _configs = new();

        public ConfigurationRegistry(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException(folderPath);

            foreach (var file in Directory.GetFiles(folderPath, "*.json"))
            {
                var json = File.ReadAllText(file);
                var config = JsonSerializer.Deserialize<UploadAutomationSettings>(json);

                if (config == null)
                    throw new InvalidOperationException($"Invalid JSON: {file}");

                _configs.Add(config);
            }
        }

        public IReadOnlyList<UploadAutomationSettings> GetAll() => _configs;

        public UploadAutomationSettings GetByTenant(string tenantCode) =>
            _configs.FirstOrDefault(x =>
                x.TenantCode.ToLower().Equals(tenantCode?.ToLower(), StringComparison.OrdinalIgnoreCase));
    }

}
