using System.Threading.Tasks;

namespace Project2FA.Services.Marketplace
{
    public interface IMarketplaceService
    {
        Task LaunchAppInStore();

        Task LaunchAppReviewInStoreAsync();

        Task LaunchPublisherPageInStoreAsync();

        //NagEx CreateAppReviewNag();

        //NagEx CreateAppReviewNag(string message);
    }
}
