using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace Configuration
{
    public sealed class AutomationContext
    {
        public string FlowName { get; }
        public bool IsFirstRun { get; }
        private readonly IHostEnvironment _env;
        public AutomationFlowSettings automationFlowSettings;
        public string _appRootPath;

        public AutomationContext(AutomationFlowSettings settings, BaseContext _baseContext)
        {
            FlowName = _baseContext.Flow;
            automationFlowSettings = settings;
            _appRootPath = _baseContext.AppRootPath;
            IsFirstRun = _baseContext.IsFirstRun;
        }

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
               _appRootPath,
                Constants.SETTINGS_FOLDER,
                flowName);

            Console.WriteLine("Folder Path " + folderPath);

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
