using System;
using System.Security.Cryptography;
using System.Text;

namespace Project2FA.Core.Services.Gravatar
{
    public static class Gravatar
    {
        static MD5 md5 = MD5.Create();

        /// <summary>
        /// Default constructor
        /// </summary>
        static Gravatar() { }

        /// <summary>
        /// Generates a Gravatar URL from the mail address
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static string GetImageUrlAsString(string email)
        {
            String hashString = "";

            if (!string.IsNullOrEmpty(email))
            {
                // calculate hash
                byte[] hashByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes(email.ToLower().Trim()));

                // transform hash to string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashByteArray.Length; i++)
                {
                    sb.Append(hashByteArray[i].ToString("X2"));
                }
                hashString = sb.ToString().ToLower();
            }

            string imageUrl = "https://www.gravatar.com/avatar/" + hashString + "?d=mp";

            return imageUrl;
        }
    }
}
