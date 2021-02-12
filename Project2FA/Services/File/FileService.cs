using System;
using System.IO;
using System.Threading.Tasks;
using Project2FA.Core.Services.JSON;

namespace Project2FA.Services.File
{
    public class FileService : IFileService
    {
        INewtonsoftJSONService _serializer;
        public FileService()
        {
            _serializer = new NewtonsoftJSONService();
        }
        public bool DeleteFile(string key, StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            string tmpPath = string.Empty;
            switch (location)
            {
                case StorageStrategies.Local:
                    tmpPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), key);
                    break;
                case StorageStrategies.Personal:
                    tmpPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), key);
                    break;
                case StorageStrategies.Custom:
                    tmpPath = Path.Combine(path, key);
                    break;
                default:
                    break;
            }
            if (System.IO.File.Exists(tmpPath))
            {
                System.IO.File.Delete(tmpPath);
                if (!System.IO.File.Exists(tmpPath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;//not exists
            }
        }

        public bool FileExists(string key, StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            string tmpPath = string.Empty;
            switch (location)
            {
                case StorageStrategies.Local:
                    tmpPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), key);
                    break;
                case StorageStrategies.Personal:
                    tmpPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), key);
                    break;
                case StorageStrategies.Custom:
                    tmpPath = Path.Combine(path, key);
                    break;
                default:
                    break;
            }
            return System.IO.File.Exists(tmpPath);
        }

        //private bool FileExists(string path)
        //{
        //    return System.IO.File.Exists(path);
        //}

        public Task<T> ReadFileAsync<T>(string key, StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            throw new NotImplementedException();
        }

        public string ReadString(string key, StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            string tmpPath = string.Empty;
            switch (location)
            {
                case StorageStrategies.Local:
                    tmpPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), key);
                    break;
                case StorageStrategies.Personal:
                    tmpPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), key);
                    break;
                case StorageStrategies.Custom:
                    tmpPath = Path.Combine(path, key);
                    break;
                default:
                    break;
            }
            if (System.IO.File.Exists(tmpPath))
            {
                return System.IO.File.ReadAllText(tmpPath);
            }
            else
            {
                return string.Empty;
            }
            
        }

        public Task<bool> WriteFileAsync<T>(string key, T value, StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            throw new NotImplementedException();
        }

        public async void WriteStringAsync(string key, string value, StorageStrategies location = StorageStrategies.Local, string path = null)
        {
            string tmpPath = string.Empty;
            switch (location)
            {
                case StorageStrategies.Local:
                    tmpPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), key);
                    break;
                case StorageStrategies.Personal:
                    tmpPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), key);
                    break;
                case StorageStrategies.Custom:
                    tmpPath = Path.Combine(path, key);
                    break;
                default:
                    break;
            }
            if (System.IO.File.Exists(tmpPath))
            {
                await System.IO.File.WriteAllTextAsync(tmpPath,value, new System.Threading.CancellationToken(false));
            }
            //else
            //{
            //    System.IO.File.Create(tmpPath);
            //}
        }

        private string Serialize<T>(T item)
            => _serializer.Serialize(item);

        private T Deserialize<T>(string json)
            => _serializer.Deserialize<T>(json);
    }
}