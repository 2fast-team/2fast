using System.Collections.ObjectModel;

namespace Project2FA.Repository.Models
{
    public class IconNameCollectionModel
    {
        public string AppVersion { get; set; }
        public ObservableCollection<IconNameModel> Collection { get; set; }
    }
}
