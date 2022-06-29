using Foundation;
using Project2FA.MAUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.Platforms.iOS
{
    public static class FileHelper
    {
        public static Task<bool> StartAccessFile(string path)
        {
            try
            {
                var url = NSUrl.FromString(path);
                url.StartAccessingSecurityScopedResource();
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public static Task<bool> StopAccessFile(string path)
        {
            try
            {
                var url = NSUrl.FromString(path);
                url.StopAccessingSecurityScopedResource();
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}
