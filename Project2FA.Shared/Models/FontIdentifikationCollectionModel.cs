using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Project2FA.Repository.Models
{
    public class FontIdentifikationCollectionModel
    {
        public string Name { get; set; }
        private string Version { get; set; }
        public ObservableCollection<FontIdentifikationModel> Collection { get; set; }
    }
}
