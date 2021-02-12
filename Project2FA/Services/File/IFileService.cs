using System.Threading.Tasks;
using System.IO;

namespace Project2FA.Services.File
{
    public interface IFileService
    {
        bool FileExists(string key, StorageStrategies location = StorageStrategies.Local, string path = null);
        bool DeleteFile(string key, StorageStrategies location = StorageStrategies.Local, string path = null);
        Task<T> ReadFileAsync<T>(string key, StorageStrategies location = StorageStrategies.Local, string path = null);
        string ReadString(string key, StorageStrategies location = StorageStrategies.Local, string path = null);
        Task<bool> WriteFileAsync<T>(string key, T value, StorageStrategies location = StorageStrategies.Local, string path = null);
        void WriteStringAsync (string key, string value, StorageStrategies location = StorageStrategies.Local, string path = null);
    }
}