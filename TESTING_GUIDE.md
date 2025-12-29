# Hướng Dẫn Test Trang Đăng Nhập

## 📋 Mục Lục
1. [Chuẩn Bị](#chuẩn-bị)
2. [Chạy Ứng Dụng](#chạy-ứng-dụng)
3. [Test Trang Đăng Nhập](#test-trang-đăng-nhập)
4. [Test Đăng Ký](#test-đăng-ký)
5. [Test Đăng Xuất](#test-đăng-xuất)
6. [Tài Khoản Test](#tài-khoản-test)
7. [Kiểm Tra Database](#kiểm-tra-database)

---

## 🔧 Chuẩn Bị

### 1. Kiểm tra SQL Server
- Đảm bảo SQL Server đang chạy
- Instance: `LAPTOP-2CVL2A9H\MSSQLSERVER01`
- Windows Authentication đã được bật

### 2. Kiểm tra Connection String
File: `DMS/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LAPTOP-2CVL2A9H\\MSSQLSERVER01;Database=DMS_University_DB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

---

## 🚀 Chạy Ứng Dụng

### Cách 1: Chạy từ Visual Studio
1. Mở solution `DMS.sln` trong Visual Studio
2. Nhấn `F5` hoặc click **Run**
3. Trình duyệt sẽ tự động mở tại: `https://localhost:7275` hoặc `http://localhost:5068`

### Cách 2: Chạy từ Terminal/Command Prompt
```bash
cd DMS
dotnet run
```

### Cách 3: Chạy với HTTPS
```bash
cd DMS
dotnet run --launch-profile https
```

---

## 🧪 Test Trang Đăng Nhập

### Bước 1: Truy cập Trang Đăng Nhập

**URL:** 
- `https://localhost:7275/Account/Login`
- `http://localhost:5068/Account/Login`

Hoặc click vào link **"Đăng nhập"** trên navbar (nếu chưa đăng nhập)

### Bước 2: Kiểm Tra UI

✅ **Kiểm tra các elements:**
- [ ] Left section (40%) có background xanh đậm với hình ảnh mờ
- [ ] Icon graduation cap màu trắng trong ô vuông trắng
- [ ] Tiêu đề "Secure Academic Document Management" màu trắng
- [ ] Subtitle màu trắng
- [ ] Right section (60%) có nền trắng
- [ ] Form có tiêu đề "Welcome back"
- [ ] Input field "Institutional Email" với placeholder "student@university.edu"
- [ ] Input field "Password" với placeholder "Enter your password"
- [ ] Icon eye để toggle password visibility
- [ ] Checkbox "Remember me"
- [ ] Link "Forgot password?"
- [ ] Nút "Sign In" màu xanh
- [ ] Link "Register here"
- [ ] Footer links: Privacy Policy, Terms of Service, Help Center

### Bước 3: Test Đăng Nhập với Tài Khoản Admin

**Tài khoản mẫu (tự động tạo khi chạy lần đầu):**
- **Email:** `admin@dms.com`
- **Password:** `Admin@123`

**Các bước:**
1. Nhập email: `admin@dms.com`
2. Nhập password: `Admin@123`
3. (Tùy chọn) Tích vào "Remember me"
4. Click nút **"Sign In"**

**Kết quả mong đợi:**
- ✅ Đăng nhập thành công
- ✅ Redirect đến trang `/Course/Index`
- ✅ Navbar hiển thị "Xin chào admin@dms.com!" và nút "Đăng xuất"

### Bước 4: Test Validation

#### Test 1: Để trống email và password
- Nhấn "Sign In" mà không nhập gì
- **Kết quả:** Hiển thị lỗi "Vui lòng nhập email và mật khẩu."

#### Test 2: Email hoặc password sai
- Email: `wrong@email.com`
- Password: `wrongpassword`
- **Kết quả:** Hiển thị lỗi "Email hoặc mật khẩu không đúng."

#### Test 3: Email đúng, password sai
- Email: `admin@dms.com`
- Password: `WrongPassword123`
- **Kết quả:** Hiển thị lỗi "Email hoặc mật khẩu không đúng."

### Bước 5: Test Password Toggle

1. Nhập password vào field
2. Click icon **eye** bên phải password field
3. **Kết quả:** Password hiển thị dạng text
4. Click lại icon eye
5. **Kết quả:** Password ẩn lại (dạng dots)

---

## 📝 Test Đăng Ký

### Bước 1: Truy cập Trang Đăng Ký

**URL:** 
- `https://localhost:7275/Account/Register`
- `http://localhost:5068/Account/Register`

Hoặc click link **"Register here"** trên trang đăng nhập

### Bước 2: Điền Form Đăng Ký

**Thông tin test:**
- **Full Name:** `Nguyễn Văn A`
- **Institutional Email:** `student1@university.edu`
- **Password:** `Student123`
- **Confirm Password:** `Student123`

### Bước 3: Submit Form

**Kết quả mong đợi:**
- ✅ Đăng ký thành công
- ✅ Tự động đăng nhập
- ✅ Redirect đến trang `/Course/Index`

### Bước 4: Test Validation Đăng Ký

#### Test 1: Để trống các field
- Submit form trống
- **Kết quả:** Hiển thị lỗi "Vui lòng điền đầy đủ thông tin."

#### Test 2: Password không khớp
- Password: `Password123`
- Confirm Password: `Password456`
- **Kết quả:** Hiển thị lỗi "Mật khẩu xác nhận không khớp."

#### Test 3: Email đã tồn tại
- Đăng ký với email đã có (ví dụ: `admin@dms.com`)
- **Kết quả:** Hiển thị lỗi từ Identity (ví dụ: "Email đã được sử dụng")

#### Test 4: Password quá ngắn
- Password: `123` (dưới 6 ký tự)
- **Kết quả:** Hiển thị lỗi từ Identity

---

## 🚪 Test Đăng Xuất

### Bước 1: Đăng nhập vào hệ thống

### Bước 2: Click nút "Đăng xuất" trên navbar

**Kết quả mong đợi:**
- ✅ Đăng xuất thành công
- ✅ Redirect về trang `/Account/Login`
- ✅ Navbar hiển thị "Đăng ký" và "Đăng nhập"

---

## 👤 Tài Khoản Test

### Tài khoản Admin (Tự động tạo)
```
Email: admin@dms.com
Password: Admin@123
Role: Admin
```

### Tạo thêm tài khoản test
Bạn có thể tạo thêm tài khoản bằng cách:
1. Đăng ký qua form Register
2. Hoặc thêm trực tiếp vào database

---

## 🗄️ Kiểm Tra Database

### Kiểm tra bằng SQL Server Management Studio (SSMS)

1. **Kết nối đến SQL Server:**
   - Server: `LAPTOP-2CVL2A9H\MSSQLSERVER01`
   - Authentication: Windows Authentication

2. **Kiểm tra Database:**
   ```sql
   USE DMS_University_DB;
   GO
   ```

3. **Xem danh sách Users:**
   ```sql
   SELECT Id, UserName, Email, FullName, CreatedDate 
   FROM AspNetUsers;
   ```

4. **Xem danh sách Courses:**
   ```sql
   SELECT * FROM Courses;
   ```

5. **Xem Roles:**
   ```sql
   SELECT * FROM AspNetRoles;
   ```

6. **Xem User Roles:**
   ```sql
   SELECT u.UserName, r.Name AS RoleName
   FROM AspNetUsers u
   INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
   INNER JOIN AspNetRoles r ON ur.RoleId = r.Id;
   ```

---

## 🐛 Troubleshooting

### Lỗi: "Cannot connect to SQL Server"
**Giải pháp:**
- Kiểm tra SQL Server đang chạy
- Kiểm tra instance name đúng: `LAPTOP-2CVL2A9H\MSSQLSERVER01`
- Kiểm tra Windows Authentication

### Lỗi: "Database does not exist"
**Giải pháp:**
- Database sẽ tự động được tạo khi chạy ứng dụng lần đầu
- Kiểm tra migrations đã được apply chưa

### Lỗi: "Invalid login attempt"
**Giải pháp:**
- Kiểm tra email và password đúng
- Kiểm tra user đã được tạo trong database chưa
- Xóa database và chạy lại để seed data tự động tạo user admin

### UI không hiển thị đúng
**Giải pháp:**
- Xóa cache trình duyệt (Ctrl + F5)
- Kiểm tra file CSS đã được load: `~/css/login.css`
- Kiểm tra console browser có lỗi JavaScript không

---

## ✅ Checklist Test Hoàn Chỉnh

### UI/UX
- [ ] Trang đăng nhập hiển thị đúng layout 2 cột
- [ ] Background image hiển thị với blur effect
- [ ] Icon graduation cap hiển thị đúng
- [ ] Tất cả text hiển thị đúng
- [ ] Form inputs hoạt động tốt
- [ ] Password toggle hoạt động
- [ ] Responsive trên mobile

### Chức Năng
- [ ] Đăng nhập thành công với tài khoản admin
- [ ] Đăng nhập thất bại với thông tin sai
- [ ] Validation hoạt động đúng
- [ ] Đăng ký tài khoản mới thành công
- [ ] Đăng xuất hoạt động
- [ ] Remember me hoạt động
- [ ] Redirect sau đăng nhập đúng

### Database
- [ ] Database được tạo tự động
- [ ] Migrations được apply
- [ ] Seed data được tạo (admin user, courses)
- [ ] User mới được lưu vào database

---

## 📞 Hỗ Trợ

Nếu gặp vấn đề, kiểm tra:
1. Logs trong console khi chạy ứng dụng
2. Browser console (F12) để xem lỗi JavaScript
3. SQL Server logs
4. Application logs trong Visual Studio Output window

---

**Chúc bạn test thành công! 🎉**

