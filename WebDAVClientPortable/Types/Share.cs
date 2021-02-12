namespace WebDAVClient.Types
{
    /// <summary>
    /// Provides basic information of a ownCloud Share
    /// </summary>
    public class Share
    {
        /// <summary>
        /// The shares Id assigned by ownCloud
        /// </summary>
        public int ShareId { get; set; }
        /// <summary>
        /// The path to the target file/folder
        /// </summary>
        public string TargetPath { get; set; }
        /// <summary>
        /// The path to the actual path
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// The permissions granted on the share
        /// </summary>
        public int Perms { get; set; }
		/// <summary>
		/// Gets or sets the shares advanced properties.
		/// </summary>
		/// <value>The advanced properties.</value>
		public AdvancedShareProperties AdvancedProperties { get; set; }
    }

	/// <summary>
	/// Advanced share properties.
	/// </summary>
	public class AdvancedShareProperties {
		/// <summary>
		/// Gets or sets the type of the item.
		/// </summary>
		/// <value>The type of the item.</value>
		public string ItemType { get; set; }
		/// <summary>
		/// Gets or sets the item source.
		/// </summary>
		/// <value>The item source.</value>
		public string ItemSource { get; set; }
		/// <summary>
		/// Gets or sets the parent share.
		/// </summary>
		/// <value>The parent.</value>
		public string Parent { get; set; }
		/// <summary>
		/// Gets or sets the S time.
		/// </summary>
		/// <value>The S time.</value>
		public string STime { get; set; }
		/// <summary>
		/// Gets or sets the expiration date.
		/// </summary>
		/// <value>The expiration.</value>
		public string Expiration { get; set; }
		/// <summary>
		/// Gets or sets the storage location.
		/// </summary>
		/// <value>The storage.</value>
		public string Storage { get; set; }
		/// <summary>
		/// Gets or sets the mail send.
		/// </summary>
		/// <value>The mail send.</value>
		public string MailSend { get; set; }
		/// <summary>
		/// Gets or sets the owner.
		/// </summary>
		/// <value>The owner.</value>
		public string Owner { get; set; }
		/// <summary>
		/// Gets or sets the storage identifier.
		/// </summary>
		/// <value>The storage identifier.</value>
		public string StorageId { get; set; }
		/// <summary>
		/// Gets or sets the file source.
		/// </summary>
		/// <value>The file source.</value>
		public string FileSource { get; set; }
		/// <summary>
		/// Gets or sets the file parent.
		/// </summary>
		/// <value>The file parent.</value>
		public string FileParent { get; set; }
		/// <summary>
		/// Gets or sets the share with displayname.
		/// </summary>
		/// <value>The share with displayname.</value>
		public string ShareWithDisplayname { get; set; }
		/// <summary>
		/// Gets or sets the displayname owner.
		/// </summary>
		/// <value>The displayname owner.</value>
		public string DisplaynameOwner { get; set; }
	}
}
