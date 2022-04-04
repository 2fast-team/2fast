using Project2FA.Repository.Models;
using Project2FA.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Project2FA.UWP.Helpers
{
    public static class SVGColorHelper
    {


        public async static Task<string> ManipulateSVGColor(TwoFACodeModel model, string name, bool overwriteFavourite = false)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string path = string.Format("ms-appx:///Assets/AccountIcons/{0}.svg", name);
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    XDocument xmlDocument = XDocument.Load(stream);
                    XElement pathAttr = xmlDocument.Descendants().SingleOrDefault(p => p.Name.LocalName == "path");
                    if (model.IsFavourite && !overwriteFavourite)
                    {
                        pathAttr.SetAttributeValue("fill", "white");
                    }
                    else
                    {
                        switch ((Window.Current.Content as FrameworkElement).RequestedTheme)
                        {
                            case ElementTheme.Default:
                                if (SettingsService.Instance.OriginalAppTheme == ApplicationTheme.Dark)
                                {
                                    pathAttr.SetAttributeValue("fill", "black");
                                }
                                else
                                {
                                    pathAttr.SetAttributeValue("fill", "white");
                                }
                                break;
                            case ElementTheme.Dark:
                                pathAttr.SetAttributeValue("fill", "white");
                                break;
                            case ElementTheme.Light:
                                pathAttr.SetAttributeValue("fill", "black");
                                break;

                            default:
                                break;
                        }
                    }

                    return xmlDocument.ToString();
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
