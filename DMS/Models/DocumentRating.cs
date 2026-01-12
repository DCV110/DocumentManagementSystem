using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class DocumentRating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        public virtual Document Document { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // 1-5 sao

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}


