using Automation.Core;
using Microsoft.Playwright;
using Configuration;
using Microsoft.Extensions.Options;
using TestCases.common;
using System.Net;
namespace TestCases
{
    public class TC_001_UploadCityDataToServer : ITestCase
    {
        public string TestCaseId => "TC_001";

        public async Task ExecuteAsync(TestContext context, Dictionary<string, string> parameters, IOptions<AutomationSettings> options)
        {
            Console.WriteLine($"TestCase Started : {TestCaseId}");
            var page = context.Page;
            var filesToUpload = new List<string>();

            string state = parameters["State"];
            string city = parameters["City"];

            // Navigate
            await page.GotoAsync(options.Value.BaseUrl);
            await page.ClickAsync("#RptrOdfPlusReports_ctl01_lnkbtn_PageLinkHeader");
            await page.ClickAsync($"a:has-text('{state}')");
            await page.ClickAsync($"a:has-text('{city}')");

            // Get block count (retry once if not loaded)
            int blockCount = await page.Locator("a[id$='_lnk_BlockName']").CountAsync();
            if (blockCount == 0)
            {
                await Task.Delay(2000);
                blockCount = await page.Locator("a[id$='_lnk_BlockName']").CountAsync();
            }
            Console.WriteLine($"Total State Count: {blockCount}");
            try
            {

                for (int i = 0; i < blockCount; i++)
                {
                    var block = page.Locator("a[id$='_lnk_BlockName']").Nth(i);
                    string blockName = (await block.InnerTextAsync()).Trim();

                    Console.WriteLine($"Clicking block: {blockName}");

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
                Console.WriteLine("Exception Occurred"+ex.Message);
            }
            // Upload files
            //var authService = new AuthService(options);
            //string token = await authService.GetTokenAsync();

            //var uploadFileService = new UploadFileService(options);
            //var result = await uploadFileService.UploadFiles(token, filesToUpload);

            //if (!(result.Item1 == "Import completed successfully" &&
            //      result.Item2 == HttpStatusCode.OK))
            //{
            //    throw new Exception("Import failed.");
            //}

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
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