using WebDAVClient.Types;

namespace Project2FA.Repository.Models
{
    public class PathInfoModel
    {
        public ResourceInfoModel ResourceInfo { get; set; }

        public bool IsRoot { get; set; }
    }
}
