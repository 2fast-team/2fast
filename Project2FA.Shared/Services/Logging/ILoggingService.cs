using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2FA.Services.Logging
{
    public interface ILoggingService
    {
        Task Log(string message);
        Task LogException(Exception exc);
    }
}
