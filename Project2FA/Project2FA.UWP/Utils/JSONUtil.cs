using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Template10.Services.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using Prism.Ioc;

namespace Project2FA.UWP.Utils
{
    public class JSONUtil<T>
    {
        public async Task<ObservableCollection<T>> GetJSONDataAsync(Uri uri)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                IRandomAccessStreamWithContentType randomStream = await file.OpenReadAsync();
                using (StreamReader r = new StreamReader(randomStream.AsStreamForRead()))
                {
                    return App.Current.Container.Resolve<ISerializationService>().Deserialize<ObservableCollection<T>>(await r.ReadToEndAsync());
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
