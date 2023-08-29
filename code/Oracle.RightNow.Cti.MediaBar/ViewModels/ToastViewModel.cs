using System.Threading.Tasks;
using Oracle.RightNow.Cti.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Oracle.RightNow.Cti.MediaBar.ViewModels {
    public class ToastViewModel : ViewModel {
        public ToastViewModel() {
            initializeCommands();
        }
        public IInteraction Interaction { get; set; }
        public DateTime LastActivity { get; set; }

        public bool Inactive { get; set; }

        public ICommand AcceptCommand { get; set; }

        private void initializeCommands() {
            AcceptCommand = new DelegateCommand(o => accept());
        }

        private void accept() {
            Task.Factory.StartNew(() => {
                if (Interaction != null && Interaction.State == InteractionState.Ringing) {
                    Interaction.Accept();
                }
            });            
        }
    }
}
