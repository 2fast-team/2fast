using System.Threading.Tasks;

namespace Project2FA.Uno.Core.Network
{
    public interface INetworkService 
    {
        Task<bool> GetIsInternetAvailableAsync();
        Task<bool> GetIsNetworkAvailableAsync();
    }
}