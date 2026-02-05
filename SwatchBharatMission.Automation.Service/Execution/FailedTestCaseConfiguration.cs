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
        private static string FailureDir = Path.Combine(Constants.GetPersistentPath(), Constants.FAILED_TEST_FOLDER);

        public static List<FailedTestCase> ReadTodayFailedTests()
        {
            var path = GetCsvPath();

            Console.WriteLine("Reading file for failed testcases from path :"+ path);

            if (!File.Exists(path))
                return new();

            var lines = File.ReadAllLines(path).Skip(1);

            var res = lines.Select(l =>{var p = l.Split(',');
                return new FailedTestCase
                {
                    TestName = p[0],
                    City = p[1],
                    TenantCode = p[2],
                    CreatedDate = DateTime.Parse(p[3]).Date,
                    Flow = p[4],
                };
            }).ToList().Where(x => x.CreatedDate == DateTime.Now.Date);

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
                sw.WriteLine("TestCaseId,City,TenantCode,CreatedDate,flow");

                foreach (var f in failed)
                {
                    sw.WriteLine(
                        $"{f.TestName},{f.City},{f.TenantCode},{DateTime.Now.Date},{f.Flow}"
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
