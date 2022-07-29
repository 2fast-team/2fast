using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.MAUI.Helpers
{
    public partial class FileHelper
    {
        public bool StartAccessFile(string path)
            => PlatformStartAccessFile(path);

        public bool StopAccessFile(string path)
             => PlatformStopAccessFile(path);
    }
}
