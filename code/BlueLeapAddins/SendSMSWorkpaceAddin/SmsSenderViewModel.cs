using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Runtime.Serialization;
using Oracle.RightNow.Cti.Model;
using Oracle.RightNow.Cti;
using BlueLeapAddins.SMSAddin;
using System.Windows;
using BlueLeap.Helpers;
using Oracle.RightNow.Cti.Logger;

namespace BlueLeap.Addins.SMS
{
    [DataContract]
    public abstract class BLNotifyingObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class BLDelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged;

        public BLDelegateCommand(Action<object> execute) : this(execute, null)
        {
        }

        public BLDelegateCommand(Action<object> execute,
            Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
    public class SmsSenderViewModel : BLNotifyingObject
    {
        private Contact _selectedContact;
        private readonly Action<bool, Contact> _resultHandler;
        private string _userInput;
        public string Caption { get; set; }
        private string _destinationNumber;
        private bool _IsReadOnly;
        private string _MessageLabelText;
        private int segmentCount;
        KeyValuePair<int, string> _fromListSelection;
        public Dictionary<int, string> _SendList;
        private string _MessageCountText;
        private bool _showErrorLabel;

        public bool ShowErrorLabel
        {
            get
            {
                return _showErrorLabel;
            }
            set
            {
                _showErrorLabel = value;
                OnPropertyChanged("ShowErrorLabel");
            }
        }
        public string MessageCountText
        {
            get
            {
                return _MessageCountText;
            }
            set
            {
                _MessageCountText = value;
                OnPropertyChanged("MessageCountText");
            }
        }
        public KeyValuePair<int, string> FromListSection
        {
            get
            {
                return _fromListSelection;
            }
            set
            {
                ShowErrorLabel = false;
                _fromListSelection = value;
                if (_fromListSelection.Key == 1 || _fromListSelection.Key == 2)
                {
                    AlphaNumericHeader = true;
                    OptOutOverride = false;
                    if (_fromListSelection.Key == 2)
                    {
                        OptOutOverride = true;
                    }
                }
                else
                {
                    AlphaNumericHeader = false;
                    OptOutOverride = false;
                    if (_fromListSelection.Key == 4)
                    {
                        OptOutOverride = true;
                    }
                }
            }
        }

        private bool _OptOutOverride;

        public bool OptOutOverride
        {
            get { return _OptOutOverride; }
            set { _OptOutOverride = value; }
        }


        private bool _AlphaNumericHeader;

        public bool AlphaNumericHeader
        {
            get { return _AlphaNumericHeader; }
            set { _AlphaNumericHeader = value; }
        }

        public Dictionary<int, string> SendList
        {
            get
            {
                return _SendList;
            }
            set
            {
                _SendList = value;
            }
        }

        public SmsSenderViewModel(Contact inContact, Action<bool, Contact> resultHandler, string caption = "Send SMS")
        {
            _selectedContact = inContact;
            if (inContact != null)
            {
                DestinationNumber = inContact.Number ;
                TxtIsReadOnly = true;
            } else
            {
                DestinationNumber = "";
                TxtIsReadOnly = false;
            }
            
            _resultHandler = resultHandler;
            initializeCommands();
            Caption = caption;
            MessageLabelText = "Segment 1";
            segmentCount = 1;

            _SendList = new Dictionary<int, string>();
            if (WorkspaceNoteUpdateComponent._ShowAlphaNumericSendingOptions)
            {
                SendList.Add(1, "Use alphanumeric sender id – no replies"); // Set Alphanumeric Flag.
                SendList.Add(2, "Use alphanumeric sender id with over-ride on opt-out – no replies"); // Set Alphanumeric flag with Opt Out Override Flag.
            }
            SendList.Add(3, "Use displayed number");
            SendList.Add(4, "Use displayed number with over-ride on opt-out");
            MessageCountText = "0 of 160";
            ShowErrorLabel = false;
        }

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

        public string MessageLabelText
        {
            get
            {
                return _MessageLabelText;
            }
            set
            {
                _MessageLabelText = value;
                OnPropertyChanged("MessageLabelText");
            }
        }

        private int _SMSMaxLength;

        public int SMSMaxLength
        {
            get { return _SMSMaxLength; }
            set { _SMSMaxLength = value; }
        }


        public bool TxtIsReadOnly
        {
            get
            {
                return _IsReadOnly;
            }
            set
            {
                _IsReadOnly = value;
                OnPropertyChanged("TxtIsReadOnly");
            }
        }

        public string UserInput
        {
            get
            {
                return _userInput;
            }
            set
            {                
                _userInput = value;
                segmentCount = _userInput.Length / 160;
                MessageLabelText = String.Format("Segment {0}",segmentCount + 1);
                MessageCountText = String.Format("{0} of {1}", _userInput.Length, 160 * (segmentCount + 1));
                OnPropertyChanged("UserInput");
            }
        }

        public Contact SelectedContact
        {
            get
            {
                return _selectedContact;
            }
            set
            {
                _selectedContact = value;
                OnPropertyChanged("SelectedContact");
            }
        }

        public ICommand AcceptCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ICommand DialCommand { get; set; }

        private void initializeCommands()
        {
            AcceptCommand = new BLDelegateCommand(accept);
            CancelCommand = new BLDelegateCommand(cancel);
            DialCommand = new BLDelegateCommand(dial);
        }

        private void cancel(object obj)
        {
            _resultHandler(false, null);
        }

        private void dial(object obj)
        {
            var dialog = new TransferDialog();

            ConsoleWindowHelper.SetupOwner(dialog);
            dialog.DataContext = new TransferDialogViewModel(null, (result, contact) =>
            {
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
                    Logger.Log.Error("showDialPad Exception :::::: ", ex);
                }
                dialog.Close();
            }, "Dial", true, WorkspaceNoteUpdateComponent._LockInternationalCalls);

            dialog.ShowDialog();
        }
        private string RemoveSpecialCharacters(string str)
        {
            string retVal = "";
            if (!string.IsNullOrWhiteSpace(str))
            {
                retVal = Regex.Replace(str, "[^0-9+]+", "", RegexOptions.Compiled);
            }
            return retVal;
        }
        private void accept(object obj)
        {
            Contact contact = SelectedContact;
            if (contact == null)
            {
                contact = new Contact
                {
                    Description = UserInput,
                    Name = UserInput,
                    Number = RemoveSpecialCharacters(UserInput),
                    TransferType = TransferTypes.Cold
                };
            }
            _resultHandler(true, contact);
            //UserInput = "";
        }

    }
}
