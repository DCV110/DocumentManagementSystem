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

        public int? CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        public string? CreatedBy { get; set; }
        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser? Creator { get; set; }

        public int? ParentFolderId { get; set; }
        [ForeignKey("ParentFolderId")]
        public virtual Folder? ParentFolder { get; set; }
        public virtual ICollection<Folder> SubFolders { get; set; } = new List<Folder>();

        public bool IsPublic { get; set; } = false;

        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

        // Soft Delete fields
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
        [ForeignKey("DeletedBy")]
        public virtual ApplicationUser? Deleter { get; set; }
    }
}
