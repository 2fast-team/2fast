using System.Collections.ObjectModel;

namespace Project2FA.Repository.Models
{
    public class DatafileModel
    {
        public ObservableCollection<TwoFACodeModel> Collection { get; set; }

        public ObservableCollection<CategoryModel> GlobalCategories { get; set; }
        public byte[] IV { get; set; }

        public int Version { get; set; }
    }
}
