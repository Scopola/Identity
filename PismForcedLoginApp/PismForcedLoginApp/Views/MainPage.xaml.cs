using System;

using PismForcedLoginApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace PismForcedLoginApp.Views
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
