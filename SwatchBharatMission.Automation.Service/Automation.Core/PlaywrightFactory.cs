using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automation.Core
{
    public static class PlaywrightFactory 
    {
        public static async Task<TestContext> CreateAsync(bool headLess)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions { Headless = headLess });

            var page = await browser.NewPageAsync();
            return new TestContext(playwright,browser, page);
        }
    }
}
