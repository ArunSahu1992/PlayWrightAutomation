using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Automation.Core
{

    public class TestContext
    {
        public IPage Page { get; }
        public IBrowser Browser { get; }

        public TestContext(IBrowser browser, IPage page)
        {
            Browser = browser;
            Page = page;
        }
    }
}
