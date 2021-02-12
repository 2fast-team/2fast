using System;
using System.Net;
using System.Text;

namespace WebDAVClient
{
    /// <summary>
    /// 
    /// </summary>
    public class UrlBuilder
    {
        #region Variables / Properties

        private readonly StringBuilder _builder;

        #endregion Variables / Properties

        #region Constructor

        /// <summary>
        /// </summary>
        /// <param name="urlBase"></param>
        public UrlBuilder(string urlBase)
        {
            _builder = new StringBuilder(urlBase);
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UrlBuilder AddQueryParameter(string paramName, string value)
        {
            _builder.Append(_builder.ToString().Contains("?") ? "&" : "?");
            _builder.Append(WebUtility.UrlEncode(paramName));
            _builder.Append("=");
            _builder.Append(WebUtility.UrlEncode(value));

            return this;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return _builder.ToString();
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public Uri ToUri()
        {
            return new Uri(_builder.ToString());
        }

        #endregion Methods
    }
}