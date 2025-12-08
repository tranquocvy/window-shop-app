using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT.Interop;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class WindowHelper
    {
        public static Window GetWindowForElement(FrameworkElement element)
        {
            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);

            // Tìm window chứa element
            object? current = element;
            while (current != null)
            {
                if (current is Window w)
                    return w;

                if (current is FrameworkElement fe)
                    current = fe.Parent;
                else
                    break;
            }

            return App.MainWindow;
        }
    }
}
