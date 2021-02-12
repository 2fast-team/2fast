using Prism.Commands;
using Prism.Mvvm;
using Project2FA.Repository.Models;
using Project2FA.UWP.Services;
using System.Windows.Input;
using System.Linq;

namespace Project2FA.UWP.ViewModels
{
    public class EditAccountContentDialogViewModel : BindableBase
    {
        private TwoFACodeModel _twoFACodeModel;
        private string _tempIssuer, _tempLabel;

        public ICommand CancelButtonCommand { get; }
        public ICommand PrimaryButtonCommand { get; }

        public EditAccountContentDialogViewModel()
        {
            PrimaryButtonCommand = new DelegateCommand(() =>
            {
                DataService.Instance.WriteLocalDatafile();
            });
            CancelButtonCommand = new DelegateCommand(() =>
            {
                TwoFACodeModel.Issuer = _tempIssuer;
                TwoFACodeModel.Label = _tempLabel;
            });
        }

        public string Issuer
        {
            get => TwoFACodeModel.Issuer;
            set
            {
                TwoFACodeModel.Issuer = value;
            }
        }
        public string Label
        {
            get => TwoFACodeModel.Label;
            set
            {
                TwoFACodeModel.Label = value;
            }
        }

        public TwoFACodeModel TwoFACodeModel
        {
            get => _twoFACodeModel;
            set
            {
                SetProperty(ref _twoFACodeModel, value);
                _tempIssuer = TwoFACodeModel.Issuer;
                _tempLabel = TwoFACodeModel.Label;
            }
        }
    }
}
