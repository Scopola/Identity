using System;

using PrismForcedLoginApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace PrismForcedLoginApp.Views
{
    public sealed partial class GraphSamplePage : Page
    {
        private GraphSampleViewModel ViewModel => DataContext as GraphSampleViewModel;

        public GraphSamplePage()
        {
            InitializeComponent();
        }
    }
}
