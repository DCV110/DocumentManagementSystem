using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Models
{
    public class Folder
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string FolderName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
