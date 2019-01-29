using System;

using Microsoft.UI.Xaml.Controls;

using Windows.UI.Xaml;

namespace OptionalLoginApp.Helpers
{
    public class NavHelper
    {
        public static Type GetNavigateTo(NavigationViewItem item)
        {
            return (Type)item.GetValue(NavigateToProperty);
        }

        public static void SetNavigateTo(NavigationViewItem item, Type value)
        {
            item.SetValue(NavigateToProperty, value);
        }

        public static bool GetRestricted(NavigationViewItem item)
        {
            return (bool)item.GetValue(RestrictedProperty);
        }

        public static void SetRestricted(NavigationViewItem item, bool value)
        {
            item.SetValue(RestrictedProperty, value);
        }

        public static readonly DependencyProperty NavigateToProperty =
            DependencyProperty.RegisterAttached("NavigateTo", typeof(Type), typeof(NavHelper), new PropertyMetadata(null));

        public static readonly DependencyProperty RestrictedProperty =
            DependencyProperty.RegisterAttached("Restricted", typeof(bool), typeof(NavHelper), new PropertyMetadata(false));
    }
}
