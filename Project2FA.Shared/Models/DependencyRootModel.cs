using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Project2FA.Repository.Models
{
    public partial class DependencyRootModel
    {
        public ObservableCollection<DependencyGroupModel> Groups { get; set; } = new ObservableCollection<DependencyGroupModel>();
    }
}
