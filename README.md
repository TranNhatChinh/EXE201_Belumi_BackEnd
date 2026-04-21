# 🌟 Belumi - Skincare AI Backend

Belumi là một hệ thống Backend hiện đại được xây dựng trên nền tảng **.NET 8** theo kiến trúc **Clean Architecture**. Dự án tích hợp trí tuệ nhân tạo (AI) để cá nhân hóa hành trình chăm sóc da của người dùng.

---

## 🛠️ Yêu cầu hệ thống (Prerequisites)

Hãy đảm bảo bạn đã cài đặt các công cụ sau trước khi bắt đầu:
- **.NET 8.0 SDK** (Link tải: [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0))
- **PostgreSQL** (Phiên bản 14 trở lên)
- **Visual Studio 2022** hoặc **VS Code**
- **EF Core CLI tool**:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## 🚀 Hướng dẫn chạy dự án (Getting Started)

### Bước 1: Clone dự án
Tải mã nguồn về máy của bạn:
```bash
git clone <repository_url>
cd belumi
```

### Bước 2: Cấu hình môi trường (Environment)
Dự án sử dụng tệp `.env` để quản lý các cấu hình bí mật.
1. Copy tệp mẫu:
   ```bash
   cp .env.example .env
   ```
2. Mở tệp `.env` và cập nhật các thông tin sau:
   - **ConnectionStrings__DefaultConnection**: Thông tin kết nối DB PostgreSQL của bạn.
   - **SMTP Configuration**: (Cần thiết để gửi mail xác thực) - Nên dùng *Gmail App Password*.
   - **JwtSettings**: Các key bảo mật cho Token.

### Bước 3: Khởi tạo Cơ sở dữ liệu (Database)
Di chuyển vào thư mục API và chạy lệnh cập nhật database:
```bash
cd src/YourApp.API
dotnet ef database update 
```

### Bước 4: Chạy ứng dụng
Khởi động dự án:
```bash
dotnet run
```
Sau khi chạy thành công, bạn có thể truy cập:
- **Swagger UI**: `https://localhost:xxxx/swagger` (Kiểm tra cổng hiển thị trong Terminal)
- **API Base**: `https://localhost:xxxx/api`

---

## 🏗️ Kiến trúc & Công nghệ (Tech Stack)

Dự án tuân thủ nghiêm ngặt các nguyên tắc phần mềm hiện đại:
- **Clean Architecture**: Tách biệt rõ ràng các tầng Domain, Application, Infrastructure và API.
- **Vertical Slice Architecture**: Tổ chức code theo tính năng (Features).
- **CQRS**: Sử dụng thư viện **MediatR**.
- **Validation**: **FluentValidation**.
- **Mapping**: **Riok.Mapperly** (Hiệu suất cao hơn AutoMapper).
- **Email Service**: **MailKit** tích hợp **Scriban Template Engine** (HTML Email chuyên nghiệp).
- **Authentication**: **JWT** với cơ chế xác thực Email.

---

## 📂 Cấu trúc thư mục nền tảng
- `src/YourApp.Domain`: Thực thể, Enums, Logic cốt lõi.
- `src/YourApp.Application`: DTOs, Mappers, Handlers (Logic nghiệp vụ).
- `src/YourApp.Infrastructure`: Persistence (EF Core), Mail Services, Security.
- `src/YourApp.API`: Controllers/Endpoints, Middleware, Cấu hình Host.

---

## 🤝 Đóng góp
Nếu bạn gặp vấn đề hoặc muốn đóng góp tính năng, vui lòng tạo Issue hoặc gửi Pull Request.

**© 2025 Belumi Team.**
