namespace DMS.Models
{
    public enum DocumentStatus
    {
        Pending = 0,    // Chờ phê duyệt
        Approved = 1,   // Đã phê duyệt
        Rejected = 2,   // Đã từ chối
        Draft = 3       // Bản nháp
    }
}

