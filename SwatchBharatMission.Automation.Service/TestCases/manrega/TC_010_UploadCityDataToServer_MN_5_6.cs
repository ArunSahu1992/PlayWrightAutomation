using Automation.Core;
using Microsoft.Playwright;
using Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TestCases.common;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace TestCases
{
    public class TC_010_UploadCityDataToServer_MN_5_6 : ITestCase
    {
        public string TestCaseId => "TC_010_UploadCityDataToServer_MN_5_6";

        public async Task ExecuteAsync(AutomationContext automationContext, ILogger logger)
        {
            var filesToUpload = new Dictionary<string, string>();
            await using var context = await PlaywrightFactory.CreateAsync(automationContext.automationFlowSettings.Headless);
            var page = context.Page;
            page.SetDefaultTimeout(60000);           // 60 seconds
            page.SetDefaultNavigationTimeout(60000);
            logger.LogInformation($"TestCase {TestCaseId} Started for : {automationContext.automationFlowSettings.TenantCode} ");

            try
            {
                string state = automationContext.automationFlowSettings.BaseSetting.State;
                string city = automationContext.automationFlowSettings.City;
                automationContext.automationFlowSettings.TestCaseName = GetType().Name;

                // Navigate
                await page.GotoAsync(automationContext.automationFlowSettings.BaseUrl);
                await page.WaitForLoadStateAsync(LoadState.Load);
                logger.LogInformation("Page Loaded");

                var text = await page.InnerTextAsync("#ContentPlaceHolder1_lblStopSpam");
                var match = Regex.Match(text, @"(\d+)\s*([+\-*/])\s*(\d+)");
                if (!match.Success)
                    throw new Exception("Invalid captcha format");

                int a = int.Parse(match.Groups[1].Value);
                int b = int.Parse(match.Groups[3].Value);
                string op = match.Groups[2].Value;

                int answer = op switch
                {
                    "+" => a + b,
                    "-" => a - b,
                    "*" => a * b,
                    "/" => b != 0 ? a / b : 0,
                    _ => 0
                };

                await page.FillAsync("#ContentPlaceHolder1_txtCaptcha", answer.ToString());
                await page.ClickAsync("#ContentPlaceHolder1_btnLogin");
                await page.WaitForLoadStateAsync(LoadState.Load);
                logger.LogInformation("State Page Loaded.");

                int year = DateTime.Now.Year;
                string fullYear = $"{year - 1}-{year}";

                await page.SelectOptionAsync(
                         "#ContentPlaceHolder1_ddlfinyr",
                         new SelectOptionValue { Label = fullYear }
                     );

                await page.SelectOptionAsync(
                     "#ContentPlaceHolder1_ddl_States",
                     state
                 );

                await page.WaitForLoadStateAsync(LoadState.Load);


                await page.Locator("li a", new()
                {
                    HasText = "GP With No Employment"
                }).ClickAsync();


                await page.WaitForLoadStateAsync(LoadState.Load);
                logger.LogInformation("City Page Loaded.");


                await page.WaitForLoadStateAsync(LoadState.Load);

                await page.ClickAsync($"a:has-text('{city}')");

                await page.WaitForLoadStateAsync(LoadState.Load);

                await page.ClickAsync("#ContentPlaceHolder1_rdbuttondistrict_0");
                await page.ClickAsync("#ContentPlaceHolder1_RBtnLst_1");

                var download = await page.RunAndWaitForDownloadAsync(async () =>
                {
                    await page.Locator("#ContentPlaceHolder1_LinkButton1").ClickAsync();
                });

                await page.WaitForLoadStateAsync(LoadState.Load);

                // Prepare temp directory
                string tempDir = Path.Combine(Path.GetTempPath(), "AutomationFiles");
                Directory.CreateDirectory(tempDir);

                // Build file path
                string fileName = $"{Guid.NewGuid()}_{download.SuggestedFilename}";

                string filePath = Path.Combine(tempDir, fileName);

                await download.SaveAsAsync(filePath);
                filesToUpload.Add("TillDate", filePath);

                await page.WaitForLoadStateAsync(LoadState.Load);

                await page.ClickAsync("#ContentPlaceHolder1_RBtnLst_0");

                var months = GetMonthsFromAprilTillNow();

                var dropdown = page.Locator("select#ContentPlaceHolder1_DDmonth");

                foreach (var month in months)
                {
                    await dropdown.SelectOptionAsync(new SelectOptionValue
                    {
                        Label = month
                    });

                    await page.WaitForLoadStateAsync(LoadState.Load);
                    await page.WaitForTimeoutAsync(2000);

                    download = await page.RunAndWaitForDownloadAsync(async () =>
                    {
                        await page.Locator("#ContentPlaceHolder1_LinkButton1").ClickAsync();
                    });

                    fileName = $"{Guid.NewGuid()}_{download.SuggestedFilename}";

                    filePath = Path.Combine(tempDir, fileName);

                    await download.SaveAsAsync(filePath);
                    filesToUpload.Add(month, filePath);
                    await page.WaitForTimeoutAsync(2000);
                }

                logger.LogInformation("Executing Get Token.");
                // Upload files
                var authService = new AuthService(automationContext);
                string token = await authService.GetTokenAsync();

                logger.LogInformation("Token recieved.");

                logger.LogInformation("Executing UploadFileService.");

                // generate request object;
                var param = new Dictionary<string, string>();
                param.Add("year", fullYear);

                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(File.OpenRead($"{filesToUpload["TillDate"]}")), "TillDate", $"{filesToUpload["TillDate"]}");

                foreach (var item in months)
                {
                    content.Add(new StreamContent(File.OpenRead($"{filesToUpload[item]}")), $"{item}", $"{filesToUpload[item]}");
                }

                var uploadFileService = new UploadFileService(automationContext);
                var result = await uploadFileService.UploadFiles(token, null, TestCaseId, param, sourceContent: content);

                if (!(result.Item1 == "Import completed successfully" &&
                      result.Item2 == HttpStatusCode.OK))
                {
                    logger.LogInformation("File uploaded failed.");
                    throw new Exception("Import failed.");
                }

                logger.LogInformation("File uploaded successfully.");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.Message);
                throw;
            }
            finally
            {

            }
        }
        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        public static List<string> GetMonthsFromAprilTillNow()
        {
            var result = new List<string>();

            DateTime now = DateTime.Now;

            int startYear = now.Month >= 4 ? now.Year : now.Year - 1;
            DateTime start = new DateTime(startYear, 4, 1);

            DateTime current = new DateTime(now.Year, now.Month, 1);

            while (start <= current)
            {
                result.Add(start.ToString("MMMM"));
                start = start.AddMonths(1);
            }

            return result;
        }
    }
}