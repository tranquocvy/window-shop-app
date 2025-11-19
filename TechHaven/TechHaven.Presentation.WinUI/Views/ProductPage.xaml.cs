using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Linq;

namespace TechHaven.Presentation.WinUI.Views
{
    // LƯU Ý QUAN TRỌNG: Tên lớp phải là 'partial' và khớp chính xác với x:Class trong XAML
    public sealed partial class ProductPage : Page
    {
        // Khai báo một danh sách sản phẩm mẫu để gán vào ListView nếu cần
        // public ObservableCollection<Product> Products { get; set; } 

        public ProductPage()
        {
            // Hàm tạo này là cần thiết và phải được định nghĩa DUY NHẤT 1 lần.
            this.InitializeComponent();

            // Nếu bạn sử dụng MVVM, bạn sẽ gán ViewModel ở đây
            // this.DataContext = new ProductViewModel(); 

            // Ví dụ về việc tải dữ liệu tĩnh nếu không dùng Data Binding
            // LoadStaticData(); 
        }

        // -------------------------------------------------------------------
        // PHƯƠNG THỨC XỬ LÝ SỰ KIỆN TỪ XAML
        // -------------------------------------------------------------------

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút "Thêm Sản Phẩm"
        /// </summary>
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Viết logic để mở cửa sổ/dialog thêm sản phẩm mới

            // Ví dụ: Hiện thông báo đơn giản
            // (Bạn cần có một TextBlock hoặc thông báo trong UI để hiển thị)
            // System.Diagnostics.Debug.WriteLine("Nút Thêm Sản Phẩm đã được nhấn!");
        }

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào một mục (Item) trong ProductsList
        /// </summary>
        private void ProductsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            // TODO: Viết logic để điều hướng đến trang chi tiết sản phẩm hoặc mở dialog chỉnh sửa

            // Lấy dữ liệu của Item được click
            // var selectedProduct = e.ClickedItem as Product; 

            // if (selectedProduct != null)
            // {
            //     System.Diagnostics.Debug.WriteLine($"Sản phẩm được chọn: {selectedProduct.Name}");
            //     // Ví dụ: Navigation.Frame.Navigate(typeof(ProductDetailPage), selectedProduct);
            // }
        }

        // -------------------------------------------------------------------
        // CÁC PHƯƠNG THỨC HỖ TRỢ KHÁC (Tùy chọn)
        // -------------------------------------------------------------------

        // private void LoadStaticData()
        // {
        //     // Logic để tạo dữ liệu tĩnh và gán vào ListView (nếu không dùng ItemTemplate tĩnh)
        // }
    }
}