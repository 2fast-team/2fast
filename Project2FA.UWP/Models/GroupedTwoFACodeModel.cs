using Prism.Mvvm;
using Project2FA.Repository.Models;
using System.Collections.ObjectModel;

namespace Project2FA.UWP.Models
{
    public class GroupedTwoFACodeModel : BindableBase
    {
        private char _groupKey;
        public char GroupKey { get => _groupKey; set { SetProperty(ref _groupKey, value); } }

        private ObservableCollection<TwoFACodeModel> _items;
        public ObservableCollection<TwoFACodeModel> Items { get => _items; set { SetProperty(ref _items, value); } }
    }
}
