# Hướng Dẫn UI Đã Triển Khai và Cách Test

## 📋 Danh Sách UI Đã Hoàn Thành

### ✅ 1. Hệ Thống (System)

#### 1.1. Đăng nhập/Đăng xuất ✅
- **File**: `DMS/Views/Account/Login.cshtml`
- **URL**: `/Account/Login`
- **Tính năng**: 
  - Form đăng nhập với email và password
  - Toggle hiển thị password
  - Remember me
  - Link đăng ký
- **Status**: ✅ Hoàn thành

#### 1.2. Quản lý người dùng (Admin) ✅
- **File**: `DMS/Views/Admin/UserManagement.cshtml`
- **URL**: `/Admin/UserManagement`
- **Tính năng**:
  - Danh sách tất cả users
  - Tìm kiếm users
  - Filter theo role
  - Tạo user mới (modal)
  - Khóa/Mở khóa user
  - Phân quyền (thay đổi role)
- **Status**: ✅ Hoàn thành

#### 1.3. 
Nhật ký hệ thống (Admin) ✅
- **File**: `DMS/Views/Admin/AuditLog.cshtml`
- **URL**: `/Admin/AuditLog`
- **Tính năng**:
  - Xem lịch sử hoạt động
  - Filter theo user, action, date
- **Status**: ✅ Hoàn thành (UI ready, backend cần implement)

#### 1.4. Sao lưu dữ liệu (Admin) ✅
- **File**: `DMS/Views/Admin/Backup.cshtml`
- **URL**: `/Admin/Backup`
- **Tính năng**:
  - Nút thực hiện backup
  - Lịch sử backup
- **Status**: ✅ Hoàn thành (UI ready, backend cần implement)

---

### ✅ 2. Quản Lý Thư Mục (Folder Management)

#### 2.1. Tạo/Sửa/Xóa thư mục ✅
- **Files**: 
  - `DMS/Views/Folder/Index.cshtml` - Danh sách thư mục
  - `DMS/Views/Folder/Create.cshtml` - Tạo mới
  - `DMS/Views/Folder/Edit.cshtml` - Sửa
- **URL**: 
  - `/Folder/Index`
  - `/Folder/Create`
  - `/Folder/Edit/{id}`
- **Tính năng**:
  - Xem danh sách thư mục
  - Tạo thư mục mới
  - Sửa thư mục
  - Xóa thư mục
  - Hiển thị số lượng documents trong mỗi thư mục
- **Status**: ✅ Hoàn thành

#### 2.2. Phân quyền thư mục ✅
- **File**: `DMS/Views/Folder/Permissions.cshtml`
- **URL**: `/Folder/Permissions/{id}`
- **Tính năng**:
  - Chọn users có quyền truy cập
  - Phân quyền: Xem, Đóng góp, Quản lý
- **Status**: ✅ Hoàn thành (UI ready, backend cần implement)

---

### ✅ 3. Quản Lý Tài Liệu (Document Management)

#### 3.1. Tải lên tài liệu ✅
- **File**: `DMS/Views/Document/Upload.cshtml`
- **URL**: `/Document/Upload`
- **Tính năng**:
  - Drag & drop upload
  - Click để chọn file
  - Preview file trước khi upload
  - Chọn môn học
  - Chọn thư mục
  - Nhập tiêu đề, mô tả
  - Gán tags
  - Progress bar
- **Status**: ✅ Hoàn thành

#### 3.2. Phê duyệt tài liệu ✅
- **File**: `DMS/Views/Document/Approval.cshtml`
- **URL**: `/Document/Approval`
- **Tính năng**:
  - Danh sách tài liệu chờ duyệt
  - Preview tài liệu
  - Nút Duyệt
  - Nút Từ chối (có modal nhập lý do)
  - Summary card số lượng chờ duyệt
- **Status**: ✅ Hoàn thành

#### 3.3. Quản lý phiên bản ⚠️
- **Status**: ⚠️ Chưa triển khai (cần thêm field Status vào Document model)

#### 3.4. Xóa/Lưu trữ tài liệu ⚠️
- **Status**: ⚠️ Chưa triển khai (có thể thêm vào MyDocuments)

#### 3.5. Gán nhãn (Tagging) ✅
- **File**: `DMS/Views/Document/Upload.cshtml`
- **Tính năng**: Input field cho tags trong form upload
- **Status**: ✅ Hoàn thành (UI ready, backend cần implement logic)

