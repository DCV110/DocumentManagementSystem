using DMS.Models;

namespace DMS.ViewModels
{
    public class RecentActivityVM
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }
}

