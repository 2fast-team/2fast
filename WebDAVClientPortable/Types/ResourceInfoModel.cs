using System;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WebDAVClient.Types
{
    /// <summary>
    /// File or directory information
    /// </summary>
    public class ResourceInfoModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the base name of the file without path
        /// </summary>
        /// <value>name of the file</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full path to the file without name and without trailing slash
        /// </summary>
        /// <value>path to the file</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes
        /// </summary>
        /// <value>size of the file in bytes</value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the file content type
        /// </summary>
        /// <value>file etag</value>
        public string ETag { get; set; }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <value>file content type</value>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets if the file is a directory
        /// </summary>
        /// <returns></returns>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Gets or sets the last modified time
        /// </summary>
        /// <value>last modified time</value>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the creation time
        /// </summary>
        /// <value>creation time</value>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the quota used in bytes.
        /// </summary>
        /// <value>The quota used in bytes.</value>
        public long QuotaUsed { get; set; }

        /// <summary>
        /// Gets or sets the quota available in bytes.
        /// </summary>
        /// <value>The quota available in bytes.</value>
        public long QuotaAvailable { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = "ResourceInfo {\n";
            sb += "\tName: " + Name + "\n";
            sb += "\tPath: " + Path + "\n";
            sb += "\tSize: " + Size + "\n";
            sb += "\tETag: " + ETag + "\n";
            sb += "\tContentType: " + ContentType + "\n";
            sb += "\tLastModified: " + LastModified + "\n";
            sb += "\tCreated: " + Created + "\n";
            sb += "\tQuotaUsed: " + QuotaUsed + "\n";
            sb += "\tQuotaAvailable: " + QuotaAvailable + "\n";
            sb += "}";
            return sb;
        }



        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        #region Nextcloud specific

        // See https://docs.nextcloud.com/server/12/developer_manual/client_apis/WebDAV/index.html for a list of all NC specific WebDAV properties.

        /// <summary>
        /// Gets or sets the ID (the fileid namespaced by the instance id, globally unique).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the FileID (the unique id for the file within the instance).
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Gets or sets IsFavorite.
        /// </summary>
        private bool _isFavorite;
        public bool IsFavorite
        {
            get
            {
                return _isFavorite;
            }
            set
            {
                SetProperty(ref _isFavorite, value);
            }
        }

        /// <summary>
        /// Gets or sets CommentsHref (link to comments).
        /// </summary>
        public Uri CommentsHref { get; set; }

        /// <summary>
        /// Gets or sets the count of comments.
        /// </summary>
        public long CommentsCount { get; set; }

        /// <summary>
        /// Gets or sets the number of comments unread.
        /// </summary>
        public long CommentsUnread { get; set; }

        /// <summary>
        /// Gets or sets the OwnerId (the user id of the owner of a shared file).
        /// </summary>
        public string OwnderId { get; set; }

        /// <summary>
        /// Gets or sets the owner display name (the display name of the owner of a shared file).
        /// </summary>
        public string OwnerDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the NextcloudShareTypes.
        /// </summary>
        public OcsShareType ShareTypes { get; set; }

        /// <summary>
        /// Gets or sets the Checksums.
        /// </summary>
        public string Checksums { get; set; }

        /// <summary>
        /// Gets or sets HasPreview.
        /// </summary>
        public bool HasPreview { get; set; }


        #endregion Nextcloud specific

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = obj as ResourceInfoModel;
            return Equals(other);
        }

        public bool Equals(ResourceInfoModel other)
        {
            if (other == null)
            {
                return false;
            }

            return GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(ResourceInfoModel a, ResourceInfoModel b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if ((object) a == null || ((object) b == null))
            {
                return false;
            }

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(ResourceInfoModel a, ResourceInfoModel b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            var hashCode =
                Created.GetHashCode() ^
                LastModified.GetHashCode() ^
                Size.GetHashCode() ^
                IsFavorite.GetHashCode() ^
                CommentsCount.GetHashCode() ^
                CommentsUnread.GetHashCode() ^
                ShareTypes.GetHashCode() ^
                HasPreview.GetHashCode();

            if (ContentType != null)
            {
                hashCode ^= ContentType.GetHashCode();
            }

            if (Name != null)
            {
                hashCode ^= Name.GetHashCode();
            }

            if (Path != null)
            {
                hashCode ^= Path.GetHashCode();
            }

            if (CommentsHref != null)
            {
                hashCode ^= CommentsHref.GetHashCode();
            }

            // Is null on directories
            if (ETag != null)
            {
                hashCode ^= ETag.GetHashCode();
            }

            if (!string.IsNullOrEmpty(Id))
            {
                hashCode ^= Id.GetHashCode();
            }

            if (!string.IsNullOrEmpty(FileId))
            {
                hashCode ^= FileId.GetHashCode();
            }

            if (!string.IsNullOrEmpty(OwnderId))
            {
                hashCode ^= OwnderId.GetHashCode();
            }

            if (!string.IsNullOrEmpty(OwnerDisplayName))
            {
                hashCode ^= OwnerDisplayName.GetHashCode();
            }

            if (!string.IsNullOrEmpty(Checksums))
            {
                hashCode ^= Checksums.GetHashCode();
            }

            return hashCode;
        }

        #endregion Equals
    }
}