---

### ✅ 4. Khai Thác Tài Liệu (Document Usage)

#### 4.1. Tìm kiếm nâng cao ✅
- **File**: `DMS/Views/Document/Search.cshtml`
- **URL**: `/Document/Search`
- **Tính năng**:
  - Search box lớn
  - Filter theo tác giả
  - Filter theo loại file (PDF, Word, Excel, PowerPoint)
  - Filter theo ngày (từ ngày - đến ngày)
  - Hiển thị kết quả với preview
  - Link đến Preview và Download
- **Status**: ✅ Hoàn thành

#### 4.2. Xem trực tuyến (Preview) ✅
- **File**: `DMS/Views/Document/Preview.cshtml`
- **URL**: `/Document/Preview/{id}`
- **Tính năng**:
  - PDF viewer (iframe)
  - Image viewer
  - Thông tin tài liệu (sidebar)
  - Nút Download
  - Nút Đánh dấu yêu thích
  - Nút Chia sẻ
- **Status**: ✅ Hoàn thành

#### 4.3. Tải xuống ✅
- **Controller**: `DocumentController.Download()`
- **URL**: `/Document/Download/{id}`
- **Tính năng**: Download file về máy
- **Status**: ✅ Hoàn thành

#### 4.4. Thư viện tài liệu (Sinh viên) ✅
- **File**: `DMS/Views/Document/Library.cshtml`
- **URL**: `/Document/Library`
- **Tính năng**:
  - Grid view với document cards
  - Search box
  - Filter theo môn học
  - Sort (Mới nhất, Cũ nhất, Tên A-Z)
  - Pagination
  - Nút Xem và Tải xuống
  - Nút Đánh dấu yêu thích
- **Status**: ✅ Hoàn thành

#### 4.5. Đánh dấu yêu thích ⚠️
- **Status**: ⚠️ Chưa triển khai (cần thêm Favorite model và logic)

---

### ✅ 5. Tương Tác & Báo Cáo

#### 5.1. Phân quyền chia sẻ ⚠️
- **Status**: ⚠️ Chưa triển khai (có thể thêm vào Preview page)

#### 5.2. Báo cáo thống kê (Admin) ✅
- **File**: `DMS/Views/Admin/Reports.cshtml`
- **URL**: `/Admin/Reports`
- **Tính năng**:
  - Summary cards (Tổng users, documents, courses)
  - Bảng thống kê theo môn học
  - Hiển thị số lượng và dung lượng
- **Status**: ✅ Hoàn thành

#### 5.3. Thông báo ⚠️
- **Status**: ⚠️ Chưa triển khai (có notification icon trong header)

---

## 🧪 Hướng Dẫn Test

### Chuẩn Bị

1. **Đảm bảo database đã được tạo và seed data**
   - Chạy migrations: `dotnet ef database update`
   - Seed data sẽ tự động chạy khi start app

2. **Tài khoản test**:
   - **Admin**: `admin@dms.com` / `Admin@123`
   - **Giảng viên**: `instructor@dms.com` / `Instructor@123`
   - **Sinh viên**: `student@dms.com` / `Student@123`

3. **Chạy ứng dụng**:
   ```bash
   cd DMS
   dotnet run
   ```
   Hoặc nhấn F5 trong Visual Studio

---

### Test Theo Role

## 🔐 Test với Admin

### 1. Đăng nhập
- URL: `https://localhost:7275/Account/Login`
- Email: `admin@dms.com`
- Password: `Admin@123`
- ✅ Sau khi đăng nhập → Redirect đến Admin Dashboard

### 2. Quản lý người dùng
- URL: `https://localhost:7275/Admin/UserManagement`
- Hoặc click "Người dùng" trong header navigation
- **Test**:
  - ✅ Xem danh sách users
  - ✅ Tìm kiếm user
  - ✅ Filter theo role (Admin, Instructor, Student)
  - ✅ Click "Tạo người dùng mới" → Modal hiện ra
  - ✅ Tạo user mới với các thông tin
  - ✅ Thay đổi role của user (dropdown)
  - ✅ Khóa/Mở khóa user (icon lock)

### 3. Phê duyệt tài liệu
- URL: `https://localhost:7275/Document/Approval`
- Hoặc click "Phê duyệt" trong header navigation
- **Test**:
  - ✅ Xem danh sách tài liệu chờ duyệt
  - ✅ Click icon "visibility" để preview
  - ✅ Click icon "check_circle" để duyệt
  - ✅ Click icon "cancel" để từ chối → Modal hiện ra nhập lý do

