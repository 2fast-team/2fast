using System.Collections.Generic;

namespace WebDAVClient.Types
{
	/// <summary>
	/// ownCloud App information.
	/// </summary>
	public class AppInfo
	{
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string Id { get; set; }
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }
		/// <summary>
		/// Gets or sets the licence.
		/// </summary>
		/// <value>The licence.</value>
		public string Licence { get; set; }
		/// <summary>
		/// Gets or sets the author.
		/// </summary>
		/// <value>The author.</value>
		public string Author { get; set; }
		/// <summary>
		/// Gets or sets the require minimum.
		/// </summary>
		/// <value>The require minimum.</value>
		public string RequireMin { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="AppInfo"/> is shipped.
		/// </summary>
		/// <value><c>true</c> if shipped; otherwise, <c>false</c>.</value>
		public bool Shipped { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="AppInfo"/> is standalone.
		/// </summary>
		/// <value><c>true</c> if standalone; otherwise, <c>false</c>.</value>
		public bool Standalone { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="AppInfo"/> default enable.
		/// </summary>
		/// <value><c>true</c> if default enable; otherwise, <c>false</c>.</value>
		public bool DefaultEnable { get; set; }
		/// <summary>
		/// Gets or sets the types.
		/// </summary>
		/// <value>The types.</value>
		public List<string> Types { get; set; }
		/// <summary>
		/// Gets or sets the remote.
		/// </summary>
		/// <value>The remote.</value>
		public Dictionary<string, string> Remote { get; set; }
		/// <summary>
		/// Gets or sets the documentation.
		/// </summary>
		/// <value>The documentation.</value>
		public Dictionary<string, string> Documentation { get; set; }
		/// <summary>
		/// Gets or sets the info.
		/// </summary>
		/// <value>The info.</value>
		public Dictionary<string, string> Info { get; set; }
		/// <summary>
		/// Gets or sets the public.
		/// </summary>
		/// <value>The public.</value>
		public Dictionary<string, string> Public { get; set; }
	}
}

