using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using Android.Util;
using Android.Widget;
using Project2FA.MAUI.Services;
using Android.Database;
using Android.Provider;
using Android.Webkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using AndroidUri = Android.Net.Uri;
using Application = Android.App.Application;
using Debug = System.Diagnostics.Debug;

namespace Project2FA.MAUI
{

    // based on https://github.com/dotnet/maui/blob/6dfb8c6b60a1200079abd7918bb96e0f1b3587c4/src/Essentials/src/FileSystem/FileSystemUtils.android.cs
    public class FileSystemHelper
    {
        const string storageTypePrimary = "primary";
        const string storageTypeRaw = "raw";
        const string storageTypeImage = "image";
        const string storageTypeVideo = "video";
        const string storageTypeAudio = "audio";

        static readonly string[] contentUriPrefixes =
{
            "content://downloads/public_downloads",
            "content://downloads/my_downloads",
            "content://downloads/all_downloads",
        };

        internal const string UriSchemeFile = "file";
        internal const string UriSchemeContent = "content";

        internal const string UriAuthorityExternalStorage = "com.android.externalstorage.documents";
        internal const string UriAuthorityDownloads = "com.android.providers.downloads.documents";
        internal const string UriAuthorityMedia = "com.android.providers.media.documents";
        public static string ResolvePhysicalPath(AndroidUri uri, bool requireExtendedAccess = true)
        {
            if (uri.Scheme.Equals(UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                // if it is a file, then return directly

                var resolved = uri.Path;
                if (File.Exists(resolved))
                    return resolved;
            }
            else if (!requireExtendedAccess || !OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                // if this is on an older OS version, or we just need it now

                if (OperatingSystem.IsAndroidVersionAtLeast(19) && DocumentsContract.IsDocumentUri(Application.Context, uri))
                {
                    var resolved = ResolveDocumentPath(uri);
                    if (File.Exists(resolved))
                        return resolved;
                }
                else if (uri.Scheme.Equals(UriSchemeContent, StringComparison.OrdinalIgnoreCase))
                {
                    var resolved = ResolveContentPath(uri);
                    if (File.Exists(resolved))
                        return resolved;
                }
            }

            return null;
        }

        static string ResolveDocumentPath(AndroidUri uri)
        {
            Debug.WriteLine($"Trying to resolve document URI: '{uri}'");

            var docId = DocumentsContract.GetDocumentId(uri);

            var docIdParts = docId?.Split(':');
            if (docIdParts == null || docIdParts.Length == 0)
                return null;

            if (uri.Authority.Equals(UriAuthorityExternalStorage, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Resolving external storage URI: '{uri}'");

                if (docIdParts.Length == 2)
                {
                    var storageType = docIdParts[0];
                    var uriPath = docIdParts[1];

                    // This is the internal "external" memory, NOT the SD Card
                    if (storageType.Equals(storageTypePrimary, StringComparison.OrdinalIgnoreCase))
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        var root = global::Android.OS.Environment.ExternalStorageDirectory.Path;
#pragma warning restore CS0618 // Type or member is obsolete

                        return Path.Combine(root, uriPath);
                    }

                    // TODO: support other types, such as actual SD Cards
                }
            }
            else if (uri.Authority.Equals(UriAuthorityDownloads, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Resolving downloads URI: '{uri}'");

                // NOTE: This only really applies to older Android vesions since the privacy changes

                if (docIdParts.Length == 2)
                {
                    var storageType = docIdParts[0];
                    var uriPath = docIdParts[1];

                    if (storageType.Equals(storageTypeRaw, StringComparison.OrdinalIgnoreCase))
                        return uriPath;
                }

                // ID could be "###" or "msf:###"
                var fileId = docIdParts.Length == 2
                    ? docIdParts[1]
                    : docIdParts[0];

                foreach (var prefix in contentUriPrefixes)
                {
                    var uriString = prefix + "/" + fileId;
                    var contentUri = AndroidUri.Parse(uriString);

                    if (GetDataFilePath(contentUri) is string filePath)
                        return filePath;
                }
            }
            else if (uri.Authority.Equals(UriAuthorityMedia, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Resolving media URI: '{uri}'");

                if (docIdParts.Length == 2)
                {
                    var storageType = docIdParts[0];
                    var uriPath = docIdParts[1];

                    AndroidUri contentUri = null;
                    if (storageType.Equals(storageTypeImage, StringComparison.OrdinalIgnoreCase))
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    else if (storageType.Equals(storageTypeVideo, StringComparison.OrdinalIgnoreCase))
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    else if (storageType.Equals(storageTypeAudio, StringComparison.OrdinalIgnoreCase))
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
#pragma warning disable CS0618
                    if (contentUri != null && GetDataFilePath(contentUri, $"{MediaStore.MediaColumns.Id}=?", new[] { uriPath }) is string filePath)
                        return filePath;
#pragma warning restore CS0618
                }
            }

            Debug.WriteLine($"Unable to resolve document URI: '{uri}'");

            return null;
        }

        static string ResolveContentPath(AndroidUri uri)
        {
            Debug.WriteLine($"Trying to resolve content URI: '{uri}'");

            if (GetDataFilePath(uri) is string filePath)
                return filePath;

            // TODO: support some additional things, like Google Photos if that is possible

            Debug.WriteLine($"Unable to resolve content URI: '{uri}'");

            return null;
        }


        public static string GetFileExtension(AndroidUri uri)
        {
            var mimeType = Application.Context.ContentResolver.GetType(uri);

            return mimeType != null
                ? MimeTypeMap.Singleton.GetExtensionFromMimeType(mimeType)
                : null;
        }

        static string GetDataFilePath(AndroidUri contentUri, string selection = null, string[] selectionArgs = null)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            const string column = MediaStore.Files.FileColumns.Data;
#pragma warning restore CS0618 // Type or member is obsolete

            // ask the content provider for the data column, which may contain the actual file path
            var path = GetColumnValue(contentUri, column, selection, selectionArgs);
            if (!string.IsNullOrEmpty(path) && Path.IsPathRooted(path))
                return path;

            return null;
        }

        static string QueryContentResolverColumn(AndroidUri contentUri, string columnName, string selection = null, string[] selectionArgs = null)
        {
            string text = null;

            var projection = new[] { columnName };
            using var cursor = Application.Context.ContentResolver.Query(contentUri, projection, selection, selectionArgs, null);
            if (cursor?.MoveToFirst() == true)
            {
                var columnIndex = cursor.GetColumnIndex(columnName);
                if (columnIndex != -1)
                    text = cursor.GetString(columnIndex);
            }

            return text;
        }

        static string GetColumnValue(AndroidUri contentUri, string column, string selection = null, string[] selectionArgs = null)
        {
            try
            {
                var value = QueryContentResolverColumn(contentUri, column, selection, selectionArgs);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            catch
            {
                // Ignore all exceptions and use null for the error indicator
            }

            return null;
        }
    }
}
