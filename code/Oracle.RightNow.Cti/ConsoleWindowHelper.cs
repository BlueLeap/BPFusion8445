using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Oracle.RightNow.Cti {
    public static class ConsoleWindowHelper {
        public static void SetupOwner(Window window) {
            if (System.Threading.SynchronizationContext.Current is DispatcherSynchronizationContext) {
                window.Owner = System.Windows.Application.Current.MainWindow;
            }
            else if (System.Windows.Forms.Application.OpenForms.Count > 0) {
                WindowInteropHelper helper = new WindowInteropHelper(window);
                helper.Owner = System.Windows.Forms.Application.OpenForms[0].Handle;
            }
        }

        public static IntPtr GetMainWindowHandle() {
            IntPtr result = IntPtr.Zero;
            if (System.Windows.Application.Current.MainWindow != null &&
                string.Compare(System.Windows.Application.Current.MainWindow.GetType().Assembly.GetName().Name, "RightNow.Client.Presentation") == 0) {
                WindowInteropHelper helper = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
                result = helper.Handle;
            }
            else if (System.Windows.Forms.Application.OpenForms.Count > 0) {
                result = System.Windows.Forms.Application.OpenForms[0].Handle;
            }

            return result;
        }
    }
}