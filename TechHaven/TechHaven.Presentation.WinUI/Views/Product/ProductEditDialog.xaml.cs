using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class ProductEditDialog : ContentDialog
    {
        // Sau khi nhấn Lưu, caller sẽ đọc EditedProduct
        public ProductCreateUpdateDto EditedProduct { get; private set; }

        private readonly ProductDto _original;

        public ProductEditDialog(ProductDto original)
        {
            this.InitializeComponent();
            _original = original ?? throw new ArgumentNullException(nameof(original));

            // Populate fields
            NameBox.Text = _original.ProductName ?? string.Empty;
            PriceBox.Text = _original.SellPrice.ToString();
            StockBox.Text = _original.StockQuantity.ToString();
            BrandBox.Text = _original.BrandName ?? string.Empty;
            DescBox.Text = _original.Description ?? string.Empty;
        }

        // Bắt sự kiện PrimaryButtonClick để validate và gán EditedProduct
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Validation cơ bản
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                args.Cancel = true;
                var err = new ContentDialog
                {
                    Title = "Thiếu thông tin",
                    Content = "Tên sản phẩm không được để trống.",
                    CloseButtonText = "Đóng"
                };
                err.XamlRoot = this.XamlRoot;
                _ = err.ShowAsync();
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out var price))
            {
                args.Cancel = true;
                var err = new ContentDialog
                {
                    Title = "Giá không hợp lệ",
                    Content = "Vui lòng nhập giá hợp lệ.",
                    CloseButtonText = "Đóng"
                };
                err.XamlRoot = this.XamlRoot;
                _ = err.ShowAsync();
                return;
            }

            if (!int.TryParse(StockBox.Text, out var stock))
            {
                args.Cancel = true;
                var err = new ContentDialog
                {
                    Title = "Số lượng không hợp lệ",
                    Content = "Vui lòng nhập số nguyên cho số lượng.",
                    CloseButtonText = "Đóng"
                };
                err.XamlRoot = this.XamlRoot;
                _ = err.ShowAsync();
                return;
            }

            // Tạo DTO trả về caller
            EditedProduct = new ProductCreateUpdateDto
            {
                ProductName = NameBox.Text.Trim(),
                SellPrice = price,
                StockQuantity = stock,
                BrandName = string.IsNullOrWhiteSpace(BrandBox.Text) ? null : BrandBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescBox.Text) ? null : DescBox.Text.Trim()
                // Nếu DTO của bạn có thêm trường, gán thêm ở đây
            };

            // Không set args.Cancel => dialog sẽ đóng và trả ContentDialogResult.Primary cho caller
        }

        // Optional: gán handler trong constructor nếu bạn thích code thay vì XAML
        // public ProductEditDialog(ProductDto original) { InitializeComponent(); this.PrimaryButtonClick += ContentDialog_PrimaryButtonClick; ... }
    }
}
