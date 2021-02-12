using DecaTec.WebDav;
using DecaTec.WebDav.Tools;
using WebDAVClient.Types;
using WebDAVClient.WebDav;
using System;
using System.Xml.Linq;

namespace WebDAVClient.Extensions
{
    /// <summary>
    /// Creates a <see cref="ResourceInfoModel"/> from a <see cref="WebDavSessionItem"/>.
    /// </summary>
    public static class WebDavSessionItemExtensions
    {
        public const string NsOc = "http://owncloud.org/ns";

        public static ResourceInfoModel ToResourceInfo(this WebDavSessionItem item, Uri baseUri)
        {
            var res = new ResourceInfoModel
            {
                ContentType = item.ContentType,
                IsDirectory = item.IsFolder.HasValue ? true: false,
                Created = item.CreationDate ?? DateTime.MinValue,
                ETag = item.ETag,
                LastModified = item.LastModified ?? DateTime.MinValue,
                Name = item.Name,
                QuotaAvailable = item.QuotaAvailableBytes ?? 0,
                QuotaUsed = item.QuotaUsedBytes ?? 0,
                Size = item.ContentLength.HasValue && item.ContentLength.Value != 0 ? item.ContentLength.Value : item.QuotaUsedBytes ?? 0,
                Path = Uri.UnescapeDataString(item.Uri.AbsoluteUri.Replace(baseUri.AbsoluteUri, ""))
            };

            // NC specific properties.
            var ncProps = item.AdditionalProperties;

            if(ncProps != null)
            {
                var key = XName.Get(WebDAVPropNameConstants.Checksums, NsOc);

                if (ncProps.ContainsKey(key))
                    res.Checksums = ncProps[key];

                key = XName.Get(WebDAVPropNameConstants.CommentsCount, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    var commentsCount = ncProps[key];
                    res.CommentsCount = string.IsNullOrEmpty(commentsCount) ? 0 : long.Parse(commentsCount);
                }

                key = XName.Get(WebDAVPropNameConstants.CommentsHref, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    var commentsHref = ncProps[key];
                    res.CommentsHref = string.IsNullOrEmpty(commentsHref) ? null : UriHelper.CombineUri(baseUri, new Uri(commentsHref, UriKind.Relative));
                }

                key = XName.Get(WebDAVPropNameConstants.CommentsUnread, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    var commentsUnread = ncProps[key];
                    res.CommentsUnread = string.IsNullOrEmpty(commentsUnread) ? 0 : long.Parse(commentsUnread);
                }

                key = XName.Get(WebDAVPropNameConstants.Favorite, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    var favorite = ncProps[key];
                    res.IsFavorite = string.IsNullOrEmpty(favorite) ? false : string.CompareOrdinal(favorite, "1") == 0 ? true : false;
                }

                key = XName.Get(WebDAVPropNameConstants.FileId, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    res.FileId = ncProps[key];
                }

                key = XName.Get(WebDAVPropNameConstants.HasPreview, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    var hasPreview = ncProps[key];
                    res.HasPreview = string.IsNullOrEmpty(hasPreview) ? false : string.CompareOrdinal(hasPreview, "1") == 0 ? true : false;
                }

                key = XName.Get(WebDAVPropNameConstants.Id, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    res.Id = ncProps[key];
                }

                key = XName.Get(WebDAVPropNameConstants.OwnerDisplayName, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    res.OwnerDisplayName = ncProps[key];
                }

                key = XName.Get(WebDAVPropNameConstants.OwnerId, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    res.OwnderId = ncProps[key];
                }

                key = XName.Get(WebDAVPropNameConstants.ShareTypes, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    var shareType = ncProps[key];

                    if(!string.IsNullOrEmpty(shareType))
                        res.ShareTypes = (OcsShareType)int.Parse(ncProps[key]);
                }

                key = XName.Get(WebDAVPropNameConstants.Size, NsOc);

                if (ncProps.ContainsKey(key))
                {
                    var size = ncProps[key];
                    res.Size = string.IsNullOrEmpty(size) ? 0 : long.Parse(size);
                }
            }

            return res;
        }
    }
}
