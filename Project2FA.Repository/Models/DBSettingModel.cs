using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project2FA.Repository.Models
{
    public class DBSettingModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Language { get; set; }

        /// <summary>
        /// Get/Set the size of items for grouping
        /// </summary>
        public int GroupingSize { get; set; }

        /// <summary>
        /// Get/Set if grouping is enabled
        /// </summary>
        public bool GroupingEnabled { get; set; }

        public bool AppRated { get; set; }
    }
}
