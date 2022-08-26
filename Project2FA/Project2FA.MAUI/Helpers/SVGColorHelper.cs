using Project2FA.MAUI.Services;
using Project2FA.MAUI.Services.Settings.Enums;
using Project2FA.Repository.Models;
using System.Xml.Linq;

namespace Project2FA.MAUI.Helpers
{
    public static class SVGColorHelper
    {
        public async static Task<(bool success, string icon)> GetSVGIconWithThemeColor(bool isFavourite, string name, bool overwriteFavourite = false)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string path = string.Format(FileSystem.Current.AppDataDirectory + "/Assets/AccountIcons/{0}.svg", name);
                if (await FileSystem.Current.AppPackageFileExistsAsync(path))
                {
                    using (Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(path))
                    {
                        XDocument xmlDocument = XDocument.Load(fileStream);
                        XElement pathAttr = xmlDocument.Descendants().SingleOrDefault(p => p.Name.LocalName == "path");
                        if (isFavourite && !overwriteFavourite)
                        {
                            pathAttr.SetAttributeValue("fill", "white");
                        }
                        else
                        {
                            switch (SettingsService.Instance.ApplyTheme)
                            {
                                case Theme.System:
                                    if (App.Current.RequestedTheme == AppTheme.Dark)
                                    {
                                        pathAttr.SetAttributeValue("fill", "white");
                                    }
                                    else
                                    {
                                        pathAttr.SetAttributeValue("fill", "black");
                                    }
                                    break;
                                case Theme.Dark:
                                    pathAttr.SetAttributeValue("fill", "white");
                                    break;
                                case Theme.Light:
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
            else
            {
                return (false, string.Empty);
            }
        }


        public async static Task<bool> GetSVGIconWithThemeColor(TwoFACodeModel model, string name, bool overwriteFavourite = false)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string path = string.Format(FileSystem.Current.AppDataDirectory + "/Assets/AccountIcons/{0}.svg", name);
                if (await FileSystem.Current.AppPackageFileExistsAsync(path))
                {
                    using (Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(path))
                    {
                        XDocument xmlDocument = XDocument.Load(fileStream);
                        XElement pathAttr = xmlDocument.Descendants().SingleOrDefault(p => p.Name.LocalName == "path");
                        if (model.IsFavourite && !overwriteFavourite)
                        {
                            pathAttr.SetAttributeValue("fill", "white");
                        }
                        else
                        {
                            switch (SettingsService.Instance.ApplyTheme)
                            {
                                case Theme.System:
                                    if (App.Current.RequestedTheme == AppTheme.Dark)
                                    {
                                        pathAttr.SetAttributeValue("fill", "white");
                                    }
                                    else
                                    {
                                        pathAttr.SetAttributeValue("fill", "black");
                                    }
                                    break;
                                case Theme.Dark:
                                    pathAttr.SetAttributeValue("fill", "white");
                                    break;
                                case Theme.Light:
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
            else
            {
                return false;
            }
        }
    }
}
