using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project2FA.Uno.Core.Dialogs
{
    /// <summary>
    /// Interface to show modal and non-modal dialogs.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="name">The dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog, IDialogParameters parameters, TimeSpan? timeout = null, CancellationToken? token = null);

        /// <summary>
        /// closes all dialogs where no user defined token was passed
        /// </summary>
        void CloseDialogs();

        /// <summary>
        /// Checks if a Dialog is running
        /// </summary>
        /// <returns>true if a dialog was started by this service</returns>
        Task<bool> IsDialogRunning();
    }
}
