namespace WebDAVClient.Types
{
    /// <summary>
    /// Provides information of a public ownCloud share.
    /// </summary>
	public class PublicShare : Share
	{
        /// <summary>
        /// Remote access URL
        /// </summary>
		public string Url { get; set; }
        /// <summary>
        /// The shares token
        /// </summary>
		public string Token { get; set; }
	}
}

