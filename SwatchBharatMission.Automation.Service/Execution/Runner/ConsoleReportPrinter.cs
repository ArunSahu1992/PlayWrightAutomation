using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution.Runner
{
    public static class ConsoleReportPrinter
    {
        public static void Print(List<TestCaseResult> results)
        {
            Console.WriteLine();
            Console.WriteLine("┌───────────────────────────────────────────┬──────────┬──────────────────┬────────────────────┐");
            Console.WriteLine("│ Test Case ID │ Status │ Start Time │ End Time │");
            Console.WriteLine("├───────────────────────────────────────────┼──────────┼──────────────────┼────────────────────┤");


            foreach (var r in results)
            {
                Console.WriteLine(
                $"│ {r.TestCaseId,-41} │ {r.Status,-8} │ {r.StartTime:yyyy-MM-dd HH:mm:ss} │ {r.EndTime:yyyy-MM-dd HH:mm:ss} │");
            }


            Console.WriteLine("└───────────────────────────────────────────┴──────────┴──────────────────┴────────────────────┘");
        }
    }
}
