using Automation.Core;
using Microsoft.Playwright;
using Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TestCases.common;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace TestCases
{
    public class TC_005_UploadCityDataToServer_MR_166B : ITestCase
    {
        public string TestCaseId => "TC_005_UploadCityDataToServer_MR_166B";

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

                await page.ClickAsync("#RptrPhaseIIMISOtherReport_Households_CSC_ctl04_lnkbtn_PageLinkHeader");
                await page.WaitForLoadStateAsync(LoadState.Load);
                logger.LogInformation("State Page Loaded.");

                await page.Locator($"a:text-matches('{state}', 'i')").ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.Load);

                logger.LogInformation("City Page Loaded.");
                await page.ClickAsync($"a:has-text('{city}')");

                await page.WaitForLoadStateAsync(LoadState.Load);

                logger.LogInformation("Sub City Page loaded");
                // Get block count (retry once if not loaded)
                int blockCount = await page.Locator("a[id$='_lnkBlockName']").CountAsync();
                if (blockCount == 0)
                {
                    logger.LogInformation("Waiting for delay 5000ms.");
                    await Task.Delay(5000);
                    blockCount = await page.Locator("a[id$='_lnkBlockName']").CountAsync();
                }

                logger.LogInformation($"Total City Count: {blockCount}");
                try
                {

                    for (int i = 0; i < blockCount; i++)
                    {
                        await page.WaitForLoadStateAsync(LoadState.Load);
                        var block = page.Locator("a[id$='_lnkBlockName']").Nth(i);
                        string blockName = (await block.InnerTextAsync()).Trim();

                        logger.LogInformation($"Clicking block: {blockName}");

                        await block.ClickAsync();

                        await page.WaitForLoadStateAsync(LoadState.Load);
                        var download = await page.RunAndWaitForDownloadAsync(async () =>
                        {
                            await page.Locator("#ctl00_ContentPlaceHolder1_btnExcel").ClickAsync();
                        });

                        // Prepare temp directory
                        string tempDir = Path.Combine(Path.GetTempPath(), "AutomationFiles");
                        Directory.CreateDirectory(tempDir);

                        // Build file path
                        string fileName = $"{SanitizeFileName(blockName)}_{Guid.NewGuid()}_{download.SuggestedFilename}";

                        string filePath = Path.Combine(tempDir, fileName);

                        await download.SaveAsAsync(filePath);
                        filesToUpload.Add(filePath);

                        // Go back

                        await page.ClickAsync("#ctl00_ContentPlaceHolder1_lnk_Back");

                    }
                }
                catch (Exception ex)
                {
                    logger.LogInformation("Exception Occurred" + ex.Message);
                    throw;
                }

                logger.LogInformation("Executing Get Token.");
                // Upload files
                var authService = new AuthService(automationContext);
                string token = await authService.GetTokenAsync();

                logger.LogInformation("Token recieved.");


                logger.LogInformation("Executing UploadFileService.");
                var uploadFileService = new UploadFileService(automationContext);
                var result = await uploadFileService.UploadFiles(token, filesToUpload, TestCaseId);

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
                logger.LogInformation("Exception Occurred " + ex.Message);
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