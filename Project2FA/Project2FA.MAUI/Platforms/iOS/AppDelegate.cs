using Foundation;
using Project2FA.MAUI.Services;
using System;
using UIKit;

namespace Project2FA.MAUI;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    private NSUrl _url;
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
        string urlPath = url.Path;
        if (_url != null)
        {
            //release the access
            _url.StopAccessingSecurityScopedResource();
        }
        _url = url;

        if (!NSFileManager.DefaultManager.IsReadableFile(urlPath))
        {

            if (url.StartAccessingSecurityScopedResource())
            {
                //data = NSData.FromFile(urlPath);
                //url.StopAccessingSecurityScopedResource();
            }
        }
        else
        {
            //data = NSData.FromFile(urlPath);
        }


        //NSString str = NSString.FromData(data, NSStringEncoding.UTF8);
        DataService.Instance.StorageFileUrl = url.Path.ToString();
        MauiProgram.FileActivationIOS();
        return true;

        //return base.OpenUrl(app, url, options);
    }

    //public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    //{

    //    return base.FinishedLaunching(application, launchOptions);
    //}


    public override void WillTerminate(UIApplication application)
    {
        //release the access
        if (_url != null)
        {
            _url.StopAccessingSecurityScopedResource();
        }
        base.WillTerminate(application);
    }
}
