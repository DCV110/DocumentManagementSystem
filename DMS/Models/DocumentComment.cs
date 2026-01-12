using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class DocumentComment
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
        [StringLength(2000)]
        public string Content { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
    }
}


