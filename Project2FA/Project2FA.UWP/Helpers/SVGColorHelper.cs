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
        public async static Task<(bool success, string icon)> GetSVGIconWithThemeColor(bool isFavourite, string name, bool overwriteFavourite = false)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string path = string.Format("ms-appx:///Assets/AccountIcons/{0}.svg", name);
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    XDocument xmlDocument = XDocument.Load(stream);
                    XElement pathAttr = xmlDocument.Descendants().SingleOrDefault(p => p.Name.LocalName == "path");
                    if (isFavourite && !overwriteFavourite)
                    {
                        pathAttr.SetAttributeValue("fill", "white");
                    }
                    else
                    {
                        switch (SettingsService.Instance.AppTheme)
                        {
                            case Services.Enums.Theme.System:
                                if (SettingsService.Instance.OriginalAppTheme == ApplicationTheme.Dark)
                                {
                                    pathAttr.SetAttributeValue("fill", "white");
                                }
                                else
                                {
                                    pathAttr.SetAttributeValue("fill", "black");
                                }
                                break;
                            case Services.Enums.Theme.Dark:
                                pathAttr.SetAttributeValue("fill", "white");
                                break;
                            case Services.Enums.Theme.Light:
                                pathAttr.SetAttributeValue("fill", "black");
                                break;
                            default:
                                break;
                        }
                    }
                    return (true, xmlDocument.ToString());
                }
            }
            else
            {
                return (false, string.Empty);
            }
        }
        public async static Task<bool> GetSVGIconWithThemeColor(TwoFACodeModel model, string name, bool overwriteFavourite = false)
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
                        switch (SettingsService.Instance.AppTheme)
                        {
                            case Services.Enums.Theme.System:
                                if (SettingsService.Instance.OriginalAppTheme == ApplicationTheme.Dark)
                                {
                                    pathAttr.SetAttributeValue("fill", "white");
                                }
                                else
                                {
                                    pathAttr.SetAttributeValue("fill", "black");
                                }
                                break;
                            case Services.Enums.Theme.Dark:
                                pathAttr.SetAttributeValue("fill", "white");
                                break;
                            case Services.Enums.Theme.Light:
                                pathAttr.SetAttributeValue("fill", "black");
                                break;
                            default:
                                break;
                        }
                    }
                    model.AccountSVGIcon = xmlDocument.ToString();
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
