using Automation.Core;
using Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Execution
{
    public class ReportGenerator
    {
        public static void GenerateReport(List<TestCaseResult> testResults, string outputPath)
        {
            var html = new StringBuilder();

            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title>Automation Test Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("table { border-collapse: collapse; width: 80%; margin: 20px auto; }");
            html.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f4f4f4; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<h2 style='text-align:center'>Automation Test Report</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<thead><tr><th>Test Case</th><th>Status</th><th>Screenshot</th></tr></thead>");
            html.AppendLine("<tbody>");

            foreach (var t in testResults)
            {
                string color = t.Status == "Passed" ? "green" : "red";

                html.AppendLine($@"
        <tr>
            <td>{t.TestCaseId}</td>
            <td style='color:{color}'>{t.Status}</td>
            <td></td>
        </tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            string reportFolder = Path.GetTempPath();
            string reportFileName = "AutomationReport.html";

            string reportFilePath = Path.Combine(reportFolder, reportFileName);

            // Create ONLY the folder
            Directory.CreateDirectory(reportFolder);

            // Write to FILE
            File.WriteAllText(reportFilePath, html.ToString());
            Console.WriteLine($"Report generated at {Path.GetFullPath(reportFilePath)}");
        }
    }
}
