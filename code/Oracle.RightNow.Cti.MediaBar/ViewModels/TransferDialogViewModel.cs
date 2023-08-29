using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Oracle.RightNow.Cti.Model;
using PhoneNumbers;
using Oracle.RightNow.Cti.AddIn;
using System.Text.RegularExpressions;

namespace Oracle.RightNow.Cti.MediaBar.ViewModels {
    public class TransferDialogViewModel : ViewModel {
        private Contact _selectedContact;
        private string _userInput;
        private readonly Action<bool, Contact> _resultHandler;
        private PhoneNumberUtil phoneUtil;
        private AsYouTypeFormatter formatter;
        private List<string> _countryCode;
        private bool _forWebRTCCalling;
        private string _selectedCountryCode;
        private string _errorText;
        private bool _showErrorLabel;
        private bool _preventInternationalCall;
        public TransferDialogViewModel(IList<Contact> contacts, Action<bool, Contact> resultHandler, string caption = "Transfer", bool forWebRTCCalling = false, bool preventInternationalCall = false) {
            _resultHandler = resultHandler;
            if (contacts != null)
            {
                Contacts = new ObservableCollection<Contact>(contacts);
            }

            initializeCommands();
            phoneUtil = PhoneNumberUtil.GetInstance();
            
            
            Caption = caption;
            _forWebRTCCalling = forWebRTCCalling;            
            _errorText = "";
            _showErrorLabel = false;
            _preventInternationalCall = preventInternationalCall;
            try
            {
                SelectedCountryCode = phoneUtil.GetRegionCodeForCountryCode(Convert.ToInt32(MediaBarAddIn._LocalCountryCode));
            } catch (Exception ex)
            {
                SelectedCountryCode = "AU";
            }

            if (_preventInternationalCall)
            {
                
                _countryCode = new List<string> { SelectedCountryCode };
            } else
            {
                _countryCode = phoneUtil.GetSupportedRegions().OrderBy(q => q).ToList();
            }
        }

        public HashSet<string> ContryCodes
        {
            get
            {
                return phoneUtil.GetSupportedRegions(); ;
            }
        }

        public bool IsWebRTCCall
        {
            get
            {
                return _forWebRTCCalling;
            }
        }

        public bool ShowWebErrorLabel
        {
            get
            {
                return _showErrorLabel;
            }
            set
            {
                _showErrorLabel = value;
                OnPropertyChanged("ShowWebErrorLabel");
            }
        }


        public string DestinationNumberMargin
        {
            get
            {
                return String.Format("{0},5,5,0", _forWebRTCCalling ? "70" : "5");
            }
        }
        public List<string> CountryCodeList
        {
            get
            {
                return _countryCode;
            }
        }

        
        public string SelectedCountryCode
        {
            get
            {
                return _selectedCountryCode;
            }
            set
            {
                if (_selectedCountryCode != value)
                {
                    _selectedCountryCode = value;
                    UserInput = "";
                    formatter = phoneUtil.GetAsYouTypeFormatter(_selectedCountryCode);
                    OnPropertyChanged("SelectedCountryCode");

                }
            }
        }

        public string ErrorText
        {
            get
            {
                return _errorText;
            }
            set
            {
                if (_errorText != value)
                {
                    _errorText = value;
                    OnPropertyChanged("ErrorText");

                }
            }
        }

        public ObservableCollection<Contact> Contacts { get; set; }

        public string Caption { get; set; }

        public Contact SelectedContact {
            get {
                return _selectedContact;
            }
            set {
                _selectedContact = value;
                OnPropertyChanged("SelectedContact");
            }
        }

        public string UserInput {
            get {
                return _userInput;
            }
            set {
                _userInput = value;
                if (string.IsNullOrEmpty(_userInput) && formatter != null)
                {
                    formatter.Clear();
                }
                OnPropertyChanged("UserInput");
            }
        }

        public ICommand DigitInputCommand { get; set; }
        public ICommand AcceptCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        private void initializeCommands() {
            DigitInputCommand = new DelegateCommand(digitInput);
            AcceptCommand = new DelegateCommand(accept);
            CancelCommand = new DelegateCommand(cancel);
            ClearCommand = new DelegateCommand(clear);
        }

        private void clear(object obj)
        {
            formatter.Clear();
            UserInput = "";
        }

        private void cancel(object obj) {
            formatter.Clear();
            _resultHandler(false, null);
        }
  
        private void accept(object obj) {
            formatter.Clear();
            Contact contact = SelectedContact;
            if (contact ==null){
                contact = new Contact{
                    Description = UserInput,
                    Name= UserInput,
                    Number = UserInput,
                    TransferType = TransferTypes.Cold
                };
            }
            if (string.IsNullOrEmpty(contact.Number) || contact.Number.Length < 4)
            {
                ErrorText = "Not a valid number.";
                ShowWebErrorLabel = true;
            }
            else
            {
                _resultHandler(true, contact);
            }
            UserInput = "";
        }
  
        private void digitInput(object obj) {
            ShowWebErrorLabel = false;
            if (_forWebRTCCalling)
            {
                UserInput = formatter.InputDigit(obj.ToString()[0]);
                try
                {
                    PhoneNumber ccPhoneNumber = phoneUtil.Parse(UserInput, SelectedCountryCode);
                    UserInput = phoneUtil.Format(ccPhoneNumber, PhoneNumberFormat.E164);
                }
                catch (NumberParseException e)
                {
                    ShowWebErrorLabel = true;
                    ErrorText = "Cannot conver the number to E164 format. WebRTC calling may not work.";
                }
            }
            else
            {
                UserInput += obj.ToString();
            }
        }
    }
}
