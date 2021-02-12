namespace WebDAVClient.Types
{
    /// <summary>
    /// Provides information of a user ownCloud share.
    /// </summary>
	public class UserShare : Share
	{
        /// <summary>
        /// Name of the user the target is being shared with
        /// </summary>
        public string SharedWith { get; set; }
	}
}

