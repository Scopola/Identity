using System;

using PrismForcedLoginApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace PrismForcedLoginApp.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
