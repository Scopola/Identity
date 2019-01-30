using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace OptionalLoginApp.Helpers
{
    public static class FrameExtensions
    {
        public static void RemoveFromBackStack(this Frame frame, IEnumerable<Type> pagesTypeToRemove)
        {
            foreach (var backStack in frame.BackStack)
            {
                if (pagesTypeToRemove.Any(type => type == backStack.SourcePageType))
                {
                    frame.BackStack.Remove(backStack);
                }
            }
        }
    }
}
