using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if IOS
using Foundation;
#endif

namespace Project2FA.MAUI.Helpers
{
    public partial class FileHelper
    {
        internal virtual bool PlatformStartAccessFile(string path)
        {

#if IOS
            try
            {
                NSString urlString = new NSString(string.Format("file://{0}",path));
                NSUrl filePath = NSUrl.CreateFileUrl(path, false, null); //new NSUrl(urlString);
                return filePath.StartAccessingSecurityScopedResource();
            }
            catch (Exception exc)
            {

                return false;
            }
#else
            return true;
#endif
        }

        internal virtual bool PlatformStopAccessFile(string path)
        {
#if IOS
            try
            {
                NSUrl filePath = NSUrl.FromString(path);
                filePath.StopAccessingSecurityScopedResource();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
#else
            return true;
#endif
        }
    }
}
