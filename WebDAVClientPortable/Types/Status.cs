namespace WebDAVClient.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class Status
    {
        /// <summary>
        /// Gets or sets the product name of the ownCloud/nextCloud instance
        /// </summary>
        public string Productname { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the ownCloud/nextCloud instance was installed successfully; 
        /// true indicates a successful installation, and false indicates an unsuccessful installation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if installed; otherwise, <c>false</c>.
        /// </value>
        public bool Installed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the db needs a upgrade.
        /// </summary>
        /// <value>
        ///   <c>true</c> if db needs a upgrade; otherwise, <c>false</c>.
        /// </value>
        public bool NeedsDbUpgrade { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether maintenance mode to disable ownCloud/nextCloud is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if maintenance; otherwise, <c>false</c>.
        /// </value>
        public bool Maintenance { get; set; }

        /// <summary>
        /// Gets or sets the current version number of your ownCloud/nextCloud installation.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the current version string of your ownCloud/nextCloud installation.
        /// </summary>
        /// <value>
        /// The version string.
        /// </value>
        public string VersionString { get; set; }

        /// <summary>
        /// Gets or sets the edition.
        /// </summary>
        /// <value>
        /// The edition.
        /// </value>
        public string Edition { get; set; }
    }
}
