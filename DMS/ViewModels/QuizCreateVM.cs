using System.ComponentModel.DataAnnotations;

namespace DMS.ViewModels
{
    public class QuizCreateVM
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        public int? CourseId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(0, 1000, ErrorMessage = "Thời gian làm bài phải từ 0 đến 1000 phút")]
        public int TimeLimitMinutes { get; set; } = 0; // 0 = không giới hạn

        [Range(0, 100, ErrorMessage = "Số lần làm bài phải từ 0 đến 100")]
        public int MaxAttempts { get; set; } = 0; // 0 = không giới hạn

        public List<QuestionCreateVM> Questions { get; set; } = new List<QuestionCreateVM>();
    }

    public class QuestionCreateVM
    {
        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        [StringLength(1000, ErrorMessage = "Nội dung câu hỏi không được vượt quá 1000 ký tự")]
        public string QuestionText { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Điểm số phải từ 1 đến 100")]
        public int Points { get; set; } = 1;

        public List<OptionCreateVM> Options { get; set; } = new List<OptionCreateVM>();
    }

    public class OptionCreateVM
    {
        [Required(ErrorMessage = "Nội dung lựa chọn không được để trống")]
        [StringLength(500, ErrorMessage = "Nội dung lựa chọn không được vượt quá 500 ký tự")]
        public string OptionText { get; set; } = string.Empty;

        [StringLength(10)]
        public string? OptionLabel { get; set; }

        public bool IsCorrect { get; set; } = false;
    }
}

