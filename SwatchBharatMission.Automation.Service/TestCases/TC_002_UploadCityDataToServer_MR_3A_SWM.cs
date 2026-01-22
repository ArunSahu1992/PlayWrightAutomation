using Automation.Core;
using Microsoft.Playwright;
using Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TestCases.common;
using System.Net;

namespace TestCases
{
    public class TC_002_UploadCityDataToServer_MR_3A_SWM : ITestCase
    {
        public string TestCaseId => "TC_002_UploadCityDataToServer_MR_3A_SWM";

        public async Task ExecuteAsync(UploadAutomationSettings uploadAutomationSettings, AutomationSettings automationSettings, ILogger logger)
        {
            var filesToUpload = new Dictionary<string, List<string>>();
            await using var context = await PlaywrightFactory.CreateAsync(automationSettings.Headless);

            logger.LogInformation($"TestCase {TestCaseId} Started for : {uploadAutomationSettings.TenantCode} ");

            var page = context.Page;
            page.SetDefaultTimeout(60000);           // 60 seconds
            page.SetDefaultNavigationTimeout(60000);
            var categories = new[] {
                "Grey Water Management",
                "Solid Waste Management"
            };

            try
            {
                string state = uploadAutomationSettings.State;
                string city = uploadAutomationSettings.TenantCode;
                uploadAutomationSettings.TestCaseName = GetType().Name;

                // Navigate
                await page.GotoAsync(automationSettings.BaseUrl);
                await page.WaitForLoadStateAsync(LoadState.Load);
                logger.LogInformation("Page Loaded");

                await page.ClickAsync("#RptrPhaseIIMISreport_ctl01_lnkbtn_PageLinkHeader");
                await page.WaitForLoadStateAsync(LoadState.Load);
                logger.LogInformation("State Page Loaded.");

                await page.WaitForLoadStateAsync(LoadState.Load);
                logger.LogInformation("Page Loaded");

                foreach (var category in categories)
                {
                    logger.LogInformation($"Processing category: {category}");

                    filesToUpload[category] = new List<string>();


                    await page.RunAndWaitForNavigationAsync(async () =>
                    {
                        await page.SelectOptionAsync(
                            "#ctl00_ContentPlaceHolder1_ddlcat",
                            new SelectOptionValue { Label = category }
                        );
                    });

                    await page.WaitForLoadStateAsync(LoadState.Load);
                    logger.LogInformation("State Page Loaded.");

                    // Click state & city
                    await page.ClickAsync($"a:text-matches('{state}', 'i')");

                    await page.WaitForLoadStateAsync(LoadState.Load);
                    logger.LogInformation("City Page Loaded.");
                    await page.ClickAsync($"a:text-matches('{city}', 'i')");

                    await page.WaitForLoadStateAsync(LoadState.Load);
                    logger.LogInformation("Sub City Page loaded");
                    int blockCount = 0;
                    // Get block count (retry once)
                    if (category == "Grey Water Management")
                        blockCount = await page.Locator("a[id$='lbl_blknameGWM']").CountAsync();
                    else
                        blockCount = await page.Locator("a[id$='_lbl_block']").CountAsync();

                    if (blockCount == 0)
                    {
                        await Task.Delay(5000);
                        logger.LogInformation("Waiting for delay 5000ms.");
                        // Get block count (retry once)
                        if (category == "Grey Water Management")
                            blockCount = await page.Locator("a[id$='lbl_blknameGWM']").CountAsync();
                        else
                            blockCount = await page.Locator("a[id$='_lbl_block']").CountAsync();
                    }

                    logger.LogInformation($"Total Block Count ({category}): {blockCount}");

                    for (int i = 0; i < blockCount; i++)
                    {
                        await page.WaitForLoadStateAsync(LoadState.Load);
                        ILocator block;
                        if (category == "Grey Water Management")
                            block = page.Locator("a[id$='lbl_blknameGWM']").Nth(i);
                        else
                        {
                            block = page.Locator("a[id$='_lbl_block']").Nth(i);
                        }
                        string blockName = (await block.InnerTextAsync()).Trim();

                        logger.LogInformation($"[{category}] Clicking block: {blockName}");

                        await block.ClickAsync();

                        await page.WaitForLoadStateAsync(LoadState.Load);
                        var download = await page.RunAndWaitForDownloadAsync(async () =>
                        {
                            await page.Locator("#ctl00_ContentPlaceHolder1_btnExcel").ClickAsync();
                        });

                        // Prepare temp directory
                        string tempDir = Path.Combine(Path.GetTempPath(), "AutomationFiles");
                        Directory.CreateDirectory(tempDir);

                        // File name includes category
                        string fileName =
                            $"{SanitizeFileName(category)}_{SanitizeFileName(blockName)}_{Guid.NewGuid()}_{download.SuggestedFilename}";

                        string filePath = Path.Combine(tempDir, fileName);

                        await download.SaveAsAsync(filePath);

                        // Store under correct category
                        filesToUpload[category].Add(filePath);

                        // Go back

                        if (category == "Grey Water Management")
                            await page.ClickAsync("#ctl00_ContentPlaceHolder1_lnk_backdetailgwm");
                        else
                            await page.ClickAsync("#ctl00_ContentPlaceHolder1_lnk_backdetailswm");
                    }
                }


                logger.LogInformation("Executing Get Token.");
                // Upload files
                var authService = new AuthService(uploadAutomationSettings);
                string token = await authService.GetTokenAsync();

                logger.LogInformation("Token recieved.");

                logger.LogInformation("Executing UploadFileService.");

                var processFileData = new Dictionary<string, List<string>>();
                foreach (var key in filesToUpload.Keys)
                {
                    if (key == "Grey Water Management")
                    {
                        processFileData.Add("grey", filesToUpload[key]);
                    }
                    else
                    {
                        processFileData.Add("solid", filesToUpload[key]);
                    }
                }

                var uploadFileService = new CustomUploadFileService(uploadAutomationSettings);
                var result = await uploadFileService.UploadFiles(token, processFileData);

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
                throw ex;
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
    }
}