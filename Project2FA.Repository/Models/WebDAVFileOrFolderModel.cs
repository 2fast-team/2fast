using WebDAVClient.Types;

namespace Project2FA.Repository.Models
{
    public class WebDAVFileOrFolderModel : ResourceInfoModel
    {
        public WebDAVFileOrFolderModel()
        {
        }

        public WebDAVFileOrFolderModel(ResourceInfoModel item)
        {
            Name = item.Name;
            Path = item.Path;
            Size = item.Size;
            ETag = item.ETag;
            IsDirectory = item.IsDirectory;
            ContentType = item.ContentType;
            LastModified = item.LastModified;
            Created = item.Created;
            QuotaUsed = item.QuotaUsed;
            QuotaAvailable = item.QuotaAvailable;
            Id = item.Id;
            FileId = item.FileId;
            IsFavorite = item.IsFavorite;
            CommentsHref = item.CommentsHref;
            CommentsCount = item.CommentsCount;
            CommentsUnread = item.CommentsUnread;
            OwnderId = item.OwnderId;
            OwnerDisplayName = item.OwnerDisplayName;
            ShareTypes = item.ShareTypes;
            Checksums = item.Checksums;
            HasPreview = item.HasPreview;
            Glyph = SetTypeGlyph();
        }

        private string SetTypeGlyph()
        {
            if (IsDirectory)
            {
                return "\U0001F4C1";
            }
            else
            {
                switch (ContentType)
                {
                    case "application/2fa":
                        return "\U0001F511";
                    default:
                        return "\U0001F511"; //"\uE8D7";//return "\uE8A5"; //📁
                }
            }
        }
        private string _glyph;
        public string Glyph
        {
            get
            {
                return _glyph;
            }

            set
            {
                SetProperty(ref _glyph, value);
            }
        }
    }
}
