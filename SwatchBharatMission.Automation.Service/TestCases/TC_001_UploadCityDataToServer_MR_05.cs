using Automation.Core;
using Microsoft.Playwright;
using Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using TestCases.common;
using System.Net;
namespace TestCases
{
    public class TC_001_UploadCityDataToServer_MR_05 : ITestCase
    {
        public string TestCaseId => "TC_001";

        public async Task ExecuteAsync(UploadAutomationSettings uploadAutomationSettings, AutomationSettings automationSettings, ILogger logger)
        {
            var filesToUpload = new List<string>();
            await using var context = await PlaywrightFactory.CreateAsync(automationSettings.Headless);
            var page = context.Page;
            logger.LogInformation($"TestCase {TestCaseId} Started for : {uploadAutomationSettings.TenantCode} ");

            try
            {
                string state = uploadAutomationSettings.State;
                string city = uploadAutomationSettings.TenantCode;
                uploadAutomationSettings.TestCaseName = GetType().Name;

                // Navigate
                await page.GotoAsync(automationSettings.BaseUrl);
                await page.ClickAsync("#RptrOdfPlusReports_ctl01_lnkbtn_PageLinkHeader");
                await page.Locator($"a:text-matches('{state}', 'i')")
                .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

                await page.Locator($"a:text-matches('{state}', 'i')").ClickAsync();
                await page.ClickAsync($"a:has-text('{city}')");

                // Get block count (retry once if not loaded)
                int blockCount = await page.Locator("a[id$='_lnk_BlockName']").CountAsync();
                if (blockCount == 0)
                {
                    await Task.Delay(2000);
                    blockCount = await page.Locator("a[id$='_lnk_BlockName']").CountAsync();
                }

                logger.LogInformation($"Total City Count: {blockCount}");
                try
                {

                    for (int i = 0; i < blockCount; i++)
                    {
                        var block = page.Locator("a[id$='_lnk_BlockName']").Nth(i);
                        string blockName = (await block.InnerTextAsync()).Trim();

                        logger.LogInformation($"Clicking block: {blockName}");

                        await block.ClickAsync();

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
                        await page.ClickAsync("#ctl00_ContentPlaceHolder1_lnk_back");
                    }

                }
                catch (Exception ex)
                {
                    logger.LogInformation("Exception Occurred" + ex.Message);
                }

                logger.LogInformation("Executing Get Token.");
                // Upload files
                var authService = new AuthService(uploadAutomationSettings);
                string token = await authService.GetTokenAsync();

                logger.LogInformation("Token recieved.");


                logger.LogInformation("Executing UploadFileService.");
                var uploadFileService = new UploadFileService(uploadAutomationSettings);
                var result = await uploadFileService.UploadFiles(token, filesToUpload);

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