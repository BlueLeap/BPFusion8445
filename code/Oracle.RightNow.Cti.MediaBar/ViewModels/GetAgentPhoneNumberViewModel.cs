using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.RightNow.Cti.Model;
using System.Windows.Input;
using Oracle.RightNow.Cti.MediaBar.Views;
using Oracle.RightNow.Cti.AddIn;
using Microsoft.Windows.Controls;
using Oracle.RightNow.Cti.MediaBar.Helpers;

namespace Oracle.RightNow.Cti.MediaBar.ViewModels
{
    public class GetAgentPhoneNumberViewModel : ViewModel
    {
        public string Caption { get; set; }
        private readonly Action<bool> _resultHandler;
        private string _destinationNumber;

        public ICommand AcceptCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ICommand DialCommand { get; set; }
        public string DestinationNumber
        {
            get
            {
                return _destinationNumber;
            }
            set
            {
                _destinationNumber = value;
                OnPropertyChanged("DestinationNumber");
            }
        }


        public GetAgentPhoneNumberViewModel(Action<bool> resultHandler, string destNumber,string caption = "Change Agent Number")
        {
            _resultHandler = resultHandler;
            initializeCommands();
            Caption = caption;
            DestinationNumber = destNumber;
            if(DestinationNumber == null)
            {
                DestinationNumber = "";
            }
        }

        private void initializeCommands()
        {
            AcceptCommand = new DelegateCommand(accept);
            CancelCommand = new DelegateCommand(cancel);
            DialCommand = new DelegateCommand(dial);
        }

        private void cancel(object obj)
        {
            _resultHandler(false);
        }

        private void accept(object obj)
        {
            _resultHandler(true);
        }

        private void dial(object obj)
        {
            var dialog = new TransferDialog();

            ConsoleWindowHelper.SetupOwner(dialog);
            dialog.DataContext = new TransferDialogViewModel(null, (result, contact) => {
                try
                {
                    if (result)
                    {
                        DestinationNumber = contact.Number;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Messages.MessagePerformOutboundDial, Messages.MessageBoxTitle);
                    Logger.Logger.Log.Error("showDialPad Exception :::::: ", ex);
                }
                dialog.Close();
            }, "Dial", true, MediaBarAddIn._LockInternationalCalls);

            dialog.ShowDialog();
        }
    }
}
