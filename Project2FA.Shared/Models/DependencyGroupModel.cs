using System.Collections.ObjectModel;

namespace Project2FA.Repository.Models
{
#if WINDOWS_UWP && NET10_0_OR_GREATER
    [WinRT.GeneratedBindableCustomPropertyAttribute]
#endif
    public partial class DependencyGroupModel
    {
        public string GroupName { get; set; }
        public ObservableCollection<DependencyModel> Items { get; set; } = new ObservableCollection<DependencyModel>();
        public override string ToString()
        {
            return this.GroupName;
        }
    }
}
