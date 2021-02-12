namespace WebDAVClient.Exceptions
{
	/// <summary>
	/// OCS API response error.
	/// </summary>
	public class OcsResponseError : ResponseError
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="OcsResponseError" /> class.
        /// </summary>
        public OcsResponseError () : base()	{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="OcsResponseError"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OcsResponseError (string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OcsResponseError"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="statusCode">Status code associated to the error.</param>
        public OcsResponseError (string message, string statusCode) : base(message, statusCode) { }
	}
}

