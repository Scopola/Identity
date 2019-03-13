using System;
using OptionalLoginApp.Helpers;
using OptionalLoginApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace OptionalLoginApp.Views
{
    [Restricted]
    public sealed partial class RestrictedContentPage : Page
    {
        public RestrictedContentViewModel ViewModel { get; } = new RestrictedContentViewModel();

        public RestrictedContentPage()
        {
            InitializeComponent();
        }
    }
}
