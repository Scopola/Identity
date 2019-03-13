using OptionalLoginApp.Helpers;
using OptionalLoginApp.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace OptionalLoginApp.Views
{
    [Restricted]
    public sealed partial class GraphSamplePage : Page
    {
        public GraphSampleViewModel ViewModel { get; } = new GraphSampleViewModel();

        public GraphSamplePage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadDataAsync();
        }
    }
}
