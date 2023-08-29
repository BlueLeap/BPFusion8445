using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Oracle.RightNow.Cti.Model;
using Oracle.RightNow.Cti.AddIn;
using PhoneNumbers;
using System.Text.RegularExpressions;
using Oracle.RightNow.Cti.MediaBar.Views;
using Microsoft.Windows.Controls;
using Oracle.RightNow.Cti.MediaBar.Helpers;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Oracle.RightNow.Cti.MediaBar.ViewModels
{
    public class SmsSenderViewModel : ViewModel
    {
        private Contact _selectedContact;
        private readonly Action<bool, Contact> _resultHandler;
        private string _userInput;
        public string Caption { get; set; }
        private string _destinationNumber;
        private bool _IsReadOnly;
        private PhoneNumberUtil phoneUtil;
        private string _MessageLabelText;
        private int segmentCount;
        KeyValuePair<int, string> _fromListSelection;
        public Dictionary<int, string> _SendList;
        private string _MessageCountText;
        private bool _showErrorLabel;
        private ObservableCollection<BLMessageTemplate> _ListMessageTemplate;
        private RightNowObjectProvider _objectProvider;
        private int _AccountID;
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

        private BLMessageTemplate _SelectedTemplate;

        public BLMessageTemplate SelectedTemplate
        {
            get { return _SelectedTemplate; }
            set { _SelectedTemplate = value;

                if (_SelectedTemplate.Id != 0)
                {
                    UserInput = _SelectedTemplate.MessageBody;
                }

                OnPropertyChanged("SelectedTemplate");
            }
        }


        public ObservableCollection<BLMessageTemplate> MessageTemplates
        {
            get { return _ListMessageTemplate; }
            set { _ListMessageTemplate = value;
                OnPropertyChanged("MessageTemplates");
            }
        }
        public SmsSenderViewModel(Contact inContact, Action<bool, Contact> resultHandler, RightNowObjectProvider provider, int AccountID, string caption = "Send SMS")
        {
            _AccountID = AccountID;
            _objectProvider = provider;
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
            phoneUtil = PhoneNumberUtil.GetInstance();
            MessageLabelText = "Segment 1";
            segmentCount = 1;

            _SendList = new Dictionary<int, string>();
            if (MediaBarAddIn._ShowAlphaNumericSendingOptions)
            {
                SendList.Add(1, "Use alphanumeric sender id – no replies"); // Set Alphanumeric Flag.
                SendList.Add(2, "Use alphanumeric sender id with over-ride on opt-out – no replies"); // Set Alphanumeric flag with Opt Out Override Flag.
                SendList.Add(4, "Use displayed number with over-ride on opt-out");
            }
            else if (MediaBarAddIn._ShowOptOutSendingOptions)
            {
                SendList.Add(2, "Use alphanumeric sender id with over-ride on opt-out – no replies"); // Set Alphanumeric flag with Opt Out Override Flag.
                SendList.Add(4, "Use displayed number with over-ride on opt-out");
            }

            SendList.Add(3, "Use displayed number");
            foreach (var item in SendList)
            {
                if(item.Value.ToLower().Equals(MediaBarAddIn._ShowSendingOptionsDefault)) {
                    FromListSection = new KeyValuePair<int, string>(item.Key, item.Value);
                }
            }

            MessageCountText = "0 of 160";
            //
            ShowErrorLabel = false;
            LoadTemplateList();
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

        public int SMSMaxLength
        {
            get
            {
                return MediaBarAddIn._SMSMaxLength;
            }
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

        public ICommand TemplateCommand { get; set; }

        private void initializeCommands()
        {
            AcceptCommand = new DelegateCommand(accept);
            CancelCommand = new DelegateCommand(cancel);
            DialCommand = new DelegateCommand(dial);
            TemplateCommand = new DelegateCommand(SaveTemplate);
        }

        private void cancel(object obj)
        {
            _resultHandler(false, null);
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
                    System.Windows.MessageBox.Show(Messages.MessagePerformOutboundDial, Messages.MessageBoxTitle);
                    Logger.Logger.Log.Error("showDialPad Exception :::::: ", ex);
                }
                dialog.Close();
            }, "Dial", true, MediaBarAddIn._LockInternationalCalls);

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

        private void SaveTemplate(object obj)
        {
            if (SelectedTemplate.Id > 0)
            {
                BLMessageTemplate template = _SelectedTemplate;
                template.MessageBody = UserInput;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (!_objectProvider.CreateUpdateSMSTemplate(ref template, true))
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                            new Action(() =>
                            {
                                System.Windows.MessageBox.Show(Messages.CannotUpdateMessageTemplate, Messages.MessageBoxTitle);
                            })
                            );
                        }
                        else
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                            new Action(() =>
                            {
                                //LoadTemplateList();
                            })
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                });
            }
            else if (SelectedTemplate.Id == 0)
            {
                InputDialog inputDialog = new InputDialog("Please enter the template name:", "");
                if (inputDialog.ShowDialog() == true) {
                    BLMessageTemplate template = new BLMessageTemplate();
                    template.MessageBody = UserInput;
                    template.TemplateName = inputDialog.Answer;
                    template.CreatedByAgentID = _AccountID;// Convert.ToInt32(RightNowGlobalContext.AccountId);

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (!_objectProvider.CreateUpdateSMSTemplate(ref template))
                            {
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    System.Windows.MessageBox.Show(Messages.CannotCreateMessageTemplate, Messages.MessageBoxTitle);
                                })
                                );
                            }
                            else
                            {
                                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                                 DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    MessageTemplates.Add(template);
                                    SelectedTemplate = template;
                                    //LoadTemplateList();
                                })
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    });
                }
            }
            
        }

        private void LoadTemplateList()
        {
           if(MessageTemplates != null)
            {
                MessageTemplates.Clear();
            } else { 
                MessageTemplates = new ObservableCollection<BLMessageTemplate>();
            }

            BLMessageTemplate temp = new BLMessageTemplate();
            temp.TemplateName = "Create new template.";
            temp.Id = 0;

            MessageTemplates.Add(temp);

            SelectedTemplate = MessageTemplates[0];
            //return;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ObservableCollection<BLMessageTemplate> tempMessage;
                    tempMessage = new ObservableCollection<BLMessageTemplate>(_objectProvider.GetObjects<BLMessageTemplate>());
                    if (tempMessage.Count == 0)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                               DispatcherPriority.Background,
                               new Action(() =>
                               {
                                   //SelectedTemplate = 0;
                               })
                               );
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                               DispatcherPriority.Background,
                               new Action(() =>
                               {
                                   foreach (BLMessageTemplate tmp in tempMessage)
                                   {
                                       MessageTemplates.Add(tmp);
                                   }
                                   //SelectedTemplate = 0;
                                   //SelectedTemplate = MessageTemplates.Min(x => x.Id);
                               })
                               );

                    }

                }
                catch (Exception ex)
                {
                    temp.TemplateName = "Problem fetching templates.";
                }
            });
        }
    }
}
