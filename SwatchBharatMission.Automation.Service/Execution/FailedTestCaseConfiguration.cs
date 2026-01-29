using Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution
{
    public class FailedTestCaseConfiguration
    {
        private static string FailureDir = Path.Combine(GetPersistentPath(), "failed-tests");

        public static string GetPersistentPath()
        {
            // 1️⃣ Try ApplicationData (Windows/macOS)
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrWhiteSpace(appData))
                return appData;

            // 2️⃣ Linux fallback → HOME
            var home = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrWhiteSpace(home))
                return Path.Combine(home, ".config");

            // 3️⃣ Last resort (CI-safe)
            return "/tmp";
        }
        public static List<FailedTestCase> ReadTodayFailedTests()
        {
            var path = GetCsvPath();

            if (!File.Exists(path))
                return new();

            var lines = File.ReadAllLines(path).Skip(1);

            var res = lines.Select(l =>{var p = l.Split(',');
                return new FailedTestCase
                {
                    TestName = p[0],
                    City = p[1],
                    TenantCode = p[2],
                    CreatedDate = DateTime.Parse(p[3]).Date
                };
            }).ToList().Where(x => x.CreatedDate == DateTime.Now.Date);

            File.Delete(GetCsvPath());

            return res.ToList();
        }

        public static void WriteFailedTests(List<FailedTestCase> failed)
        {
            var CsvPath = GetCsvPath();
            if (failed != null && !failed.Any())
            {
                if (File.Exists(CsvPath))
                    File.Delete(CsvPath);
                return;
            }

            Console.WriteLine("Writing Failied TestCases at Location: " + CsvPath);

            if (File.Exists(CsvPath))
                File.Delete(CsvPath);

            Directory.CreateDirectory(FailureDir);

            Console.WriteLine("Directory Created at path:  " + CsvPath);

            try
            {
                using var sw = new StreamWriter(CsvPath, false);
                sw.WriteLine("TestCaseId,City,TenantCode,CreatedDate");

                foreach (var f in failed)
                {
                    sw.WriteLine(
                        $"{f.TestName},{f.City},{f.TenantCode},{DateTime.Now.Date}"
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed TestCase Written failed due to :  " + ex.Message);
                throw;
            }
            Console.WriteLine("Failed TestCase Written Succeddfully at :  " + CsvPath);
        }

        private static string GetCsvPath()
        {

            return Path.Combine(
                FailureDir,
                $"failed-tests.csv"
            );
        }


    }
}