### 4. Báo cáo thống kê
- URL: `https://localhost:7275/Admin/Reports`
- Hoặc click "Báo cáo" trong header navigation
- **Test**:
  - ✅ Xem summary cards (Tổng users, documents, courses)
  - ✅ Xem bảng thống kê theo môn học

### 5. Nhật ký hệ thống
- URL: `https://localhost:7275/Admin/AuditLog`
- Hoặc click "Nhật ký" trong header navigation
- **Test**:
  - ✅ Xem trang audit log (UI ready, backend cần implement)

### 6. Sao lưu dữ liệu
- URL: `https://localhost:7275/Admin/Backup`
- **Test**:
  - ✅ Click "Thực hiện sao lưu ngay"
  - ✅ Xem thông báo thành công

---

## 👨‍🏫 Test với Giảng viên

### 1. Đăng nhập
- URL: `https://localhost:7275/Account/Login`
- Email: `instructor@dms.com`
- Password: `Instructor@123`
- ✅ Sau khi đăng nhập → Redirect đến Instructor Dashboard

### 2. Dashboard
- URL: `https://localhost:7275/Home/InstructorDashboard`
- **Test**:
  - ✅ Xem KPI cards (Tổng tài liệu, lượt xem, lượt tải, chờ duyệt)
  - ✅ Xem khóa học đang giảng dạy
  - ✅ Xem tài liệu gần đây
  - ✅ Click "Tải lên tài liệu mới" → Chuyển đến Upload page

### 3. Tài liệu của tôi
- URL: `https://localhost:7275/Home/MyDocuments`
- Hoặc click "Tài liệu của tôi" trong sidebar
- **Test**:
  - ✅ Xem summary cards (Tổng lượt xem, lượt tải, chờ duyệt)
  - ✅ Search tài liệu
  - ✅ Filter (Tất cả, Đã đăng, Nháp)
  - ✅ Click "Thêm tài liệu mới" → Chuyển đến Upload page
  - ✅ Xem bảng tài liệu với status badges
  - ✅ Click icon "more_vert" để xem actions

### 4. Tải lên tài liệu
- URL: `https://localhost:7275/Document/Upload`
- Hoặc click "Tải lên tài liệu" trong sidebar
- **Test**:
  - ✅ Drag & drop file vào upload zone
  - ✅ Hoặc click "Chọn file"
  - ✅ Xem preview file đã chọn
  - ✅ Nhập tiêu đề, mô tả
  - ✅ Chọn môn học (required)
  - ✅ Chọn thư mục (optional)
  - ✅ Nhập tags (phân cách bằng dấu phẩy)
  - ✅ Click "Tải lên" → Redirect đến MyDocuments
  - ✅ File được lưu trong `wwwroot/uploads/documents/`

### 5. Phê duyệt tài liệu
- URL: `https://localhost:7275/Document/Approval`
- Hoặc click "Phê duyệt" trong sidebar
- **Test**: Tương tự như Admin

### 6. Quản lý thư mục
- URL: `https://localhost:7275/Folder/Index`
- Hoặc click "Quản lý thư mục" trong sidebar
- **Test**:
  - ✅ Xem danh sách thư mục
  - ✅ Click "Tạo thư mục mới" → Form tạo mới
  - ✅ Tạo thư mục với tên và môn học
  - ✅ Click icon "edit" để sửa
  - ✅ Click icon "lock" để phân quyền
  - ✅ Click icon "delete" để xóa (có confirm)

### 7. Tìm kiếm nâng cao
- URL: `https://localhost:7275/Document/Search`
- Hoặc click "Tìm kiếm" trong sidebar
- **Test**:
  - ✅ Nhập từ khóa tìm kiếm
  - ✅ Filter theo tác giả
  - ✅ Filter theo loại file
  - ✅ Filter theo ngày
  - ✅ Click "Tìm kiếm" → Xem kết quả
  - ✅ Click "Xem" hoặc "Tải xuống" từ kết quả

### 8. Xem trực tuyến
- URL: `https://localhost:7275/Document/Preview/{id}`
- **Test**:
  - ✅ Xem PDF trong iframe
  - ✅ Xem thông tin tài liệu ở sidebar
  - ✅ Click "Tải xuống"
  - ✅ Click "Đánh dấu yêu thích"
  - ✅ Click "Chia sẻ"

