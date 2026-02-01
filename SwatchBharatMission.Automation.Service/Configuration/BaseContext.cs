using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public class BaseContext
    {
        public string AppRootPath { get; set; }
        public string Flow { get; set; }
        public bool IsFirstRun { get; set; }
    }
}
