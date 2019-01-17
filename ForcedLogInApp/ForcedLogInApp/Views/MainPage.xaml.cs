using System;

using ForcedLogInApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace ForcedLogInApp.Views
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
