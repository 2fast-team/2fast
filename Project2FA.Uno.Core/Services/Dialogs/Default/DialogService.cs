using System;
using System.Threading;
using System.Threading.Tasks;
using Prism.Common;
using Prism.Ioc;
#if HAS_WINUI
using Microsoft.UI.Xaml.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace Project2FA.Uno.Core.Dialogs
{
    public class DialogService : IDialogService
    {
        private readonly bool _logStartingEvents = true;
        private readonly IContainerProvider _containerProvider;
        private static CancellationTokenSource _tokenSource;

        public DialogService(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            if (_tokenSource is null)
            {
                _tokenSource = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// closes all dialogs where no user defined token was passed
        /// </summary>
        public async void CloseDialogs()
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
        public async Task<bool> IsDialogRunning()
        {
            return await DialogManager.IsDialogRunning();
        }

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="dialog">The dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        public Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog, IDialogParameters parameters, TimeSpan? timeout = null, CancellationToken? token = null)
        {
            return ShowDialogInternalAsync(dialog, parameters, timeout, token);
        }

        private async Task<ContentDialogResult> ShowDialogInternalAsync(ContentDialog dialog, IDialogParameters parameters, TimeSpan? timeout = null, CancellationToken? token = null)
        {
            await ConfigureContentDialogContent(dialog, parameters);

            var tk = token ?? _tokenSource.Token;
            return await DialogManager.OneAtATimeAsync(async () => await dialog.ShowAsync(tk), timeout, tk);

        }

        private async Task ConfigureContentDialogContent(ContentDialog dialog, IDialogParameters parameters)
        {
            MvvmHelpers.AutowireViewModel(dialog);

            object viewModel = dialog.DataContext;
            Initialize(parameters, viewModel);
            await InitializeAsync(parameters, viewModel);
        }

        private void Initialize(IDialogParameters parameters, object vm)
        {
            if (vm is IDialogInitialize new_vm_ing)
            {
                new_vm_ing.Initialize(parameters);
            }
        }

        private async Task InitializeAsync(IDialogParameters parameters, object vm)
        {
            if (vm is IDialogInitializeAsync new_vm_ing)
            {
                await new_vm_ing.InitializeAsync(parameters);
            }
        }
    }
}
