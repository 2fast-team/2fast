using System;
using System.Threading;
using System.Threading.Tasks;
using Template10.Services.Dialog;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Services.DialogService
{
    public class PrismDialogService 
        //:IDialogService
    {
        private static CancellationTokenSource _tokenSource;
        public Task<MessageBoxResult> AlertAsync(string content, IDialogResourceResolver resolver = null)
        {
            return AlertAsync(string.Empty, content, resolver);
        }

        public Task<MessageBoxResult> AlertAsync(string title, string content, IDialogResourceResolver resolver = null)
        {
            return new MessageBoxEx(title, content, MessageBoxType.Ok, resolver).ShowAsync();
        }

        /// <summary>
        /// closes all dialogs where no user defined token was passed
        /// </summary>
        public async Task CancelDialogs()
        {
            if (await IsDialogRunning())
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel();
                    _tokenSource = new CancellationTokenSource();
                }
            }
        }

        /// <summary>
        /// calls up whether dialogs are currently active through the service
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsDialogRunning()
        {
            return PrismDialogManager.IsDialogRunning();
        }

        public Task<MessageBoxResult> PromptAsync(string content, MessageBoxType type = MessageBoxType.YesNo, IDialogResourceResolver resolver = null)
        {
            return PromptAsync(string.Empty, content, type, resolver);
        }

        public Task<MessageBoxResult> PromptAsync(string title, string content, MessageBoxType type = MessageBoxType.YesNo, IDialogResourceResolver resolver = null)
        {
            return new MessageBoxEx(title, content, type, resolver).ShowAsync();
        }

        public Task<bool> PromptAsync(string content, MessageBoxType type, MessageBoxResult expected, IDialogResourceResolver resolver = null)
        {
            return PromptAsync(string.Empty, content, type, expected, resolver);
        }

        public async Task<bool> PromptAsync(string title, string content, MessageBoxType type, MessageBoxResult expected, IDialogResourceResolver resolver = null)
        {
            return (await PromptAsync(title, content, type, resolver)).Equals(expected);
        }

        //void ConfigureDialogWindowContent(string dialogName, IDialogWindow window, IDialogParameters parameters)
        //{
        //    var content = _containerProvider.Resolve<object>(dialogName);
        //    if (!(content is FrameworkElement dialogContent))
        //    {
        //        throw new NullReferenceException("A dialog's content must be a FrameworkElement");
        //    }

        //    MvvmHelpers.AutowireViewModel(content);

        //    if (!(dialogContent.DataContext is IDialogAware viewModel))
        //    {
        //        throw new NullReferenceException($"A dialog's ViewModel must implement the IDialogAware interface ({dialogContent.DataContext})");
        //    }

        //    ConfigureDialogWindowProperties(window, dialogContent, viewModel);

        //    MvvmHelpers.ViewAndViewModelAction<IDialogAware>(viewModel, d => d.OnDialogOpened(parameters));
        //}

        public Task<ContentDialogResult> ShowAsync(ContentDialog dialog, TimeSpan? timeout = null, CancellationToken? token = null)
        {
            if (_tokenSource is null)
            {
                _tokenSource = new CancellationTokenSource();
            }
            var tk = token ?? _tokenSource.Token;
            return PrismDialogManager.OneAtATimeAsync(async () => await dialog.ShowAsync(tk), timeout, tk);
        }

        public Task<IUICommand> ShowAsync(MessageDialog dialog, TimeSpan? timeout = null, CancellationToken? token = null)
        {
            if (_tokenSource is null)
            {
                _tokenSource = new CancellationTokenSource();
            }
            CancellationToken tk = token ?? _tokenSource.Token;
            return PrismDialogManager.OneAtATimeAsync(async () => await dialog.ShowAsync().AsTask(tk), timeout, tk);
        }
    }
}
