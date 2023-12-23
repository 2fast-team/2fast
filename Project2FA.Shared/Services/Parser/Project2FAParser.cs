using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
#if WINDOWS_UWP
using Project2FA.UWP;
#endif

namespace Project2FA.Services.Parser
{
    public class Project2FAParser : IProject2FAParser
    {
        /// <summary>
        /// Parse the QR-Code string into a list of KeyValuePair
        /// </summary>
        /// <param name="qrCodeStr"></param>
        /// <returns>list of keyvaluepair strings</returns>
        public List<KeyValuePair<string, string>> ParseQRCodeStr(string qrCodeStr)
        {
            List<KeyValuePair<string, string>> qrParams = new List<KeyValuePair<string, string>>();

            try
            {
                //otpauth://TYPE/LABEL?PARAMETERS
                Match match = Regex.Match(qrCodeStr, @"otpauth://([^/]+)/([^?]+)", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    //check if the QR-Code contains a totp auth request
                    if (match.Groups[1].Value == "totp")
                    {
                        qrParams.Add(new KeyValuePair<string, string>("auth", match.Groups[1].Value));
                        string label = string.Empty;
                        string issuer = match.Groups[2].Value;
                        string collectionVar;
                        //check if the first parameter contains the title of the service
                        if (issuer.Contains(":"))
                        {
                            string[] issuerArray = issuer.Split(':');
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
                        System.Collections.Specialized.NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(qrCodeStr);
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
                        else if (string.IsNullOrEmpty(label))
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
            }
            catch (System.Exception exc)
            {
                qrParams.Clear();
#if WINDOWS_UWP
                TrackingManager.TrackExceptionCatched(nameof(ParseQRCodeStr), exc);
#endif
            }
            
            //else return empty parameter list => no otpauth

            return qrParams;
        }

        public List<KeyValuePair<string, string>> ParseCmdStr(string cmdStr)
        {
            List<KeyValuePair<string, string>> cmdParams = new List<KeyValuePair<string, string>>();

            try
            {
                //twofastauth://CATEGORY/set?PARAMETERS
                Match match = Regex.Match(cmdStr, @"twofastauth://([^/]+)/([^?]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    //check if the command line request is for config
                    if (match.Groups[1].Value == "config")
                    {
                        //get only the parameter string
                        cmdStr = cmdStr.Remove(0, match.Groups[0].Length + 1); //remove //twofastauth://CATEGORY/set?
                        string isScreenCaptureEnabled;
                        string collectionVar;

                        if (!string.IsNullOrEmpty(cmdStr))
                        {
                            //create collection of the parameter values
                            System.Collections.Specialized.NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(cmdStr);

                            collectionVar = nameValueCollection["IsScreenCaptureEnabled"];
                            if (!string.IsNullOrEmpty(collectionVar))
                            {
                                isScreenCaptureEnabled = collectionVar;
                                cmdParams.Add(new KeyValuePair<string, string>(nameof(isScreenCaptureEnabled), isScreenCaptureEnabled));
                            }
                        }
                    }
                    //else not definied
                }
            }
            catch (System.Exception exc)
            {
                cmdParams.Clear();
#if WINDOWS_UWP
                TrackingManager.TrackExceptionCatched(nameof(ParseCmdStr), exc);
#endif
            }

            return cmdParams;
        }
    }
}
