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
        private static string FailureDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "failed-tests");
        public static List<FailedTestCase> ReadTodayFailedTests()
        {
            var path = GetCsvPath();

            if (!File.Exists(path))
                return new();

            var lines = File.ReadAllLines(path).Skip(1);

            return lines.Select(l =>
            {
                var p = l.Split(',');
                return new FailedTestCase
                {
                    TestName = p[0],
                    City = p[1],
                };
            }).ToList();
        }

        public static void WriteFailedTests(List<FailedTestCase> failed)
        {
            var CsvPath = GetCsvPath();
            if (!failed.Any())
            {
                if (File.Exists(CsvPath))
                    File.Delete(CsvPath);
                return;
            }
      
            if (File.Exists(CsvPath))
                File.Delete(CsvPath);

            Directory.CreateDirectory(FailureDir);
            
            using var sw = new StreamWriter(CsvPath, false);
            sw.WriteLine("TestCaseId,TestName,Error,Timestamp");

            foreach (var f in failed)
            {
                sw.WriteLine(
                    $"{f.TestName},{f.City}"
                );
            }
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
