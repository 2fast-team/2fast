using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;

namespace Project2FA.UWP.ViewModels
{
    public class DeleteDatafileContentDialogViewModel : BindableBase
    {
        private string _password;
        private bool _showError;
        public ICommand ConfirmErrorCommand { get; }

        public bool ShowError { get => _showError; set => SetProperty(ref _showError, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        public DeleteDatafileContentDialogViewModel()
        {
            ConfirmErrorCommand = new DelegateCommand(() =>
            {
                ShowError = false;
            });
        }
    }
}
