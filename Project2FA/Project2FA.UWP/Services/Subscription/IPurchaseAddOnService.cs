using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace Project2FA.UWP.Services
{
    public interface IPurchaseAddOnService
    {
        Task<(bool IsActive, StoreLicense info)> SetupPurchaseAddOnInfoAsync();
        Task PromptUserToPurchaseAsync();
        void Initialize(string id);
    }
}
