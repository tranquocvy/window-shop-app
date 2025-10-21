# 🪟 Window Shop App

Ứng dụng hỗ trợ chủ cửa hàng nhỏ trong việc quản lý **sản phẩm, đơn hàng, khách hàng và báo cáo doanh thu**, được phát triển bằng **WinUI 3** và **C#**.

---

## 🚀 Tính năng chính

- 🛍️ Quản lý sản phẩm, danh mục và tồn kho
- 💰 Tạo và theo dõi đơn hàng bán
- 👥 Quản lý khách hàng và nhân viên
- 📊 Tạo báo cáo và thống kê doanh thu
- 🧩 Kiến trúc module theo mô hình **MVVM + Dependency Injection**
- 💾 Cơ sở dữ liệu cục bộ, hỗ trợ **backup / restore**
- 🔐 Phân quyền truy cập (Admin, Sales, Moderator)

---

## 🧱 Cấu trúc dự án

```bash
window-shop-app/
├── src/
│   ├── WindowShop.App/            # Ứng dụng WinUI chính
│   ├── WindowShop.Core/           # Business logic & data models
│   ├── WindowShop.Infrastructure/ # Database, repository, service
│   └── WindowShop.Tests/          # Unit tests
│
├── assets/                        # Hình ảnh, icon, mock data
└── README.md
```

---

## 🧩 Công nghệ sử dụng

| Thành phần           | Công nghệ                                |
| -------------------- | ---------------------------------------- |
| Giao diện            | WinUI 3                                  |
| Logic                | C#, MVVM Toolkit                         |
| Cơ sở dữ liệu        | SQL                                      |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |
| Testing              | MSTest / xUnit                           |

---

## ⚙️ Hướng dẫn cài đặt và chạy

### 1️⃣ Yêu cầu môi trường

- Visual Studio 2022 (đã cài workload **.NET Desktop Development**)
- .NET 8 SDK
- Windows App SDK (>= 1.5)

### 2️⃣ Clone dự án

```bash
git clone https://github.com/tranquocvy/window-shop-app.git
cd window-shop-app
```

### 3️⃣ Build và chạy

Mở solution bằng **Visual Studio** → nhấn **F5** để chạy.

---

## 🧭 Chiến lược nhánh (Branching Strategy)

| Nhánh           | Mục đích                        |
| --------------- | ------------------------------- |
| `main`          | Phiên bản ổn định, dùng để demo |
| `vuong, hau`    | Nhánh phát triển FE             |
| `duong, hoang*` | Nhánh phát triển BE             |

---

## 👥 Thành viên nhóm

| Họ tên    | Vai trò                      |
| --------- | ---------------------------- |
| **Vỹ**    | Team Lead / Middle Developer |
| **Vượng** | Frontend Lead                |
| **Hậu**   | Frontend Developer           |
| **Hoàng** | Backend Lead                 |
| **Dương** | Backend Developer            |

---

## 🗂️ Repository liên quan

📘 [Window Shop Docs](https://github.com/tranquocvy/window-shop-docs)
Lưu trữ tài liệu, biên bản họp và báo cáo tiến độ của nhóm.

---

> 📝 _Đây là đồ án môn Lập trình Windows, thuộc chương trình đào tạo ngành Kỹ thuật phần mềm._
