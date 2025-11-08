# Window Shop App

Kho lưu trữ **mã nguồn dự án WinUI 3 - Ứng dụng bán hàng cho cửa hàng nhỏ**.
Repo này bao gồm toàn bộ code của ứng dụng: UI (WinUI 3), business logic, data access, repository/service và unit test.

Mục tiêu:

- Lưu trữ và phát triển mã nguồn chính
- Tổ chức kiến trúc theo mô hình **MVVM + Dependency Injection**
- Làm việc theo quy trình **Gitflow** để FE/BE phát triển song song

---

## Cấu trúc thư mục

```

```

---

## Chiến lược làm việc với Git

Repo áp dụng **Gitflow cơ bản** nhằm giữ `main` ổn định, trong khi `develop` là nơi tích hợp các tính năng FE/BE.

### 1. Nhánh chính

- **main**

  - Code ổn định, dùng để demo/release
  - Chỉ merge từ `develop`

- **develop**

  - Nhánh tích hợp chung
  - Mọi feature đều xuất phát từ đây

### 2. Feature branches

Mỗi tính năng tạo **một nhánh riêng** từ `develop`:

```
feature/login-frontend
feature/product-backend
feature/customer-frontend
feature/order-backend
```

Lợi ích:

- FE và BE làm song song
- Ít xung đột
- Dễ review và theo dõi phạm vi thay đổi

### 3. Quy trình làm việc

1. Lead tạo `develop` từ `main`
2. Dev tạo `feature/*` từ `develop`
3. Commit & push thường xuyên
4. Hoàn thành → Tạo **Pull Request** về `develop`
5. Lead/team review → yêu cầu chỉnh sửa nếu cần
6. Review đạt → merge vào `develop`
7. Khi `develop` ổn định → merge vào `main`

---

## Quy tắc đặt tên branch

### Feature branch

```
feature/<module>-<frontend|backend>
```

Ví dụ:

- `feature/login-frontend`
- `feature/product-backend`

### Bugfix branch

```
bugfix/<mô-tả-ngắn>
```

---

## Quy tắc đặt tên commit

### Prefix chuẩn

| Prefix      | Ý nghĩa                                  |
| ----------- | ---------------------------------------- |
| `add:`      | thêm file/tính năng mới                  |
| `update:`   | cập nhật logic hiện có                   |
| `fix:`      | sửa lỗi                                  |
| `refactor:` | chỉnh lại cấu trúc code, không đổi logic |
| `remove:`   | xóa code/file                            |
| `style:`    | format, rename, chỉnh style              |
| `test:`     | thêm/chỉnh unit test                     |

### Ví dụ commit tốt

```bash
add: create product viewmodel
fix: handle null customer email
refactor: move repository to Core project
style: format order service
```

---

## Quy tắc tạo Pull Request (PR)

### 1. Mỗi PR xử lý **một nhiệm vụ cụ thể**

Tốt: “Thêm ProductPage UI”

Không tốt: “Thêm UI + sửa random bug + refactor backend”

### 2. Tiêu đề PR

Theo format:

```
[feature] <tên tính năng>
[bugfix] <mô tả lỗi>
[refactor] <mô tả>
```

### 3. Nội dung PR cần có

```
### Nội dung
- Thêm ProductPage
- Bind dữ liệu từ ProductViewModel
- Đăng ký IProductRepository vào DI

### Testing
- Build pass
- Điều hướng ProductPage
- CRUD mock data
```

### 4. Reviewer kiểm tra

- Code style
- Logic đúng MVVM
- Không xung đột
- Kiểm tra exception & edge case

### 5. Merge PR

- Chỉ merge khi **Approved**
- Không conflict
- Build OK

---

## Tài liệu dự án

Tất cả tài liệu kỹ thuật được lưu trong: [**Window Shop Docs**](https://github.com/tranquocvy/window-shop-docs)
