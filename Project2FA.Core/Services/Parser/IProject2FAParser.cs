using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.Core.Services.Parser
{
    public interface IProject2FAParser
    {
        List<KeyValuePair<string, string>> ParseQRCodeStr(string qrCodeStr);

        List<KeyValuePair<string, string>> ParseCmdStr(string cmdStr);
    }
}
