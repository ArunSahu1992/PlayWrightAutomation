using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Automation.Core
{

    public sealed class TestContext : IAsyncDisposable
    {
        private readonly IPlaywright _playwright;

        public IBrowser Browser { get; }
        public IPage Page { get; }

        public TestContext(
            IPlaywright playwright,
            IBrowser browser,
            IPage page)
        {
            _playwright = playwright;
            Browser = browser;
            Page = page;
        }

        public async ValueTask DisposeAsync()
        {
            if (Page != null)
                await Page.CloseAsync();

            if (Browser != null)
                await Browser.CloseAsync();

            _playwright?.Dispose();
        }
    }
}
