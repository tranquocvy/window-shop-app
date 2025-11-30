using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TechHaven.Presentation.WinUI.ViewModel;

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class ReportPage : Page
    {
        public ReportViewModel ViewModel { get; }

        public ReportPage()
        {
            this.InitializeComponent();
            ViewModel = new ReportViewModel();
            this.DataContext = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Load initial data when page is navigated to
            await ViewModel.LoadProductsCommand.ExecuteAsync(null);
            await ViewModel.LoadReportsCommand.ExecuteAsync(null);
        }

        private void ProductChartTab_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedChartTab = "S?n Ph?m";
        }

        private void RevenueChartTab_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedChartTab = "Doanh Thu";
        }
    }
}
