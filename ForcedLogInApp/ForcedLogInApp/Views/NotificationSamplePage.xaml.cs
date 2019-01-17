using System;

using ForcedLogInApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace ForcedLogInApp.Views
{
    public sealed partial class NotificationSamplePage : Page
    {
        public NotificationSampleViewModel ViewModel { get; } = new NotificationSampleViewModel();

        public NotificationSamplePage()
        {
            InitializeComponent();
        }
    }
}
