using System;
using System.Collections.Generic;
using System.Text;
using WebDAVClient.Exceptions;

namespace Project2FA.Core.Services.WebDAV
{
    public interface IResponseErrorHandlerService
    {
        void HandleException(ResponseError e);
    }
}
