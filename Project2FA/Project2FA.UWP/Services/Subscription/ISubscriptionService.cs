using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace Project2FA.UWP.Services
{
    public interface ISubscriptionService
    {
        Task SetupSubscriptionInfoAsync();
    }
}
