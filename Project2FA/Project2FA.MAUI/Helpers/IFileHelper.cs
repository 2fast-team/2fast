using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.Helpers
{
    public interface IFileHelper
    {
        Task<bool> StartAccessFile(string path);
        Task<bool> StopAccessFile(string path);
    }
}
