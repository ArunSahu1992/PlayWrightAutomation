using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public class RunTracker
    {
        private static string _csvFilePath = Path.Combine(Constants.GetPersistentPath(), Constants.TRACKTER_FOLDER);

        public RunTracker()
        {
            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(_csvFilePath))
            {
                using var writer = new StreamWriter(_csvFilePath);
                writer.WriteLine("Date,RunCount");
            }
        }

        public bool IsFirstRunToday()
        {
            var today = DateTime.UtcNow.Date;
            var lines = File.ReadAllLines(_csvFilePath);

            if (lines.Length <= 1) return true; // Only header exists

            var lastLine = lines[^1]; // Last row
            var parts = lastLine.Split(',');
            if (DateTime.TryParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastDate))
            {
                return lastDate < today;
            }

            return true;
        }

        public void RecordRun()
        {
            var today = DateTime.UtcNow.Date;
            int runCount = 1;

            var lines = File.ReadAllLines(_csvFilePath);

            if (lines.Length > 1)
            {
                var lastLine = lines[^1];
                var parts = lastLine.Split(',');
                if (DateTime.TryParseExact(parts[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastDate)
                    && lastDate == today)
                {
                    runCount = int.Parse(parts[1]) + 1;
                    // Remove last line and append updated count
                    File.WriteAllLines(_csvFilePath, lines[..^1]);
                }
            }

            using var writer = new StreamWriter(_csvFilePath, append: true);
            writer.WriteLine($"{today:yyyy-MM-dd},{runCount}");
        }
    }
}
