using Foundation;
using ObjCRuntime;
using UIKit;

namespace Project2FA.UNO.iOS
{
    //public class AppDelegate : IUIApplicationDelegate
    //{
    //    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    //    {
    //        string urlPath = url.Path;
    //        if (_url != null)
    //        {
    //            //release the access
    //            _url.StopAccessingSecurityScopedResource();
    //        }
    //        _url = url;

    //        if (!NSFileManager.DefaultManager.IsReadableFile(urlPath))
    //        {

    //            if (url.StartAccessingSecurityScopedResource())
    //            {
    //                //data = NSData.FromFile(urlPath);
    //                //url.StopAccessingSecurityScopedResource();
    //            }
    //        }
    //        else
    //        {
    //            //data = NSData.FromFile(urlPath);
    //        }


    //        //NSString str = NSString.FromData(data, NSStringEncoding.UTF8);
    //        DataService.Instance.StorageFileUrl = url.Path.ToString();
    //        MauiProgram.FileActivationIOS();
    //        return true;

    //        //return base.OpenUrl(app, url, options);
    //    }
    //}
}
