using System.Threading.Tasks;

namespace Project2FA.Uno.Core.Dialogs
{
    public interface IDialogInitializeAsync
    {
        Task InitializeAsync(IDialogParameters parameters);
    }
}
