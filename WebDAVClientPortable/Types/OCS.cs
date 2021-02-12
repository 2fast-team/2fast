namespace WebDAVClient.Types
{
	/// <summary>
	/// OCS API Response.
	/// </summary>
	public class OCS
	{
		/// <summary>
		/// Gets or sets the meta information.
		/// </summary>
		/// <value>The meta.</value>
		public Meta Meta { get; set; }
		/// <summary>
		/// Gets or sets the data payload.
		/// </summary>
		/// <value>The data.</value>
		public object Data { get; set; }
	}

	/// <summary>
	/// OCS API Meta information.
	/// </summary>
	public class Meta {
		/// <summary>
		/// Gets or sets the response status.
		/// </summary>
		/// <value>The status.</value>
		public string Status { get; set; }
		/// <summary>
		/// Gets or sets the response status code.
		/// </summary>
		/// <value>The status code.</value>
		public int StatusCode { get; set; }
		/// <summary>
		/// Gets or sets the response status message.
		/// </summary>
		/// <value>The message.</value>
		public string Message { get; set; }
	}
}

