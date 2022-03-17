using System.Threading.Tasks;
#if HAS_WINUI
using Microsoft.Storage;
#else
using Windows.Storage;
#endif

namespace Project2FA.Uno.Core.File
{
    public interface IFileService
    {
        /// <summary>Deletes a file in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns>Boolean: true if deleted, false if not deleted</returns>
        Task<bool> DeleteFileAsync(string key, StorageStrategies location = StorageStrategies.Local, string path = null);

        /// <summary>Deletes a file in the specified folder/summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns>Boolean: true if deleted, false if not deleted</returns>
        Task<bool> DeleteFileAsync(string key, StorageFolder folder);

        /// <summary>Returns if a file is found in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns>Boolean: true if found, false if not found</returns>
        Task<bool> FileExistsAsync(string key, StorageStrategies location = StorageStrategies.Local, string path = null);

        /// <summary>Returns if a file is found in the specified folder</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns>Boolean: true if found, false if not found</returns>
        Task<bool> FileExistsAsync(string key, StorageFolder folder);

        /// <summary>Reads and deserializes a file into specified type T</summary>
        /// <typeparam name="T">Specified type into which to deserialize file content</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns>Specified type T</returns>
        Task<T> ReadFileAsync<T>(string key, StorageStrategies location = StorageStrategies.Local, string path = null);

        /// <summary>Reads and deserializes a file into specified type T</summary>
        /// <typeparam name="T">Specified type into which to deserialize file content</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns>Specified type T</returns>
        Task<T> ReadFileAsync<T>(string key, StorageFolder folder);

        /// <summary>Reads and deserializes a file as string</summary>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns>string</returns>
        Task<string> ReadStringAsync(string key, StorageStrategies location = StorageStrategies.Local, string path = null);

        /// <summary>Reads and deserializes a file as string</summary>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns>string</returns>
        Task<string> ReadStringAsync(string key, StorageFolder folder);

        /// <summary>Serializes an object and write to file in specified storage strategy</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="location">Location storage strategy</param>
        /// <param name="option">defines the collision handling</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns></returns>
        Task<bool> WriteFileAsync<T>(string key, T value, StorageStrategies location = StorageStrategies.Local, CreationCollisionOption option = CreationCollisionOption.ReplaceExisting, string path = null);

        /// <summary>Serializes an object and write to file in specified storage strategy</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns></returns>
        Task<bool> WriteFileAsync<T>(string key, T value, StorageFolder folder);

        /// <summary>Write string to file in specified storage strategy</summary>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="option">defines the collision handling</param>
        /// <param name="path">Custom path for storage. Only accessible if rights have been granted</param>
        /// <returns></returns>
        Task<bool> WriteStringAsync(string key, string value, StorageStrategies location = StorageStrategies.Local, CreationCollisionOption option = CreationCollisionOption.ReplaceExisting, string path = null);

        /// <summary>Write string to file in specified storage strategy</summary>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="folder">Parent folder of the file. Only accessible if rights have been granted</param>
        /// <returns></returns>
        Task<bool> WriteStringAsync(string key, string value, StorageFolder folder);
    }
}