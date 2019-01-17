using System;

using ForcedLogInApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace ForcedLogInApp.Views
{
    public sealed partial class LogInPage : Page
    {
        public LogInViewModel ViewModel { get; } = new LogInViewModel();

        public LogInPage()
        {
            InitializeComponent();
        }
    }
}
