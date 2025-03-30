using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using UNOversal.Ioc;
using UNOversal.Services.Serialization;
#if WINDOWS_UWP
using Project2FA.UWP;
#else
using Project2FA.UnoApp;
#endif


namespace Project2FA.Utils
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
