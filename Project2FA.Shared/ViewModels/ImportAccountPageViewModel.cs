using Project2FA.Core.Utils;
using Project2FA.Repository.Models;
using Project2FA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project2FA.Core.ProtoModels;
using OtpNet;

#if WINDOWS_UWP
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Project2FA.UnoApp;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
    public class ImportAccountPageViewModel : AddAccountViewModelBase
    {
        public ObservableCollection<TwoFACodeModel> OTPList { get; internal set; } = new ObservableCollection<TwoFACodeModel>();

        private async Task PrimaryButtonCommandTask()
        {
#if WINDOWS_UWP
            await CleanUpCamera();
#endif

            if (OTPList.Count > 0)
            {
                for (int i = 0; i < OTPList.Count; i++)
                {
                    if (OTPList[i].IsChecked)
                    {
                        DataService.Instance.Collection.Add(OTPList[i]);
                    }
                }
            }
            else
            {
                Model.SelectedCategories ??= new ObservableCollection<CategoryModel>();
                Model.SelectedCategories.AddRange(GlobalTempCategories.Where(x => x.IsSelected == true), true);
                DataService.Instance.Collection.Add(Model);
            }
#if __ANDROID__ || _IOS__
            await App.ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
#endif
        }

        /// <summary>
        /// Parse the protobuf data to TwoFACodeModel list
        /// </summary>
        /// <returns></returns>
        private Task<bool> ParseMigrationQRCode(string qrCodeStr)
        {
            try
            {
                var otpmm = new OTPMigrationModel();
                var query = new Uri(qrCodeStr).Query.Replace("?data=", string.Empty);
                var dataByteArray = Convert.FromBase64String(query);
                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(dataByteArray, 0, dataByteArray.Length);
                    memoryStream.Position = 0;
                    otpmm = ProtoBuf.Serializer.Deserialize<OTPMigrationModel>(memoryStream);
                    for (int i = 0; i < otpmm.otp_parameters.Count; i++)
                    {
                        if (otpmm.otp_parameters[i].Type == OTPMigrationModel.OtpType.OtpTypeTotp)
                        {
                            string label = string.Empty, issuer = string.Empty;
                            if (otpmm.otp_parameters[i].Name.Contains(":"))
                            {
                                string[] issuerArray = otpmm.otp_parameters[i].Name.Split(':');
                                label = issuerArray[0];
                                issuer = issuerArray[1];
                            }
                            else
                            {
                                label = otpmm.otp_parameters[i].Name;
                                issuer = otpmm.otp_parameters[i].Issuer;
                            }
                            int hashMode = 0;
                            switch (otpmm.otp_parameters[i].Algorithm)
                            {
                                case OTPMigrationModel.Algorithm.AlgorithmSha1:
                                    hashMode = 0;
                                    break;
                                case OTPMigrationModel.Algorithm.AlgorithmSha256:
                                    hashMode = 1;
                                    break;
                                case OTPMigrationModel.Algorithm.AlgorithmSha512:
                                    hashMode = 2;
                                    break;
                            }
                            OTPList.Add(new TwoFACodeModel
                            {
                                Label = label,
                                Issuer = issuer,
                                SecretByteArray = otpmm.otp_parameters[i].Secret,
                                HashMode = (OtpHashMode)hashMode
                            });
                        }
                        else
                        {
                            // no TOTP, not supported
                            string label = string.Empty, issuer = string.Empty;
                            if (otpmm.otp_parameters[i].Name.Contains(":"))
                            {
                                string[] issuerArray = otpmm.otp_parameters[i].Name.Split(':');
                                label = issuerArray[0];
                                issuer = issuerArray[1];
                            }
                            else
                            {
                                label = otpmm.otp_parameters[i].Name;
                                issuer = otpmm.otp_parameters[i].Issuer;
                            }
                            OTPList.Add(new TwoFACodeModel
                            {
                                Label = label,
                                Issuer = issuer,
                                IsChecked = false,
                                IsEnabled = false
                            });
                        }
                    }
                }
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}
