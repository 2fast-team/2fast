using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDAVClient;

namespace Project2FA.UWP.Services.WebDAV
{
    public interface IWebDAVClientService
    {
        WebDAVClientService Instance { get; }
        Task<Client> GetClient();
    }
}
