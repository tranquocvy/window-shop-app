using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TechHaven.Presentation.WinUI.ViewModel;

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// Dashboard page displaying business metrics and charts
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        #region Fields

        private readonly DashboardViewModel _viewModel;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of DashboardPage
        /// </summary>
        public DashboardPage()
        {
            InitializeComponent();
            _viewModel = new DashboardViewModel();
            DataContext = _viewModel;
            
            Loaded += DashboardPage_Loaded;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles page loaded event
        /// </summary>
        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= DashboardPage_Loaded;
            await _viewModel.LoadAsync();
        }

        #endregion
    }
}
