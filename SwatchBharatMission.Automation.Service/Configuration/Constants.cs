
namespace Configuration
{
    public class Constants
    {
        public const string MANREGA_FLOW_NAME = "Mnrega";
        public const string SWATCHBHARAT_FLOW_NAME = "SwatchBharat";
        public const string FAILED_TEST_FOLDER = "failed-tests";
        public const string TRACKTER_FOLDER = "track.csv";
        public const string SETTINGS_FOLDER = "settings";

        public static string GetPersistentPath()
        {
            // 1️⃣ Try ApplicationData (Windows/macOS)
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrWhiteSpace(appData))
                return appData;

            // 2️⃣ Linux fallback → HOME
                     var home = Environment.GetFolderPath(
    Environment.SpecialFolder.ApplicationData
);

            if (!string.IsNullOrWhiteSpace(home))
                return Path.Combine(home, ".config");

            // 3️⃣ Last resort (CI-safe)
            return "/tmp";
        }
    }
}
