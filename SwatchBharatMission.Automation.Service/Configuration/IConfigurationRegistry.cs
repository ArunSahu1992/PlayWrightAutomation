using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public interface IConfigurationRegistry
    {
        IReadOnlyList<UploadAutomationSettings> GetAll();
        UploadAutomationSettings GetByTenant(string tenantCode);
    }
}
