using Microsoft.Playwright;

namespace Automation.Core
{
    public class AutomationRunner
    {
        public async Task RunAsync()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions { Headless = true });

            var context = await browser.NewContextAsync(
                new BrowserNewContextOptions
                {
                    AcceptDownloads = true
                });

            var page = await context.NewPageAsync();

            await page.GotoAsync("https://sbm.gov.in/SBMPhase2/Secure/Entry/UserMenu.aspx");

            await page.ClickAsync("#RptrOdfPlusReports_ctl01_lnkbtn_PageLinkHeader");

            await page.Locator("a:has-text('Maharashtra')").ClickAsync();
            await page.Locator("a:has-text('THANE')").ClickAsync();


            int blockCount = await page.Locator("a[id$='_lnk_BlockName']").CountAsync();
            if (blockCount == 0)
            {
                await Task.Delay(2000);
                blockCount = await page.Locator("a[id$='_lnk_BlockName']").CountAsync();
            }

            for (int i = 0; i < blockCount; i++)
            {
                var block = page.Locator("a[id$='_lnk_BlockName']").Nth(i);

                string blockName = await block.InnerTextAsync();
                Console.WriteLine($"Clicking block: {blockName}");

                await block.ClickAsync();

                var download = await page.RunAndWaitForDownloadAsync(async () =>
                {
                    await page.RunAndWaitForNavigationAsync(async () =>
                    {
                        await page.Locator("#ctl00_ContentPlaceHolder1_btnExcel").ClickAsync();
                    });
                });

                string fileName = blockName + '_' + download.SuggestedFilename;

                Console.WriteLine($"Downloading file: {fileName}");

                await download.SaveAsAsync($"C:\\Downloads\\{fileName}");

                await page.ClickAsync("#ctl00_ContentPlaceHolder1_lnk_back");


                // 👉 Do your work after click (download, scrape, etc.)

                // Go back to block list page
            }
            Console.WriteLine("Press ENTER to close browser...");
            Console.ReadLine();
        }
    }
}
