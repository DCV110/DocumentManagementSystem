using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class FavoriteDocument
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [Required]
        public int DocumentId { get; set; }
        
        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; } = null!;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Unique constraint: one user can only favorite a document once
    }
}

