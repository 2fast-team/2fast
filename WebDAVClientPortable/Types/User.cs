namespace WebDAVClient.Types
{
	/// <summary>
	/// ownCloud user information.
	/// </summary>
	public class User
	{
		/// <summary>
		/// Gets or sets the users E-mail.
		/// </summary>
		/// <value>The E mail.</value>
		public string EMail { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="User"/> is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool Enabled { get; set; }
		/// <summary>
		/// Gets or sets the users display name.
		/// </summary>
		/// <value>The display name.</value>
		public string DisplayName { get; set; }
		/// <summary>
		/// Gets or sets the users storage quota.
		/// </summary>
		/// <value>The quota.</value>
		public Quota Quota { get; set; }
	}

	/// <summary>
	/// User specific storage quota.
	/// </summary>
	public class Quota 
	{
		/// <summary>
		/// Gets or sets the free storage space in bytes.
		/// </summary>
		/// <value>The free.</value>
		public long Free { get; set; }
		/// <summary>
		/// Gets or sets the used storage space in bytes.
		/// </summary>
		/// <value>The used.</value>
		public long Used { get; set; }
		/// <summary>
		/// Gets or sets the total storage space in bytes.
		/// </summary>
		/// <value>The total.</value>
		public long Total { get; set; }
		/// <summary>
		/// Gets or sets the relative storage space in bytes.
		/// </summary>
		/// <value>The relative.</value>
		public double Relative { get; set; }

        /// <summary>
        /// Gets or sets the quota value.
        /// </summary>
        /// <value>
        /// The quota value.
        /// </value>
        public long QuotaValue { get; set; }
	}
}

