using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TechHaven.Presentation.WinUI.Views;

namespace TechHaven.Presentation.WinUI.Services.Onboarding
{
    public interface IOnboardingService
    {
        Task RunAsync(ShellWindow shell);
    }

    public class OnboardingService : IOnboardingService
    {
        public async Task RunAsync(ShellWindow shell)
        {
            // Dashboard intro
            await ShowTipAsync(shell, typeof(DashboardPage), null,
                "Bảng điều khiển",
                "Tổng quan doanh thu, đơn hàng và thông tin nhanh.");

            // Product intro
            await ShowTipAsync(shell, typeof(ProductPage), null,
                "Sản phẩm",
                "Quản lý danh sách, tồn kho và chi tiết sản phẩm.");

            // Product: search box
            var productRoot = shell.GetCurrentPageRoot();
            var searchBox = (productRoot as FrameworkElement)?.FindName("SearchBox") as FrameworkElement;
            await ShowTipAsync(shell, null, searchBox,
                "Tìm kiếm",
                "Nhập tên để tìm nhanh sản phẩm.");

            // Product: add button
            var addBtn = (productRoot as FrameworkElement)?.FindName("AddButton") as FrameworkElement;
            await ShowTipAsync(shell, null, addBtn,
                "Thêm sản phẩm",
                "Tạo sản phẩm mới từ đây.");

            // Order intro
            await ShowTipAsync(shell, typeof(OrderPage), null,
                "Đơn hàng",
                "Xem, lọc và xử lý đơn hàng.");

            // Report intro
            await ShowTipAsync(shell, typeof(ReportPage), null,
                "Báo cáo",
                "Xem biểu đồ và xuất báo cáo.");

            var reportRoot = shell.GetCurrentPageRoot();
            // Report: export button (desktop/tablet)
            var exportBtn = (reportRoot as FrameworkElement)?.FindName("ExportButton") as FrameworkElement
                             ?? (reportRoot as FrameworkElement)?.FindName("TabletExportButton") as FrameworkElement;
            await ShowTipAsync(shell, null, exportBtn,
                "Xuất báo cáo",
                "Nhấn để xuất báo cáo theo bộ lọc hiện tại.");

            // Report: period combo (desktop/tablet)
            var periodCombo = (reportRoot as FrameworkElement)?.FindName("PeriodCombo") as FrameworkElement
                              ?? (reportRoot as FrameworkElement)?.FindName("TabletPeriodCombo") as FrameworkElement;
            await ShowTipAsync(shell, null, periodCombo,
                "Theo kỳ",
                "Chọn Ngày/Tháng/Năm để đổi kỳ báo cáo.");

            // Report: search button/tablet view button
            var tabletSearch = (reportRoot as FrameworkElement)?.FindName("TabletSearchButton") as FrameworkElement;
            await ShowTipAsync(shell, null, tabletSearch,
                "Xem báo cáo",
                "Nhấn để xem dữ liệu theo bộ lọc đã chọn.");

            // Setting intro
            await ShowTipAsync(shell, typeof(SettingPage), null,
                "Cài đặt",
                "Cấu hình server, chủ đề và tuỳ chọn.");
        }

        private static async Task ShowTipAsync(ShellWindow shell, Type? pageType, FrameworkElement? target, string title, string subtitle)
        {
            try
            {
                if (pageType != null)
                {
                    shell.NavigateTo(pageType);
                }
            }
            catch { }

            var root = shell.GetCurrentPageRoot();
            if (root == null) return;

            var tip = new TeachingTip
            {
                Title = title,
                Subtitle = subtitle,
                PreferredPlacement = TeachingTipPlacementMode.Bottom,
                IsOpen = false
            };

            if (target != null)
            {
                tip.Target = target;
            }
            else
            {
                tip.ActionButtonContent = "Tiếp";
            }

            if (root is Panel p)
            {
                p.Children.Add(tip);
            }
            else if (root is Grid g)
            {
                g.Children.Add(tip);
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = subtitle,
                    PrimaryButtonText = "Tiếp",
                    XamlRoot = (shell.Content as FrameworkElement)?.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            tip.IsOpen = true;

            var tcs = new TaskCompletionSource();
            tip.ActionButtonClick += (s, e) => { tip.IsOpen = false; tcs.SetResult(); };
            tip.Closed += (s, e) => { tcs.SetResult(); };

            await tcs.Task;

            try
            {
                if (root is Panel p2)
                    p2.Children.Remove(tip);
                else if (root is Grid g2)
                    g2.Children.Remove(tip);
            }
            catch { }
        }
    }
}
