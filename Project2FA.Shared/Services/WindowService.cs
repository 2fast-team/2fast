#if WINDOWS_UWP
using System;
using System.Collections.Generic;
using System.Linq;
using UNOversal;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Project2FA.Services
{
    
    public static class WindowService
    {
        public static Dictionary<Guid, Action<WindowCreatedEventArgs>> WindowCreatedCallBacks { get; } = new Dictionary<Guid, Action<WindowCreatedEventArgs>>();

        private static readonly List<Window> _instances = new List<Window>();

        public static void Register(Window window)
        {
            if (!_instances.Contains(window))
            {
                _instances.Add(window);
                window.Closed += Window_Closed;
            }
        }

        private static void Window_Closed(object sender, CoreWindowEventArgs e)
        {
            UnRegister(sender as Window);
        }

        private static void UnRegister(Window window)
        {
            window.Closed -= Window_Closed;
            _instances.Remove(window);
        }

        public static Window[] AllWindows => _instances.ToArray();

        public static bool TryGetWindow(FrameworkElement element, out Window window)
        {
            foreach (Window item in AllWindows)
            {
                if (item.Content is UIElement ui)
                {
                    if (ui == element)
                    {
                        window = item;
                        return true;
                    }
                    List<FrameworkElement> children = ui.VisualChildren();
                    if (children.Contains(element))
                    {
                        window = item;
                        return true;
                    }
                }
            }
            window = null;
            return false;
        }

        internal static void ForwardWindowCreated(WindowCreatedEventArgs args)
        {
            GestureService.SetupWindowListeners(args.Window.CoreWindow);
            foreach (KeyValuePair<Guid, Action<WindowCreatedEventArgs>> item in WindowCreatedCallBacks.ToArray())
            {
                item.Value?.Invoke(args);
            }
            _instances.Add(args.Window);
        }
    }
}
#endif
