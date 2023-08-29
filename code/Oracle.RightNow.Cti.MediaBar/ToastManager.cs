using Oracle.RightNow.Cti.MediaBar.ViewModels;
using Oracle.RightNow.Cti.MediaBar.Views;
using Oracle.RightNow.Cti.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Oracle.RightNow.Cti.MediaBar {
    public class ToastManager {
        private static List<ToastView> _openToasts = new List<ToastView>();
        private static System.Threading.Timer _timer;
        private static SynchronizationContext _syncrhronizationContext;

        public static void Initialize(IInteractionProvider provider) {
            provider.NewInteraction += newInteraction;
            _timer = new System.Threading.Timer(timerTick, null, Timeout.Infinite, Timeout.Infinite);
            _syncrhronizationContext = SynchronizationContext.Current;
        }

        private static void timerTick(object state) {
            var now = DateTime.Now;
            foreach (var item in _openToasts) {
                if (item.Model.Inactive && item.Model.LastActivity < now) {
                    _syncrhronizationContext.Post(o => item.Close(), null);
                }
            }
        }

        private static void newInteraction(object sender, InteractionEventArgs e) {
            _syncrhronizationContext.Post(o => showToast((IInteraction)o), e.Interaction);
        }

        private static void showToast(IInteraction interaction) {
            IntPtr windowHandle = ConsoleWindowHelper.GetMainWindowHandle();
            var cxScreen = Screen.FromHandle(windowHandle);
            var toast = new ToastView {
                Model = new ToastViewModel {
                    Interaction = interaction,
                    LastActivity = DateTime.Now.AddSeconds(5),
                    Inactive = true
                }
            };

            interaction.StateChanged += interaction_StateChanged;

            toast.Left = (cxScreen.WorkingArea.X + cxScreen.WorkingArea.Width) - (toast.Width + 1);
            toast.Top = (cxScreen.WorkingArea.Y + cxScreen.WorkingArea.Height) - (toast.Height + 1) - (_openToasts.Count * toast.Height);
            toast.MouseEnter += toast_MouseEnter;
            toast.MouseLeave += toast_MouseLeave;
            toast.Closed += toast_Closed;
            _openToasts.Add(toast);
            toast.Show();
            if (_openToasts.Count == 1) {
                _timer.Change(5000, 1000);
            }
        }

        static void interaction_StateChanged(object sender, InteractionStateChangedEventArgs e) {
            var interation = (IInteraction)sender;
            interation.StateChanged -= interaction_StateChanged;

            var view = _openToasts.FirstOrDefault(v => v.Model.Interaction.Id == interation.Id);
            
            if (view != null) {
                _openToasts.Remove(view);
                _syncrhronizationContext.Post(o => view.Close(), null);
            }
        }

        static void toast_Closed(object sender, EventArgs e) {
            var view = ((ToastView)sender);

            _openToasts.Remove(view);
            view.Closed -= toast_Closed;
            view.MouseLeave -= toast_MouseLeave;
            view.MouseEnter -= toast_MouseEnter;
            if (_openToasts.Count == 0) {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        static void toast_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            var view = ((ToastView)sender);
            view.Model.LastActivity = DateTime.Now.AddSeconds(3);
            view.Model.Inactive = true;
        }

        static void toast_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            ((ToastView)sender).Model.Inactive = false;    
        }
    }
}