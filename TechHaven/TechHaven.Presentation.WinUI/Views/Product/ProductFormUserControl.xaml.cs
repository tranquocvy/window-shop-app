using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Views.Controls
{
    public sealed partial class ProductFormUserControl : UserControl
    {
        public ProductFormUserControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Đổ dữ liệu từ Product có sẵn vào Form (dùng cho Edit)
        /// </summary>
        public void LoadData(ProductDto product)
        {
            if (product == null) return;

            // Populate fields dựa trên code bạn cung cấp
            NameBox.Text = product.ProductName ?? string.Empty;
            PriceBox.Text = product.SellPrice.ToString();
            StockBox.Text = product.StockQuantity.ToString();
            BrandBox.Text = product.BrandName ?? string.Empty;
            DescBox.Text = product.Description ?? string.Empty;
        }

        /// <summary>
        /// Validate và lấy dữ liệu.
        /// Trả về DTO nếu hợp lệ, trả về null nếu lỗi (và tự hiện dialog báo lỗi).
        /// </summary>
        public ProductCreateUpdateDto GetFormData()
        {
            // 1. Validation: Tên sản phẩm
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                ShowError("Thiếu thông tin", "Tên sản phẩm không được để trống.");
                return null;
            }

            // 2. Validation: Giá
            if (!decimal.TryParse(PriceBox.Text, out var price))
            {
                ShowError("Giá không hợp lệ", "Vui lòng nhập giá hợp lệ.");
                return null;
            }

            // 3. Validation: Số lượng tồn
            if (!int.TryParse(StockBox.Text, out var stock))
            {
                ShowError("Số lượng không hợp lệ", "Vui lòng nhập số nguyên cho số lượng.");
                return null;
            }

            // 4. Tạo DTO trả về (nếu tất cả OK)
            return new ProductCreateUpdateDto
            {
                ProductName = NameBox.Text.Trim(),
                SellPrice = price,
                StockQuantity = stock,
                BrandName = string.IsNullOrWhiteSpace(BrandBox.Text) ? null : BrandBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescBox.Text) ? null : DescBox.Text.Trim()
            };
        }

        /// <summary>
        /// Hàm helper để hiển thị lỗi ngay trên giao diện hiện tại
        /// </summary>
        private void ShowError(string title, string content)
        {
            var err = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Đóng",
                // Quan trọng: Phải gán XamlRoot của UserControl để Dialog hiện được
                XamlRoot = this.XamlRoot
            };
            _ = err.ShowAsync();
        }
    }
}