---

## 👨‍🎓 Test với Sinh viên

### 1. Đăng nhập
- URL: `https://localhost:7275/Account/Login`
- Email: `student@dms.com`
- Password: `Student@123`
- ✅ Sau khi đăng nhập → Redirect đến Student Dashboard

### 2. Dashboard
- URL: `https://localhost:7275/Home/StudentDashboard`
- **Test**:
  - ✅ Xem KPI cards
  - ✅ Xem tiến độ khóa học
  - ✅ Xem tài liệu gần đây

### 3. Thư viện tài liệu
- URL: `https://localhost:7275/Document/Library`
- Hoặc click "Thư viện tài liệu" trong header navigation
- **Test**:
  - ✅ Xem grid view với document cards
  - ✅ Search tài liệu
  - ✅ Filter theo môn học
  - ✅ Sort (Mới nhất, Cũ nhất, Tên A-Z)
  - ✅ Click "Xem" → Chuyển đến Preview
  - ✅ Click "Tải xuống" → Download file
  - ✅ Click icon "favorite_border" để đánh dấu yêu thích
  - ✅ Xem pagination nếu có nhiều documents

### 4. Tìm kiếm nâng cao
- URL: `https://localhost:7275/Document/Search`
- Hoặc click "Tìm kiếm" trong header navigation
- **Test**: Tương tự như Giảng viên

### 5. Xem trực tuyến
- URL: `https://localhost:7275/Document/Preview/{id}`
- **Test**: Tương tự như Giảng viên

---

## 🔍 Test Các Tính Năng Chung

### Upload File
1. **Test drag & drop**:
   - Kéo file vào upload zone
   - ✅ Zone đổi màu khi drag over
   - ✅ File được chọn và hiển thị preview

2. **Test file validation**:
   - Upload file > 50MB → ✅ Hiển thị lỗi
   - Upload file không đúng format → ✅ Hiển thị lỗi

3. **Test upload thành công**:
   - Chọn file hợp lệ
   - Điền đầy đủ thông tin
   - Click "Tải lên"
   - ✅ File được lưu trong `wwwroot/uploads/documents/`
   - ✅ Redirect đến MyDocuments
   - ✅ Document xuất hiện trong danh sách

### Search & Filter
1. **Test search**:
   - Nhập từ khóa
   - ✅ Kết quả được filter theo từ khóa

2. **Test filters**:
   - Chọn filter khác nhau
   - ✅ Kết quả được filter đúng

3. **Test pagination**:
   - Nếu có nhiều kết quả
   - ✅ Click số trang hoặc next/prev
   - ✅ Kết quả thay đổi đúng

### Preview
1. **Test PDF viewer**:
   - Click "Xem" trên document
   - ✅ PDF hiển thị trong iframe

2. **Test download**:
   - Click "Tải xuống"
   - ✅ File được download về máy

---

## ⚠️ Các Tính Năng Chưa Triển Khai

1. **Quản lý phiên bản**: Cần thêm field Status vào Document model
2. **Xóa/Lưu trữ tài liệu**: Có thể thêm vào MyDocuments
3. **Đánh dấu yêu thích**: Cần tạo Favorite model
4. **Phân quyền chia sẻ**: Có thể thêm vào Preview page
5. **Thông báo**: Cần implement notification system
6. **Nhật ký hệ thống**: Backend cần implement audit log
7. **Sao lưu dữ liệu**: Backend cần implement backup logic

---

## 📝 Notes

- Tất cả UI đã được thiết kế theo style của Login và Dashboard đã có
- Responsive design cho mobile và tablet
- Material Symbols icons được sử dụng nhất quán
- Color scheme: Green (#16a34a) cho primary actions, Blue cho info, Yellow cho warning

---

## 🐛 Troubleshooting

### Lỗi không tìm thấy file upload
- Kiểm tra folder `wwwroot/uploads/documents/` đã được tạo chưa
- Kiểm tra quyền ghi file

### Lỗi không hiển thị documents
- Kiểm tra database có data chưa
- Kiểm tra connection string trong `appsettings.json`

### Lỗi authorization
- Đảm bảo user đã được gán đúng role
- Kiểm tra `[Authorize(Roles = "...")]` attributes trong controllers

