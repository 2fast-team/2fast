using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace Project2FA.Core.Services.Parser
{
    public class Project2FAParser : IProject2FAParser
    {
        /// <summary>
        /// Parse the QR-Code string into a list of KeyValuePair
        /// </summary>
        /// <param name="qrCodeStr"></param>
        /// <returns>List<KeyValuePair<string,string></returns>
        public List<KeyValuePair<string,string>> ParseQRCodeStr(string qrCodeStr)
        {
            List<KeyValuePair<string, string>> qrParams = new List<KeyValuePair<string, string>>();

            //otpauth://TYPE/LABEL?PARAMETERS
            var match = Regex.Match(qrCodeStr, @"otpauth://([^/]+)/([^?]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                //check if the QR-Code contains a totp auth request
                if (match.Groups[1].Value == "totp")
                {
                    string label = string.Empty;
                    string issuer = match.Groups[2].Value;
                    string collectionVar;
                    //check if the first parameter contains the title of the service
                    if (issuer.Contains(":"))
                    {
                        var issuerArray = issuer.Split(':');
                        label = issuerArray[0];
                        issuer = issuerArray[1];
                        qrParams.Add(new KeyValuePair<string, string>(nameof(label), label));
                        qrParams.Add(new KeyValuePair<string, string>(nameof(issuer), issuer));
                    }
                    else
                    {
                        //set the issuer
                        if (!string.IsNullOrEmpty(issuer))
                        {
                            qrParams.Add(new KeyValuePair<string, string>(nameof(issuer), issuer));
                        }

                    }
                    //get only the parameter string
                    qrCodeStr = qrCodeStr.Remove(0, match.Groups[0].Length + 1); //remove otpauth://totp/TitleIssuerName?

                    //create collection of the parameter values
                    var nameValueCollection = HttpUtility.ParseQueryString(qrCodeStr);
                    if (string.IsNullOrEmpty(issuer))
                    {
                        collectionVar = nameValueCollection["issuer"];
                        if (!string.IsNullOrEmpty(collectionVar))
                        {
                            issuer = collectionVar;
                            qrParams.Add(new KeyValuePair<string, string>(nameof(issuer), issuer));
                        }
                    }
                    //fallback to get the label from second issuer, if issuer already set
                    else if(string.IsNullOrEmpty(label))
                    {
                        collectionVar = nameValueCollection["issuer"];
                        if (!string.IsNullOrEmpty(collectionVar))
                        {
                            label = collectionVar;
                            qrParams.Add(new KeyValuePair<string, string>(nameof(label), label));
                        }
                    }

                    collectionVar = nameValueCollection["secret"];
                    if (!string.IsNullOrEmpty(collectionVar))
                    {
                        string secret = collectionVar;
                        qrParams.Add(new KeyValuePair<string, string>(nameof(secret), secret));
                    }

                    collectionVar = nameValueCollection["algorithm"];
                    if (!string.IsNullOrEmpty(collectionVar))
                    {
                        string algorithm = collectionVar;
                        qrParams.Add(new KeyValuePair<string, string>(nameof(algorithm), algorithm));
                    }

                    collectionVar = nameValueCollection["period"];
                    if (!string.IsNullOrEmpty(collectionVar))
                    {
                        string period = collectionVar;
                        qrParams.Add(new KeyValuePair<string, string>(nameof(period), period));
                    }

                    collectionVar = nameValueCollection["digits"];
                    if (!string.IsNullOrEmpty(collectionVar))
                    {
                        string digits = collectionVar;
                        qrParams.Add(new KeyValuePair<string, string>(nameof(digits), digits));
                    }
                }
                else
                {
                    //write the other authentification method in the parameter list 
                    //to give the user feedback about the unsupported auth method
                    qrParams.Add(new KeyValuePair<string, string>("auth", match.Groups[1].Value));
                }
            }
            //else return empty parameter list => no otpauth

            return qrParams;
        }
    }
}
