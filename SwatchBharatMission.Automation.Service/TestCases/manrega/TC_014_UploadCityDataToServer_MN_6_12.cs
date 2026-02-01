using Automation.Core;
using Microsoft.Playwright;
using Configuration;
using Microsoft.Extensions.Logging;
using TestCases.common;
using System.Net;
using System.Text.RegularExpressions;

namespace TestCases
{
    public class TC_014_UploadCityDataToServer_MN_6_12 : ITestCase
    {
        public string TestCaseId => "TC_014_UploadCityDataToServer_MN_6_12";

        public async Task ExecuteAsync(AutomationContext automationContext, ILogger logger)
        {
            var filesToUpload = new List<string>();
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
                         state.ToUpper()
                     );

                await page.WaitForLoadStateAsync(LoadState.Load);

                await page.Locator("ol li a u", new()
                {
                    HasTextString = "Dynamic Report for Monitoring and details of works"
                }).First.ClickAsync();


                await page.Locator("[name='ctl00$ContentPlaceHolder1$ddl_state']")
                         .SelectOptionAsync(new SelectOptionValue { Label = state });

                await page.Locator("[name='ctl00$ContentPlaceHolder1$ddl_dist']")
                         .SelectOptionAsync(new SelectOptionValue { Label = city.ToUpper() });


                string blockSelector = "select[name='ctl00$ContentPlaceHolder1$ddl_blk']";
                string workStatusSelector = "select[name='ctl00$ContentPlaceHolder1$Ddlwork_status']";
                string buttonSelector = "input[name='ctl00$ContentPlaceHolder1$Button1']";

                // Wait for first dropdown
                await page.WaitForSelectorAsync(blockSelector);

                // Get all BLOCK values (skip --Select--)
                var blockValues = await page.Locator($"{blockSelector} option:not([value='0'])")
                   .EvaluateAllAsync<string[]>("opts => opts.map(o => o.innerText.trim())");

                foreach (var blockValue in blockValues)
                {
                    if (blockValue == "ALL") continue;
                    // Select BLOCK
                    await SelectAndWaitAsync(page, blockSelector, blockValue);

                    // Wait for WORK STATUS dropdown to reload
                    await page.WaitForTimeoutAsync(3000); // 1 second
                    // Get WORK STATUS values (fresh every time)
                    var workStatusValues = await page.Locator($"{workStatusSelector} option:not([value='0'])")
                        .EvaluateAllAsync<string[]>("opts => opts.map(o => o.innerText.trim())");

                    foreach (var statusValue in workStatusValues)
                    {
                        if (statusValue == "ALL") continue;
                        // Select WORK STATUS
                        await SelectAndWaitAsync(page, workStatusSelector, statusValue);

                        await page.WaitForTimeoutAsync(5000); // 1 second
                        // Click button and wait

                        await page.Locator("#ContentPlaceHolder1_Button1").ClickAsync();

                        await page.WaitForTimeoutAsync(5000); // 1 second

                        var button = page.Locator("#ContentPlaceHolder1_LinkButton1");

                        var timeout = TimeSpan.FromSeconds(5);
                        var start = DateTime.UtcNow;

                        bool shouldClick = false;

                        while (DateTime.UtcNow - start < timeout)
                        {
                            if (await button.IsVisibleAsync() && await button.IsEnabledAsync())
                            {
                                shouldClick = true;
                                break;
                            }

                            await Task.Delay(200); // small polling delay
                        }
                        if (shouldClick)
                        {
                            var download = await page.RunAndWaitForDownloadAsync(async () =>
                            {
                                // Escape $ in the ID
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
                            filesToUpload.Add(filePath);
                            Console.WriteLine($"Processed Block={blockValue}, Status={statusValue}");
                        }
                        else
                        {
                            // Not visible within 3 seconds → move to next step
                            Console.WriteLine("Button not visible in 3s, skipping click for : " + statusValue);
                        }
                    }
                }

                if (!filesToUpload.Any())
                {
                    Console.WriteLine("No file present to upload");
                    return;
                }

                var authService = new AuthService(automationContext);
                string token = await authService.GetTokenAsync();

                logger.LogInformation("Token recieved.");

                logger.LogInformation("Executing UploadFileService.");
                int batchSize = 10;
                var uploadFileService = new UploadFileService(automationContext);

                // read all files in memory and sort by memory size.
                // take only files which are less than 25 in count.

                long batchLimit = 5 * 1024 * 1024; // 25 MB
                var batchFiles = new List<string>();
                long currentBatchSize = 0;
                
                foreach (var file in filesToUpload)
                {
                    var fileInfo = new FileInfo(file);

                    // skip empty files if needed
                    if (fileInfo.Length == 0)
                        continue;

                    batchFiles.Add(file);
                    currentBatchSize += fileInfo.Length;

                    if (currentBatchSize >= batchLimit)
                    {
                        var result = await uploadFileService.UploadFiles(
                      token,
                      batchFiles,
                      TestCaseId,
                      new Dictionary<string, string> { ["year"] = fullYear }
                  );
                        if (!(result.Item1 == "Import completed successfully" &&
                         result.Item2 == HttpStatusCode.OK))
                        {
                            logger.LogInformation("File uploaded failed.");
                            throw new Exception("Import failed.");
                        }

                        // reset batch
                        batchFiles.Clear();
                        currentBatchSize = 0;
                    }
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

        async Task SelectAndWaitAsync(IPage page, string selector, string value)
        {
            await Task.WhenAll(
                page.WaitForLoadStateAsync(LoadState.NetworkIdle),
                page.Locator(selector).SelectOptionAsync(value)
            );
        }
    }
}