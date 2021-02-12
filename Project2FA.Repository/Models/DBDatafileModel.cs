using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project2FA.Repository.Models
{
    public class DBDatafileModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public bool IsWebDAV { get; set; }

        public int DBPasswordHashModelId { get; set; }

        public DBPasswordHashModel DBPasswordHashModel { get; set; }
    }
}
