using System;
using System.Collections.Generic;
using System.Text;
using WebDAVClient.Exceptions;

namespace Project2FA.Core.Services.WebDAV
{
    public class ResponseErrorHandlerService : IResponseErrorHandlerService
    {
        public void HandleException(ResponseError e)
        {
            throw new NotImplementedException();
        }
    }
}
