using System;
using System.Collections.Generic;
using System.Text;

namespace Project2FA.Repository.Database
{
    public interface IProject2FARepository
    {
        IDatafileRepository Datafile { get; }

        IPasswordHashRepository Password { get; }
    }
}
