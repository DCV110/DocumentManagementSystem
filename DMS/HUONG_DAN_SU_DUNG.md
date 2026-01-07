# HƯỚNG DẪN SỬ DỤNG HỆ THỐNG QUẢN LÝ TÀI LIỆU (DMS)

## MỤC LỤC

1. [Giới thiệu](#giới-thiệu)
2. [Đăng nhập và Đăng ký](#đăng-nhập-và-đăng-ký)
3. [Hướng dẫn cho Admin](#hướng-dẫn-cho-admin)
4. [Hướng dẫn cho Giảng viên](#hướng-dẫn-cho-giảng-viên)
5. [Hướng dẫn cho Sinh viên](#hướng-dẫn-cho-sinh-viên)
6. [Các chức năng chung](#các-chức-năng-chung)
7. [Câu hỏi thường gặp (FAQ)](#câu-hỏi-thường-gặp-faq)

---

## GIỚI THIỆU

Hệ thống Quản lý Tài liệu (Document Management System - DMS) là một ứng dụng web giúp quản lý, chia sẻ và phê duyệt tài liệu học tập trong môi trường giáo dục. Hệ thống hỗ trợ 3 vai trò chính:

- **Admin**: Quản trị viên hệ thống với quyền quản lý toàn bộ hệ thống
- **Instructor**: Giảng viên có thể upload, quản lý tài liệu và chia sẻ với sinh viên
- **Student**: Sinh viên có thể xem, tải xuống tài liệu từ các khóa học đã đăng ký

---

## ĐĂNG NHẬP VÀ ĐĂNG KÝ

### Đăng nhập

1. Truy cập trang đăng nhập của hệ thống
2. Nhập **Email** hoặc **Tên đăng nhập**
3. Nhập **Mật khẩu**
4. (Tùy chọn) Tích vào **"Ghi nhớ đăng nhập"** để giữ phiên đăng nhập trong 30 ngày
5. Nhấn nút **"Đăng nhập"**

### Đăng ký tài khoản mới

1. Truy cập trang đăng ký
2. Điền đầy đủ thông tin:
   - **Họ và tên**
   - **Email**
   - **Mật khẩu** (tối thiểu 6 ký tự)
   - **Xác nhận mật khẩu**
   - **Mã số sinh viên** (nếu là sinh viên)
   - **Khoa**
3. Nhấn nút **"Đăng ký"**
4. Sau khi đăng ký thành công, bạn sẽ được chuyển đến trang đăng nhập

**Lưu ý**: Tài khoản mới đăng ký sẽ có quyền **Student** mặc định. Để có quyền **Instructor** hoặc **Admin**, vui lòng liên hệ quản trị viên.

---

## HƯỚNG DẪN CHO ADMIN

### 1. Dashboard Quản trị viên

**Đường dẫn**: Dashboard → Admin Dashboard

**Chức năng**:
- Xem tổng quan hệ thống với các chỉ số KPI:
  - Tổng số người dùng
  - Tổng số tài liệu
  - Số tài liệu chờ duyệt
  - Trạng thái hệ thống (dung lượng lưu trữ)
- Xem hoạt động gần đây (upload tài liệu, đăng ký user mới)
- Xem cảnh báo quan trọng
- Truy cập nhanh các chức năng quản lý

### 2. Quản lý Người dùng

**Đường dẫn**: Quản lý người dùng

**Chức năng**:
- **Xem danh sách người dùng**: Hiển thị tất cả user trong hệ thống với thông tin: Tên, Email, Vai trò, Trạng thái
- **Tạo người dùng mới**:
  1. Nhấn nút "Tạo người dùng mới"
  2. Điền thông tin: Họ tên, Email, Mật khẩu, Vai trò (Admin/Instructor/Student), Khoa
  3. Nhấn "Lưu"
- **Chỉnh sửa người dùng**:
  1. Tìm user cần sửa trong danh sách
  2. Nhấn icon "Chỉnh sửa"
  3. Cập nhật thông tin
  4. Nhấn "Lưu"
- **Khóa/Mở khóa tài khoản**:
  1. Tìm user trong danh sách
  2. Nhấn nút "Khóa" hoặc "Mở khóa"
  3. Xác nhận hành động
- **Gán vai trò**:
  1. Tìm user cần gán vai trò
  2. Nhấn "Gán vai trò"
  3. Chọn vai trò mới
  4. Nhấn "Lưu"
- **Xóa người dùng**: (Chỉ nên thực hiện khi cần thiết)

### 3. Quản lý Khóa học

**Đường dẫn**: Quản lý khóa học

**Chức năng**:
- **Xem danh sách khóa học**: Hiển thị tất cả khóa học với thông tin: Tên khóa học, Giảng viên, Số sinh viên, Số tài liệu
- **Tạo khóa học mới**:
  1. Nhấn nút "Tạo khóa học mới"
  2. Điền thông tin: Tên khóa học, Mô tả, Chọn giảng viên
  3. Nhấn "Lưu"
- **Chỉnh sửa khóa học**:
  1. Tìm khóa học cần sửa
  2. Nhấn icon "Chỉnh sửa"
  3. Cập nhật thông tin
  4. Nhấn "Lưu"
- **Xóa khóa học**: 
  1. Tìm khóa học cần xóa
  2. Nhấn icon "Xóa"
  3. Xác nhận xóa
- **Quản lý đăng ký sinh viên**:
  1. Nhấn "Quản lý đăng ký" ở khóa học tương ứng
  2. Xem danh sách sinh viên đã đăng ký
  3. Thêm/Xóa sinh viên khỏi khóa học

### 4. Quản lý Tài liệu (System-wide)

**Đường dẫn**: Quản lý tài liệu

**Chức năng**:
- **Xem tất cả tài liệu** trong hệ thống (không phân biệt người sở hữu)
- **Lọc tài liệu** theo:
  - Từ khóa tìm kiếm
  - Khóa học
  - Trạng thái (Tất cả/Đã phê duyệt/Chờ duyệt/Đã từ chối)
  - Người tải lên
- **Thao tác hàng loạt (Bulk Operations)**:
  - **Phê duyệt hàng loạt**:
    1. Chọn các tài liệu cần phê duyệt (tích checkbox)
    2. Nhấn nút "Phê duyệt đã chọn"
    3. Xác nhận
  - **Từ chối hàng loạt**:
    1. Chọn các tài liệu cần từ chối
    2. Nhấn nút "Từ chối đã chọn"
    3. Nhập lý do từ chối
    4. Xác nhận
  - **Xóa hàng loạt**:
    1. Chọn các tài liệu cần xóa
    2. Nhấn nút "Xóa đã chọn"
    3. Xác nhận
- **Xem chi tiết tài liệu**: Nhấn vào tên tài liệu để xem thông tin chi tiết
- **Chỉnh sửa tài liệu**: Nhấn icon "Chỉnh sửa" để sửa thông tin tài liệu
- **Xóa tài liệu**: Nhấn icon "Xóa" và xác nhận

### 5. Phê duyệt Tài liệu

**Đường dẫn**: Phê duyệt

**Chức năng**:
- **Xem danh sách tài liệu chờ duyệt**:
  - Tab "Chờ duyệt": Hiển thị tài liệu mới upload hoặc yêu cầu public
  - Tab "Đã phê duyệt": Hiển thị tài liệu đã được phê duyệt
  - Tab "Đã từ chối": Hiển thị tài liệu đã bị từ chối
- **Phê duyệt tài liệu**:
  1. Vào tab "Chờ duyệt"
  2. Tìm tài liệu cần phê duyệt
  3. Nhấn icon "Duyệt" (✓)
  4. Tài liệu sẽ được chuyển sang trạng thái "Đã phê duyệt"
- **Phê duyệt public share**:
  1. Vào tab "Chờ duyệt"
  2. Tìm tài liệu có yêu cầu "Chia sẻ công khai"
  3. Nhấn icon "Duyệt chia sẻ công khai"
  4. Tài liệu sẽ xuất hiện trong "Thư viện tài liệu"
- **Từ chối tài liệu**:
  1. Tìm tài liệu cần từ chối
  2. Nhấn icon "Từ chối" (✗)
  3. Nhập lý do từ chối
  4. Xác nhận
- **Bỏ public**:
  1. Vào tab "Đã phê duyệt"
  2. Tìm tài liệu đang public
  3. Nhấn icon "Bỏ public"
  4. Xác nhận
  5. Tài liệu sẽ không còn hiển thị trong "Thư viện tài liệu"
- **Mở lại public**:
  1. Vào tab "Đã phê duyệt"
  2. Tìm tài liệu đã bị bỏ public (có icon "Mở lại public")
  3. Nhấn icon "Mở lại public"
  4. Xác nhận

### 6. Báo cáo

**Đường dẫn**: Báo cáo

**Chức năng**:
- **Thống kê người dùng**:
  - Tổng số user theo vai trò
  - Số user mới trong tuần/tháng
  - Biểu đồ tăng trưởng user
- **Thống kê tài liệu**:
  - Tổng số tài liệu
  - Số tài liệu theo trạng thái
  - Số tài liệu upload theo thời gian
  - Top tài liệu được xem/tải nhiều nhất
- **Thống kê khóa học**:
  - Số khóa học
  - Số tài liệu theo khóa học
  - Số sinh viên đăng ký theo khóa học
- **Thống kê hoạt động**:
  - Số lượt xem/tải xuống
  - Hoạt động theo ngày/tuần/tháng
- **Xuất báo cáo**: (Nếu có chức năng)

### 7. Nhật ký Hoạt động (Audit Log)

**Đường dẫn**: Nhật ký

**Chức năng**:
- **Xem lịch sử hoạt động** của tất cả người dùng trong hệ thống
- **Lọc nhật ký** theo:
  - Người dùng
  - Hành động (Create, Update, Delete, View, Approve, Reject, etc.)
  - Khoảng thời gian (Từ ngày - Đến ngày)
- **Thông tin hiển thị**:
  - Thời gian
  - Người thực hiện
  - Hành động
  - Loại entity (Document, User, Course, Folder)
  - Mô tả chi tiết
  - IP Address
  - User Agent
- **Phân trang**: Xem thêm các hoạt động cũ hơn

### 8. Sao lưu và Khôi phục (Backup & Restore)

**Đường dẫn**: Sao lưu

**Chức năng**:
- **Tạo bản sao lưu**:
  1. Nhấn nút "Tạo bản sao lưu"
  2. Nhập tên bản backup (tùy chọn)
  3. Nhập mô tả (tùy chọn)
  4. Chọn loại backup:
     - **Backup Database**: Sao lưu cơ sở dữ liệu
     - **Backup Files**: Sao lưu thư mục uploads
     - **Backup All**: Sao lưu cả database và files
  5. Nhấn "Tạo backup"
  6. Chờ quá trình backup hoàn tất
- **Xem lịch sử backup**:
  - Danh sách tất cả các bản backup đã tạo
  - Thông tin: Tên, Ngày tạo, Người tạo, Kích thước, Trạng thái
- **Khôi phục từ backup**:
  1. Tìm bản backup cần khôi phục
  2. Nhấn nút "Khôi phục"
  3. Xác nhận khôi phục (Cảnh báo: Dữ liệu hiện tại sẽ bị ghi đè)
  4. Nhập ghi chú khôi phục (tùy chọn)
  5. Nhấn "Xác nhận khôi phục"
- **Xóa bản backup**:
  1. Tìm bản backup cần xóa
  2. Nhấn icon "Xóa"
  3. Xác nhận

**Lưu ý quan trọng**:
- Backup database yêu cầu quyền BACKUP DATABASE trong SQL Server
- Backup files yêu cầu quyền Write vào thư mục uploads
- Khôi phục sẽ ghi đè dữ liệu hiện tại, nên tạo backup trước khi khôi phục

### 9. Thông tin Hệ thống

**Đường dẫn**: Thông tin hệ thống

**Chức năng**:
- **Xem thông tin Process và Identity**:
  - Windows Identity Name (User IIS đang chạy ứng dụng)
  - Process Information (Tên, ID, Thời gian start)
  - Environment Variables
  - IIS Application Pool Info (nếu có)
  - ASP.NET Core Environment
  - HTTP Context Info
- **Mục đích**: Giúp xác định user IIS để cấu hình quyền cho backup và các thao tác hệ thống

### 10. Thông báo

**Chức năng**:
- **Xem thông báo**: Nhấn vào icon chuông ở góc trên bên phải
- **Các loại thông báo**:
  - Yêu cầu phê duyệt public mới (từ giảng viên)
  - Tài liệu đã được phê duyệt/từ chối
  - Các hoạt động khác liên quan đến tài liệu của bạn
- **Đánh dấu đã đọc**:
  - Click vào thông báo để tự động đánh dấu đã đọc
  - Hoặc nhấn "Đánh dấu tất cả đã đọc"
- **Badge**: Số thông báo chưa đọc hiển thị trên icon chuông

---

## HƯỚNG DẪN CHO GIẢNG VIÊN

### 1. Dashboard Giảng viên

**Đường dẫn**: Dashboard → Instructor Dashboard

**Chức năng**:
- Xem tổng quan tài liệu của bạn:
  - Tổng số tài liệu
  - Số tài liệu đã phê duyệt
  - Số lượt xem/tải xuống
  - Số khóa học
- Xem tài liệu gần đây
- Xem thống kê theo khóa học

### 2. Tài liệu của tôi

**Đường dẫn**: Tài liệu của tôi

**Chức năng**:
- **Xem danh sách tài liệu** của bạn:
  - Hiển thị theo dạng grid hoặc list
  - Có thể xem theo thư mục
  - Sắp xếp theo: Mới nhất, Cũ nhất, Tên (A-Z, Z-A), Lượt tải, Kích thước
- **Upload tài liệu**:
  1. Nhấn nút "Tải lên tài liệu" hoặc kéo thả file vào vùng upload
  2. Chọn một hoặc nhiều file (tối đa 50MB/file)
  3. (Tùy chọn) Nhập mô tả chung cho tất cả file
  4. (Tùy chọn) Chọn thư mục để lưu
  5. Nhấn "Tải lên"
  6. Nếu có file trùng tên, hệ thống sẽ hỏi: Giữ cả hai, Thay thế, hoặc Hủy
- **Tạo thư mục**:
  1. Nhấn nút "Tạo thư mục"
  2. Nhập tên thư mục
  3. (Tùy chọn) Chọn thư mục cha
  4. (Tùy chọn) Chọn khóa học
  5. Nhấn "Tạo"
- **Chỉnh sửa tài liệu**:
  1. Tìm tài liệu cần sửa
  2. Nhấn icon menu (3 chấm) → "Chỉnh sửa"
  3. Cập nhật: Tên, Mô tả, Thư mục
  4. Nhấn "Lưu"
- **Thay thế file**:
  1. Tìm tài liệu cần thay thế
  2. Nhấn icon menu → "Thay thế"
  3. Chọn file mới
  4. Xác nhận
- **Xem trước tài liệu**:
  1. Nhấn vào tên tài liệu hoặc icon menu → "Xem"
  2. Tài liệu sẽ mở trong tab mới
- **Tải xuống tài liệu**:
  1. Nhấn icon menu → "Tải xuống"
  2. File sẽ được tải về máy
- **Chia sẻ tài liệu**:
  1. Nhấn icon menu → "Chia sẻ"
  2. Chọn cách chia sẻ:
     - **Chia sẻ với khóa học**: Chọn khóa học → Nhấn "Chia sẻ"
     - **Chia sẻ công khai (Public)**: 
       - Nếu là Admin: Tài liệu sẽ được public ngay
       - Nếu là Instructor: Gửi yêu cầu, chờ Admin phê duyệt
     - **Tạo link chia sẻ**: Tạo link để chia sẻ với người khác
  3. Xác nhận
- **Xóa tài liệu**:
  1. Nhấn icon menu → "Xóa"
  2. Xác nhận
  3. Tài liệu sẽ được chuyển vào "Thùng rác"

### 3. Thư mục

**Chức năng**:
- **Tạo thư mục**:
  1. Nhấn "Tạo thư mục"
  2. Nhập tên thư mục
  3. (Tùy chọn) Chọn thư mục cha (để tạo thư mục con)
  4. (Tùy chọn) Chọn khóa học
  5. Nhấn "Tạo"
- **Chỉnh sửa thư mục**:
  1. Tìm thư mục cần sửa
  2. Nhấn icon menu → "Chỉnh sửa"
  3. Cập nhật tên, mô tả
  4. Nhấn "Lưu"
- **Xóa thư mục**:
  1. Nhấn icon menu → "Xóa"
  2. Xác nhận
  3. Thư mục và tất cả nội dung sẽ được chuyển vào "Thùng rác"
- **Xem nội dung thư mục**: Click vào thư mục để xem các tài liệu và thư mục con bên trong

### 4. Phê duyệt (Nếu có quyền)

**Đường dẫn**: Phê duyệt

**Chức năng**: Tương tự như Admin, nhưng chỉ có thể phê duyệt tài liệu trong phạm vi quyền của mình

### 5. Thư viện Tài liệu

**Đường dẫn**: Thư viện tài liệu

**Chức năng**:
- **Xem tài liệu công khai** đã được Admin phê duyệt
- **Lọc tài liệu**:
  - Theo khóa học
  - Tìm kiếm theo tên
- **Sắp xếp**: Mới nhất, Cũ nhất, Tên, Lượt tải, Kích thước
- **Xem trước/Tải xuống**: Click vào tài liệu để xem hoặc tải xuống

### 6. Tìm kiếm

**Đường dẫn**: Tìm kiếm

**Chức năng**:
- **Tìm kiếm nâng cao**:
  - Từ khóa (tên tài liệu, mô tả)
  - Tác giả
  - Khoảng thời gian (Từ ngày - Đến ngày)
  - Loại file (PDF, DOCX, PPTX, etc.)
- **Kết quả tìm kiếm**:
  - Hiển thị danh sách tài liệu khớp
  - Có thể xem trước, tải xuống
  - Phân trang kết quả

### 7. Thùng rác

**Đường dẫn**: Thùng rác

**Chức năng**:
- **Xem tài liệu/thư mục đã xóa**
- **Khôi phục**:
  1. Tìm tài liệu/thư mục cần khôi phục
  2. Nhấn icon "Khôi phục"
  3. Tài liệu/thư mục sẽ được khôi phục về vị trí ban đầu
- **Xóa vĩnh viễn**:
  1. Tìm tài liệu cần xóa vĩnh viễn
  2. Nhấn icon "Xóa vĩnh viễn"
  3. Xác nhận (Cảnh báo: Không thể khôi phục sau khi xóa vĩnh viễn)
- **Xóa tất cả**: Xóa vĩnh viễn tất cả tài liệu trong thùng rác

### 8. Thông báo

**Chức năng**: Tương tự như Admin
- Nhận thông báo khi:
  - Tài liệu của bạn được phê duyệt/từ chối
  - Yêu cầu public của bạn được phê duyệt/từ chối

---

## HƯỚNG DẪN CHO SINH VIÊN

### 1. Dashboard Sinh viên

**Đường dẫn**: Tổng quan

**Chức năng**:
- Xem tổng quan:
  - Số khóa học đã đăng ký
  - Số tài liệu có thể truy cập
  - Tài liệu mới trong tuần/tháng
  - Thống kê theo khóa học
- Xem timeline tài liệu mới

### 2. Thư viện Tài liệu

**Đường dẫn**: Thư viện tài liệu

**Chức năng**:
- **Xem tài liệu công khai** đã được Admin phê duyệt
- **Lọc tài liệu**:
  - Theo khóa học
  - Tìm kiếm theo tên
- **Sắp xếp**: Mới nhất, Cũ nhất, Tên, Lượt tải, Kích thước
- **Xem trước**: Click vào tài liệu để xem nội dung
- **Tải xuống**: Click vào tên tài liệu → "Tải xuống"

**Lưu ý**: 
- Chỉ hiển thị tài liệu đã được Admin phê duyệt public
- Tài liệu được chia sẻ vào khóa học (chưa public) chỉ hiển thị trong trang khóa học, không hiển thị ở đây

### 3. Khóa học của tôi

**Đường dẫn**: Khóa học của tôi

**Chức năng**:
- **Xem danh sách khóa học** đã đăng ký
- **Xem chi tiết khóa học**:
  1. Click vào khóa học
  2. Xem thông tin khóa học
  3. Xem tài liệu trong khóa học:
     - Tài liệu được giảng viên chia sẻ vào khóa học này
     - Có thể xem trước, tải xuống
     - Sắp xếp và tìm kiếm tài liệu

**Lưu ý**: 
- Chỉ thấy tài liệu trong các khóa học đã đăng ký
- Tài liệu public (trong Thư viện) không tự động xuất hiện ở đây, trừ khi được chia sẻ vào khóa học

### 4. Tìm kiếm

**Đường dẫn**: Tìm kiếm

**Chức năng**: Tương tự như Giảng viên
- Tìm kiếm trong tất cả tài liệu có quyền truy cập
- Lọc theo: Từ khóa, Tác giả, Thời gian, Loại file

### 5. Lịch học (Schedule)

**Đường dẫn**: Lịch học (nếu có)

**Chức năng**:
- Xem timeline tài liệu mới
- Xem thống kê tài liệu theo thời gian
- Xem tài liệu mới trong tuần/tháng

### 6. Thông báo

**Chức năng**: 
- Nhận thông báo về:
  - Tài liệu mới trong khóa học đã đăng ký
  - Các hoạt động liên quan

---

## CÁC CHỨC NĂNG CHUNG

### 1. Thông báo (Notifications)

**Vị trí**: Icon chuông ở góc trên bên phải (tất cả các trang)

**Chức năng**:
- **Xem thông báo**:
  1. Click vào icon chuông
  2. Dropdown hiển thị danh sách thông báo
  3. Badge hiển thị số thông báo chưa đọc
- **Đánh dấu đã đọc**:
  - Click vào thông báo → Tự động đánh dấu đã đọc
  - Hoặc nhấn "Đánh dấu tất cả đã đọc"
- **Điều hướng**: Click vào thông báo để chuyển đến trang liên quan
- **Tự động refresh**: Số thông báo chưa đọc tự động cập nhật mỗi 30 giây

**Các loại thông báo**:
- Yêu cầu phê duyệt public mới (Admin)
- Tài liệu đã được phê duyệt/từ chối (Instructor)
- Yêu cầu public đã được phê duyệt/từ chối (Instructor)

### 2. Tìm kiếm Tài liệu

**Đường dẫn**: Tìm kiếm

**Chức năng**:
- **Tìm kiếm cơ bản**: Nhập từ khóa vào ô tìm kiếm
- **Tìm kiếm nâng cao**:
  - **Từ khóa**: Tìm trong tên và mô tả tài liệu
  - **Tác giả**: Tìm theo tên người upload
  - **Khoảng thời gian**: Chọn "Từ ngày" và "Đến ngày"
  - **Loại file**: Chọn loại file (PDF, DOCX, PPTX, XLSX, ZIP, etc.)
- **Kết quả**:
  - Hiển thị danh sách tài liệu khớp
  - Phân trang (20 kết quả/trang)
  - Có thể xem trước, tải xuống từ kết quả tìm kiếm

### 3. Xem trước Tài liệu

**Chức năng**:
- **Xem trước**: Click vào tên tài liệu hoặc icon "Xem"
- **Thông tin hiển thị**:
  - Tên tài liệu
  - Mô tả
  - Người upload
  - Khóa học (nếu có)
  - Ngày upload
  - Số lượt xem/tải xuống
  - Kích thước file
- **Xem nội dung**:
  - PDF: Hiển thị trực tiếp trong trình duyệt
  - Hình ảnh: Hiển thị trực tiếp
  - ZIP: Hiển thị danh sách file bên trong
  - File khác: Có thể tải xuống để xem
- **Tải xuống**: Nhấn nút "Tải xuống"

### 4. Chia sẻ Tài liệu

**Các cách chia sẻ**:

#### a. Chia sẻ với Khóa học
1. Chọn tài liệu cần chia sẻ
2. Nhấn "Chia sẻ"
3. Chọn "Chia sẻ với khóa học"
4. Chọn khóa học
5. Nhấn "Chia sẻ"
6. Tài liệu sẽ xuất hiện trong khóa học đó

#### b. Chia sẻ Công khai (Public)
1. Chọn tài liệu cần chia sẻ
2. Nhấn "Chia sẻ"
3. Chọn "Chia sẻ công khai"
4. Nếu là Admin: Tài liệu được public ngay
5. Nếu là Instructor: Gửi yêu cầu, chờ Admin phê duyệt
6. Sau khi được phê duyệt, tài liệu sẽ xuất hiện trong "Thư viện tài liệu"

#### c. Tạo Link Chia sẻ
1. Chọn tài liệu cần chia sẻ
2. Nhấn "Chia sẻ"
3. Chọn "Tạo link chia sẻ"
4. (Tùy chọn) Đặt thời gian hết hạn
5. Copy link và chia sẻ với người khác
6. Người nhận link có thể xem/tải tài liệu mà không cần đăng nhập

### 5. Quản lý Thư mục

**Chức năng**:
- **Tạo thư mục**:
  1. Nhấn "Tạo thư mục"
  2. Nhập tên thư mục
  3. (Tùy chọn) Chọn thư mục cha (để tạo thư mục con)
  4. (Tùy chọn) Chọn khóa học
  5. Nhấn "Tạo"
- **Chỉnh sửa thư mục**:
  1. Tìm thư mục cần sửa
  2. Nhấn icon menu → "Chỉnh sửa"
  3. Cập nhật tên, mô tả
  4. Nhấn "Lưu"
- **Xóa thư mục**:
  1. Nhấn icon menu → "Xóa"
  2. Xác nhận
  3. Thư mục và tất cả nội dung sẽ vào "Thùng rác"
- **Xem nội dung**: Click vào thư mục để xem tài liệu và thư mục con bên trong
- **Điều hướng**: Sử dụng breadcrumb để quay lại thư mục cha

### 6. Thùng rác

**Chức năng**:
- **Xem tài liệu/thư mục đã xóa**
- **Khôi phục**:
  1. Tìm tài liệu/thư mục cần khôi phục
  2. Nhấn icon "Khôi phục"
  3. Tài liệu/thư mục sẽ được khôi phục về vị trí ban đầu
- **Xóa vĩnh viễn**:
  1. Tìm tài liệu cần xóa vĩnh viễn
  2. Nhấn icon "Xóa vĩnh viễn"
  3. Xác nhận (Cảnh báo: Không thể khôi phục)
- **Xóa tất cả**: Xóa vĩnh viễn tất cả trong thùng rác

**Lưu ý**: 
- Tài liệu/thư mục trong thùng rác sẽ tự động xóa sau một thời gian (nếu có cấu hình)
- Chỉ chủ sở hữu hoặc Admin mới có thể xem thùng rác của mình

---

## CÂU HỎI THƯỜNG GẶP (FAQ)

### 1. Tại sao tôi không thấy tài liệu trong "Thư viện tài liệu"?

**Trả lời**: 
- "Thư viện tài liệu" chỉ hiển thị tài liệu đã được Admin phê duyệt public
- Tài liệu được chia sẻ vào khóa học (chưa public) chỉ hiển thị trong trang khóa học
- Để tài liệu xuất hiện trong "Thư viện tài liệu", cần:
  1. Yêu cầu chia sẻ công khai
  2. Chờ Admin phê duyệt

### 2. Tại sao tôi không thấy tài liệu trong khóa học của tôi?

**Trả lời**:
- Chỉ thấy tài liệu được giảng viên chia sẻ vào khóa học đó
- Tài liệu public (trong Thư viện) không tự động xuất hiện trong khóa học
- Đảm bảo bạn đã đăng ký khóa học

### 3. Làm thế nào để upload nhiều file cùng lúc?

**Trả lời**:
1. Vào "Tài liệu của tôi"
2. Nhấn "Tải lên tài liệu"
3. Chọn nhiều file cùng lúc (giữ Ctrl/Cmd khi chọn)
4. Nhấn "Tải lên"

### 4. File của tôi bị trùng tên, tôi nên làm gì?

**Trả lời**:
- Khi upload file trùng tên, hệ thống sẽ hỏi:
  - **Giữ cả hai**: Giữ file cũ và file mới (file mới sẽ có tên khác)
  - **Thay thế**: Thay thế file cũ bằng file mới
  - **Hủy**: Không upload file mới

### 5. Tôi có thể khôi phục tài liệu đã xóa không?

**Trả lời**:
- Có, tài liệu đã xóa sẽ vào "Thùng rác"
- Vào "Thùng rác" → Tìm tài liệu → Nhấn "Khôi phục"
- Sau khi xóa vĩnh viễn, không thể khôi phục

### 6. Tại sao yêu cầu public của tôi chưa được phê duyệt?

**Trả lời**:
- Yêu cầu public cần được Admin phê duyệt
- Kiểm tra thông báo để xem trạng thái
- Nếu bị từ chối, xem lý do trong thông báo hoặc trang "Phê duyệt" → Tab "Đã từ chối"

### 7. Làm thế nào để thay đổi mật khẩu?

**Trả lời**:
- Liên hệ Admin để đổi mật khẩu
- Hoặc sử dụng chức năng "Quên mật khẩu" (nếu có)

### 8. Tôi có thể xóa tài liệu của người khác không?

**Trả lời**:
- Chỉ Admin mới có thể xóa tài liệu của người khác
- Giảng viên chỉ có thể xóa tài liệu của chính mình
- Sinh viên không thể xóa tài liệu

### 9. Backup có an toàn không?

**Trả lời**:
- Backup tạo bản sao của database và files
- Backup được lưu trong thư mục `Backups` của ứng dụng
- Nên tạo backup thường xuyên để đảm bảo an toàn dữ liệu
- Trước khi khôi phục, nên tạo backup hiện tại

### 10. Tại sao tôi không thấy hoạt động trong "Hoạt động gần đây"?

**Trả lời**:
- "Hoạt động gần đây" hiển thị:
  - 5 tài liệu upload gần nhất
  - 3 user đăng ký gần nhất
- Nếu không có hoạt động, có thể:
  - Chưa có tài liệu nào được upload
  - Chưa có user mới đăng ký
  - Tất cả tài liệu đã bị xóa

---

## HỖ TRỢ

Nếu gặp vấn đề hoặc cần hỗ trợ, vui lòng:
- Liên hệ Admin hệ thống
- Kiểm tra "Nhật ký" để xem lịch sử hoạt động
- Kiểm tra "Thông tin hệ thống" để xem cấu hình

---

**Phiên bản**: 1.0  
**Cập nhật lần cuối**: 2025-01-03

