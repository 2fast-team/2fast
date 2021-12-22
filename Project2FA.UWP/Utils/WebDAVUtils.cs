using System;
using System.Threading.Tasks;
using Template10.Services.Network;
using Prism.Ioc;
using WebDAVClient.Exceptions;
using Template10.Services.Resources;
using Prism.Services.Dialogs;

namespace Project2FA.UWP.Utils
{
    public class WebDAVUtils
    {
        private async Task<bool> CheckServerStatus(string serverAddress)
        {
            INetworkService networkService = App.Current.Container.Resolve<INetworkService>();
            IResourceService resourceService = App.Current.Container.Resolve<IResourceService>();
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            try
            {
                if (await networkService.GetIsInternetAvailableAsync())
                {
                    var status = await WebDAVClient.Client.GetServerStatus(serverAddress);
                    if (status == null)
                    {
                        await ShowServerAddressNotFoundMessage();
                        return false;
                    }
                    if (!status.Installed)
                    {
                        //TODO error info
                        //var dialog = new ContentDialog
                        //{
                        //    Title = resourceService.GetLocalizedString("AnErrorHasOccurred"),
                        //    Content = new TextBlock
                        //    {
                        //        Text = resourceService.GetLocalizedString("Auth_Unauthorized"),
                        //        TextWrapping = TextWrapping.WrapWholeWords,
                        //        Margin = new Thickness(0, 20, 0, 0)
                        //    },
                        //    PrimaryButtonText = resourceService.GetLocalizedString("OK")
                        //};
                        //await dialogService.ShowAsync(dialog);
                        return false;
                    }
                    if (status.Maintenance)
                    {
                        //TODO error info
                        //var dialog = new ContentDialog
                        //{
                        //    Title = _resourceService.GetLocalizedString("AnErrorHasOccurred"),
                        //    Content = new TextBlock
                        //    {
                        //        Text = _resourceService.GetLocalizedString("Auth_MaintenanceEnabled"),
                        //        TextWrapping = TextWrapping.WrapWholeWords,
                        //        Margin = new Thickness(0, 20, 0, 0)
                        //    },
                        //    PrimaryButtonText = _resourceService.GetLocalizedString("OK")
                        //};
                        //await _dialogService.ShowAsync(dialog);
                        return false;
                    }
                }
                else
                {

                }

            }
            catch (ResponseError e)
            {
                //ResponseErrorHandlerService.HandleException(e);
                return false;
            }
            return true;
        }

        private Task ShowServerAddressNotFoundMessage()
        {
            throw new NotImplementedException();
        }

        private async Task<bool> CheckUserLogin(string serverAddress, string username, string password)
        {
            try
            {
                return await WebDAVClient.Client.CheckUserLogin(serverAddress, username, password);
            }
            catch (ResponseError)
            {
                return false;
            }
        }
    }
}
