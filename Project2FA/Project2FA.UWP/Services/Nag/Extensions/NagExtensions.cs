using System;
using Project2FA.Services.Nag;

namespace Project2FA.Extensions
{
    public static class NagExtensions
    {
        internal static UNOversal.Services.File.StorageStrategies ToFileServiceStrategy(this Services.Nag.NagStorageStrategies strategy)
        {
            switch (strategy)
            {
                case NagStorageStrategies.Local: return UNOversal.Services.File.StorageStrategies.Local;
                case NagStorageStrategies.Roaming: return UNOversal.Services.File.StorageStrategies.Roaming;
                case NagStorageStrategies.Temporary: return UNOversal.Services.File.StorageStrategies.Temporary;
                default:
                    throw new NotSupportedException(strategy.ToString());
            }
        }
    }
}
