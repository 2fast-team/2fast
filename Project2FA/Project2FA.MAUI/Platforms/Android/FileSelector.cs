using Android.Content;
using Android.Provider;
using Project2FA.MAUI.Services.FileSelector;
using Uri = Android.Net.Uri;
using Application = Android.App.Application;
using Environment = Android.OS.Environment;
using Android.OS;

namespace Project2FA.MAUI.Platforms.Android
{
    public class FileSelector : IFileSelector
    {
        public Task SelectFileAsync()
        {
            var currentActivity = Platform.CurrentActivity;
            if (currentActivity is null)
            {
                throw new Exception("Could not get the current activity");
            }

            if ((int)Build.VERSION.SdkInt >= 30)
            {
                // Makes sure the app has ACTION_MANAGE_ALL_FILES_ACCESS_PERMISSION before trying to read the file.
                if (!Environment.IsExternalStorageManager)
                {
                    var uri = Uri.Parse($"package:{Application.Context?.ApplicationInfo?.PackageName}");
                    var permissionIntent = new Intent(Settings.ActionManageAppAllFilesAccessPermission, uri);
                    currentActivity.StartActivity(permissionIntent);
                }
            }
            else
            {
                // use the legacy permission via manifest to access all files
            }


            var intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("application/json");

            intent.PutExtra(DocumentsContract.ExtraInitialUri, MediaStore.Downloads.ExternalContentUri);

            currentActivity.StartActivityForResult(intent, 1);

            return Task.CompletedTask;
        }
    }
}
