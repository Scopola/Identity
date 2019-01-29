using System;

using OptionalLoginApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace OptionalLoginApp.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
