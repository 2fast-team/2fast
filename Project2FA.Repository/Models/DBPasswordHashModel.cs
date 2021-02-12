using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project2FA.Repository.Models
{
    public class DBPasswordHashModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Get/Set the hash from the password, which is stored in system vault
        /// </summary>
        public string Hash { get; set; }
    }
}
