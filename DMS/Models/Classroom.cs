using System.ComponentModel.DataAnnotations;

namespace DMS.Models
{
    public class Classroom
    {
        [Key]
        public int Id { get; set; }
        public string ClassName { get; set; }    // Tên lớp (ví dụ: CNTT K15)
        public int AcademicYear { get; set; }    // Niên khóa

        // Quan hệ nhiều-nhiều với môn học (nếu cần)
        public virtual ICollection<Course> Courses { get; set; }
    }
}
