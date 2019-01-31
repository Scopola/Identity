using Microsoft.UI.Xaml.Controls;

using Windows.UI.Xaml;

namespace OptionalLoginApp.Helpers
{
    public class AuthHelper
    {
        public static bool GetRestricted(NavigationViewItem item)
        {
            return (bool)item.GetValue(RestrictedProperty);
        }

        public static void SetRestricted(NavigationViewItem item, bool value)
        {
            item.SetValue(RestrictedProperty, value);
        }

        public static readonly DependencyProperty RestrictedProperty =
            DependencyProperty.RegisterAttached("Restricted", typeof(bool), typeof(NavHelper), new PropertyMetadata(false));
    }
}
