using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TechHaven.Presentation.WinUI.ViewModel;
using System.Linq;

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
            if (ViewModel.ChartTabs.Any())
            {
                ViewModel.SelectedChartTab = ViewModel.ChartTabs.First();
            }
        }

        private void RevenueChartTab_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ChartTabs.Count > 1)
            {
                ViewModel.SelectedChartTab = ViewModel.ChartTabs[1];
            }
        }

        private void CommissionTab_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedChartTab = "Hoa H?ng";
        }
    }
}
