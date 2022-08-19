using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Project2FA.MAUI.Helpers
{
    public class FileHelperForIOS : FileHelper
    {
        internal override bool PlatformStartAccessFile(string path)
        {
            try
            {
                NSUrl filePath = NSUrl.FromString(path);

                return filePath.StartAccessingSecurityScopedResource();
            }
            catch (Exception)
            {

                return false;
            }
        }

        internal override bool PlatformStopAccessFile(string path)
        {
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
        }
    }
}
