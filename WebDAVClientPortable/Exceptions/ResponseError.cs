using System;
using System.Diagnostics;

namespace WebDAVClient.Exceptions
{
	/// <summary>
	/// Response error.
	/// </summary>
	public class ResponseError : Exception
	{
		/// <summary>
		/// Status code associated to the error.
		/// </summary>
		private string statusCode;

		/// <summary>
		/// Gets the status code associated with the error.
		/// </summary>
		/// <value>The status code.</value>
		public string StatusCode {
			get {
				return statusCode;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResponseError"/> class.
		/// </summary>
		public ResponseError () : base() {	}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResponseError"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ResponseError (string message) : base(message) {	}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResponseError"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="statusCode">Status code associated to the error.</param>
		public ResponseError (string message, string statusCode) : base(message) {
			this.statusCode = statusCode;
			Debug.WriteLine ("ERROR - Code: " + this.statusCode + " - Message: " + this.Message);
		}
	}
}

