using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TechHaven.Presentation.WinUI.ViewModel;
using Microsoft.UI.Dispatching;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class DashboardPage : Page
    {
        private readonly DashboardViewModel _vm = new();

        public DashboardPage()
        {
            InitializeComponent();
            this.DataContext = _vm;
            this.Loaded += DashboardPage_Loaded;
            MonthlyChartBorder.SizeChanged += MonthlyChartBorder_SizeChanged;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= DashboardPage_Loaded;
            await _vm.LoadAsync();

            // Enqueue geometry update after layout pass to ensure ActualWidth/Height are valid
            var dq = DispatcherQueue.GetForCurrentThread();
            dq?.TryEnqueue(() => _vm.UpdateMonthlyRevenueGeometry(MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight));
        }

        private void MonthlyChartBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _vm.UpdateMonthlyRevenueGeometry(MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight);
        }
    }
}
