using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Configuration
{
    public sealed class AutomationContext
    {
        public string FlowName { get; }
        private readonly IHostEnvironment _env;
        public AutomationFlowSettings automationFlowSettings;
        public IHostEnvironment _hostEnvironment;


        public AutomationContext(string flowName,AutomationFlowSettings settings, IHostEnvironment hostEnvironment)
        {
            FlowName = flowName;
            automationFlowSettings = settings;
            _hostEnvironment = hostEnvironment;
        }

        private void EnsureDirectories()
        {
            if (!Directory.Exists(automationFlowSettings.RegistryPath))
            {
                Directory.CreateDirectory(automationFlowSettings.RegistryPath);
            }
        }

        // 🔹 Common helper paths
        public string GetReportPath(string fileName)
            => Path.Combine(automationFlowSettings.RegistryPath, fileName);

        public string GetTempPath(string fileName)
            => Path.Combine(Path.GetTempPath(), fileName);

        public Dictionary<string, UploadAutomationSettings> Cities
        {
            get
            {
                return GetAllCityData(FlowName);
            }
        }

        public Dictionary<string, UploadAutomationSettings> GetAllCityData(string flowName)
        {
            var folderPath = Path.Combine(
                _hostEnvironment.ContentRootPath,
                "Settings",
                flowName);

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException(folderPath);

            var result = new Dictionary<string, UploadAutomationSettings>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.GetFiles(folderPath, "*.json"))
            {
                var cityName = Path.GetFileNameWithoutExtension(file);

                // skip control files
                if (cityName.Equals("flow", StringComparison.OrdinalIgnoreCase) ||
                    cityName.Equals("city", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    var json = File.ReadAllText(file);
                    var entity = JsonSerializer.Deserialize<UploadAutomationSettings>(json);

                    if (entity != null)
                        result[cityName] = entity;
                }
                catch (Exception ex)
                {
                }
            }

            return result;
        }
    }

}
