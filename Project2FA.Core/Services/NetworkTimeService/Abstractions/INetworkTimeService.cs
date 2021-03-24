using System;
using System.Net;
using System.Threading.Tasks;

namespace Project2FA.Core.Services.NTP
{
    public interface INetworkTimeService
    {
        Task<DateTime> GetNetworkTimeAsync();
        Task<DateTime> GetNetworkTimeAsync(string ntpServer);
        Task<DateTime> GetNetworkTimeAsync(IPEndPoint ep);
    }
}
