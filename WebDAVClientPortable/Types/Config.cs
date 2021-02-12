namespace WebDAVClient.Types
{
	/// <summary>
	/// ownCloud configuration.
	/// </summary>
	public class Config
	{
		/// <summary>
		/// Gets or sets the version.
		/// </summary>
		/// <value>The version.</value>
		public string Version { get; set; }
		/// <summary>
		/// Gets or sets the website.
		/// </summary>
		/// <value>The website.</value>
		public string website { get; set; }
		/// <summary>
		/// Gets or sets the host.
		/// </summary>
		/// <value>The host.</value>
		public string Host { get; set; }
		/// <summary>
		/// Gets or sets the contact.
		/// </summary>
		/// <value>The contact.</value>
		public string Contact { get; set; }
		/// <summary>
		/// Gets or sets the ssl.
		/// </summary>
		/// <value>The ssl.</value>
		public string Ssl { get; set; }
	}
}

