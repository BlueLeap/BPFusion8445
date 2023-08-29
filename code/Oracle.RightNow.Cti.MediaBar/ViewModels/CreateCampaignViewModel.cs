using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.RightNow.Cti.Model;
using Oracle.RightNow.Cti.AddIn;
using System.Windows.Input;
using Microsoft.Windows.Controls;
using Oracle.RightNow.Cti.MediaBar.Helpers;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Globalization;
using System.Collections.ObjectModel;
using Oracle.RightNow.Cti.MediaBar.Views;

namespace Oracle.RightNow.Cti.MediaBar.ViewModels
{
    public enum CampaignScheduleOption { Immediately, Later }
    public class CreateCampaignViewModel : ViewModel
    {
        public SMSCampaignModel currentEditCampaign;
        public string Caption { get; set; }
        private readonly Action<bool,SMSCampaignModel> _resultHandler;
        public SortedDictionary<int,string> _SendList;
        public List<string> RecurrenceList;
        private string _RecurrenseMode;
        private string _RecurrenceFrequency;
        private IList<String> _ContactIdList;
        private string _campaignName;
        private CampaignScheduleOption _scheduleoption;
        private bool _EnableExactSend;
        private string _userInput;
        private string _MessageLabelText;
        private string _MessageCountText;
        private int segmentCount;
        private RightNowObjectProvider _objectProvider;
        public int _ContactListIDInt;
        private Dictionary<string,string> _MergeFields;
        private int _AccountID;
        KeyValuePair<int, string> _fromListSelection;
        private ObservableCollection<BLMessageTemplate> _ListMessageTemplate;

        public ObservableCollection<BLMessageTemplate> MessageTemplates
        {
            get { return _ListMessageTemplate; }
            set
            {
                _ListMessageTemplate = value;
                OnPropertyChanged("MessageTemplates");
            }
        }

        private BLMessageTemplate _SelectedTemplate;

        public BLMessageTemplate SelectedTemplate
        {
            get { return _SelectedTemplate; }
            set
            {
                _SelectedTemplate = value;

                if (_SelectedTemplate.Id != 0)
                {
                    UserInput = _SelectedTemplate.MessageBody;
                }

                OnPropertyChanged("SelectedTemplate");
            }
        }

        public string CampaignName
        {
            get { return _campaignName; }
            set { _campaignName = value;
                OnPropertyChanged("CampaignName");
            }
        }

        private DateTime _RunTime ;

        public DateTime RunTime
        {
            get { return _RunTime ; }
            set { _RunTime  = value; }
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

        private string _ContactListID;

        public string ContactListID
        {
            get { return _ContactListID; }
            set { _ContactListID = value;
                _ContactListIDInt = 0;
                ContactListDetails = Messages.MessageDisplayCampaignClickFetch;
            }
        }

        public bool EnableExactSend
        {
            get { return _EnableExactSend; }
            set
            {
                _EnableExactSend = value;
                OnPropertyChanged("EnableExactSend");
            }
            
        }

        public CampaignScheduleOption ScheduleOption
        {
            get { return _scheduleoption; }
            set { _scheduleoption = value;
                OnPropertyChanged("ScheduleOption");
                EnableExactSend = _scheduleoption == CampaignScheduleOption.Immediately ? false : true;
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
                MessageLabelText = String.Format("Segment {0}", segmentCount + 1);
                MessageCountText = String.Format("{0} of {1}", _userInput.Length, 160 * (segmentCount + 1));
                //MessageLabelText = String.Format("Message: ({0}) of {1}\nSegment {2}", _userInput.Length, 160 * (segmentCount + 1), segmentCount + 1);
                OnPropertyChanged("UserInput");
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

        public int SMSMaxLength
        {
            get
            {
                return MediaBarAddIn._SMSMaxLength;
            }
        }

        public Dictionary<string,string> MergeFields
        {
            get
            {
                return _MergeFields;
            }
            set
            {
                _MergeFields = value;
                OnPropertyChanged("MergeFields");
            }
        }

        public KeyValuePair<int,string> FromListSection
        {
            get
            {
                return _fromListSelection;
            }
            set
            {
                _fromListSelection = value;
                if(_fromListSelection.Key == 1 || _fromListSelection.Key == 2)
                {
                    AlphaNumericHeader = true;
                    OptOutOverride = false;
                    if (_fromListSelection.Key == 2)
                    {
                        OptOutOverride = true;
                    } 
                } else
                {
                    AlphaNumericHeader = false;
                    OptOutOverride = false;
                    if (_fromListSelection.Key == 4)
                    {
                        OptOutOverride = true;
                    }
                }
                OnPropertyChanged("FromListSection");
            }
        }

        #region 
        //There has to be a better way then this.
        private bool _IsDailyTabHidden; 
        private bool _IsWeeklyTabHidden;
        private bool _IsMonthlyTabHidden;
        private bool _IsYearlyTabHidden;
        #endregion
        public ICommand AcceptCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ICommand FetchCommand {get; set;}
        public ICommand TemplateCommand { get; set; }
        public CreateCampaignViewModel(Action<bool,SMSCampaignModel> resultHandler, IList<string> contactIdList, Dictionary<string,string> mergeFields, RightNowObjectProvider rnObject, StaffAccountInfo staffAccount, int accID, SMSCampaignModel campaign = null, string caption = "Create SMS Campaign")
        {
            _AccountID = accID;
            currentEditCampaign = campaign;
            _resultHandler = resultHandler;
            initializeCommands();
            Caption = caption;           
            _SendList = new SortedDictionary<int, string>();
            RecurrenceList = new List<string>();
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
                if (item.Value.ToLower().Equals(MediaBarAddIn._ShowSendingOptionsDefault))
                {
                    FromListSection = new KeyValuePair<int, string>(item.Key, item.Value);
                }
            }

            RecurrenceMode = "No";
            RecurrenceList.Add("Daily");
            RecurrenceList.Add("Weekly");
            RecurrenceList.Add("Yearly");
            RecurrenceList.Add("Monthly");
            RecurrenseFrequency = "Daily";
            IsYearlyTabHidden = IsMonthlyTabHidden = IsWeeklyTabHidden  = false;
            IsDailyTabHidden = true;
            _ContactIdList = contactIdList;
            RunTime = DateTime.Now;
            ScheduleOption = CampaignScheduleOption.Immediately;
            MessageLabelText = "Segment 1";
            MessageCountText = "0 of 160";
            segmentCount = 1;
            _objectProvider = rnObject;
            ContactListDetails = Messages.MessageDisplayCampaignClickFetch;
            _ContactListIDInt = 0;
            if (mergeFields == null || mergeFields.Count == 0) {
                mergeFields = new Dictionary<string, string>();
                mergeFields.Add("Fetching Fields. Please wait.", "Fetching Fields. Please wait.");
                MergeFields = mergeFields;
                Task.Factory.StartNew(() =>
                {
                    mergeFields = _objectProvider.GetContactFields("Contact");
                    //mergeFields = mergeFields.Concat(_objectProvider.GetContactCustomFields()).ToDictionary(x => x.Key, x => x.Value);
                    mergeFields = mergeFields.Concat(_objectProvider.GetCustomObjects(staffAccount.ContactID == 0 ? 1 : staffAccount.ContactID)).ToDictionary(x => x.Key, x => x.Value);
                    MergeFields = mergeFields.OrderBy(key => key.Key).ToDictionary(x => x.Key, x => x.Value);
                });
            }
            else
            {
                MergeFields = mergeFields.OrderBy(key => key.Key).ToDictionary(x => x.Key, x => x.Value);//.OrderBy(q => q).Distinct().ToDictionary(x => x.Key, x => x.Value);
            }

            if(currentEditCampaign != null)
            {
                CampaignName = currentEditCampaign.CampaignName;
                ContactListID = currentEditCampaign.ListID.ToString();
                UserInput = currentEditCampaign.MessageBody;
                if(currentEditCampaign.RunTime != null)
                {
                    RunTime = (DateTime)currentEditCampaign.RunTime;
                    try
                    {
                        RunTime = RunTime.ToLocalTime();
                    }
                    catch (Exception ex) { }
                    ScheduleOption = CampaignScheduleOption.Later;
                } else
                {
                    ScheduleOption = CampaignScheduleOption.Immediately;
                }
                
                ContactListDetails = Messages.MessageDisplayCampaignFetching;
                Task.Factory.StartNew(() =>
                    {
                        int parsedValue;
                        int.TryParse(ContactListID, out parsedValue);
                        int totalContact = 0;
                        string contactName = ""; string createdDate = ""; string updatedDate = "";
                        _objectProvider.GetContactListInformationV1(parsedValue, out totalContact, out contactName, out createdDate, out updatedDate);
                        try
                        {
                            DateTime cDate = Convert.ToDateTime(createdDate);
                            DateTime uDate = Convert.ToDateTime(updatedDate);
                            createdDate = cDate.ToString("dd-MM-yyyy");
                            updatedDate = uDate.ToString("dd-MM-yyyy");
                        }
                        catch (Exception ex) { }
                        if (/*totalContact != 0 &&*/ !contactName.Equals(""))
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                            new Action(() =>
                            {
                                if (!contactName.Equals("Not Found"))
                                {
                                    if (String.IsNullOrEmpty(CampaignName))
                                    {
                                        CampaignName = contactName;
                                    }
                                    ContactListDetails = String.Format("Created {1} Last Count ({2}) on {3}", parsedValue, createdDate, totalContact, updatedDate);
                                }
                                else
                                {
                                    ContactListDetails = String.Format("Not Found", parsedValue);
                                    CampaignName = "";
                                }
                                _ContactListIDInt = parsedValue;
                            })
                            );
                        }
                        else
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                            new Action(() =>
                            {
                                ContactListDetails = String.Format(Messages.MessageCannotFetchContactListDetails, parsedValue);
                                CampaignName = "";
                            })
                            );
                        }
                    });
            }
            LoadTemplateList();
        }
        private void initializeCommands()
        {
            AcceptCommand = new DelegateCommand(accept);
            CancelCommand = new DelegateCommand(cancel);
            FetchCommand = new DelegateCommand(fetch);
            TemplateCommand = new DelegateCommand(SaveTemplate);
        }
        private void cancel(object obj)
        {
            _resultHandler(false, null);
        }

        private void fetch (object obj)
        {
            if (String.IsNullOrEmpty(ContactListID))
            {
                ContactListDetails = Messages.MessageEnterContactListValue;
            }
            else
            {

                ContactListDetails = Messages.MessageDisplayCampaignFetching;
                CampaignName = "";
                int parsedValue;
                if (int.TryParse(ContactListID, out parsedValue))
                {
                    Task.Factory.StartNew(() =>
                    {
                        int totalContact = 0;
                        string contactName = ""; string createdDate = ""; string updatedDate = "";
                        _objectProvider.GetContactListInformationV1(parsedValue, out totalContact, out contactName, out createdDate, out updatedDate);
                        try
                        {
                            DateTime cDate = Convert.ToDateTime(createdDate);
                            DateTime uDate = Convert.ToDateTime(updatedDate);
                            createdDate = cDate.ToString("dd-MM-yyyy");
                            updatedDate = uDate.ToString("dd-MM-yyyy");
                        }
                        catch (Exception ex) { }
                        if (/*totalContact != 0 &&*/ !contactName.Equals(""))
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                            new Action(() =>
                            {
                                if(!contactName.Equals("Not Found")) {
                                    if (String.IsNullOrEmpty(CampaignName))
                                    {
                                        CampaignName = contactName;
                                    }
                                    ContactListDetails = String.Format("Created {1} Last Count ({2}) on {3}", parsedValue, createdDate, totalContact, updatedDate);
                                } else
                                {
                                    ContactListDetails = String.Format("Not Found", parsedValue);
                                    CampaignName = "";
                                }
                                _ContactListIDInt = parsedValue;
                            })
                            );
                        }
                        else
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                            new Action(() =>
                            {
                                ContactListDetails = String.Format(Messages.MessageCannotFetchContactListDetails, parsedValue);
                                CampaignName = "";
                            })
                            );
                        }
                    });
                }
                else
                {
                    Task.Factory.StartNew(() =>
                    {
                        int totalContact = 0;
                        int contactListId = 0; ;
                        _objectProvider.GetContactListInformation(ContactListID.Trim().ToLower(), out totalContact, out contactListId);
                        if (/*totalContact != 0 &&*/ contactListId != 0)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                            new Action(() =>
                            {
                                CampaignName = ContactListID;
                                ContactListDetails = String.Format("Contact List {0} : {1} ({2})", contactListId, ContactListID, totalContact);
                                _ContactListIDInt = contactListId;
                            })
                            );
                        }
                        else
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                            new Action(() =>
                            {
                                CampaignName = "";
                                ContactListDetails = String.Format(Messages.MessageCannotFetchContactListDetails, ContactListID);
                            })
                            );
                        }
                    });
                }
            }
        }

        private void accept(object obj)
        {
            _resultHandler(true, currentEditCampaign);
        }

        
        public string RecurrenceMode
        {
            get
            {
                return _RecurrenseMode;
            } set
            {
                _RecurrenseMode = value;               
                OnPropertyChanged("RecurrenceMode");
            }
        }

        public string RecurrenseFrequency
        {
            get
            {
                return _RecurrenceFrequency;
            }
            set
            {
                _RecurrenceFrequency = value;
                if (_RecurrenceFrequency.Equals("Monthly"))
                {
                    IsMonthlyTabHidden = false;
                    IsDailyTabHidden = IsWeeklyTabHidden = IsYearlyTabHidden = true;
                }
                else if (_RecurrenceFrequency.Equals("Weekly"))
                {
                    IsWeeklyTabHidden = false;
                    IsDailyTabHidden = IsMonthlyTabHidden = IsYearlyTabHidden = true;
                }
                else if (_RecurrenceFrequency.Equals("Yearly"))
                {
                    IsYearlyTabHidden = false;
                    IsDailyTabHidden = IsMonthlyTabHidden = IsWeeklyTabHidden = true;
                }
                else
                {
                    IsDailyTabHidden = false;
                    IsYearlyTabHidden = IsMonthlyTabHidden = IsWeeklyTabHidden = true;
                }
                OnPropertyChanged("RecurrenseFrequency");
            }
        }

        public SortedDictionary<int,string> SendList
        {
            get
            {
                return _SendList;
            } set
            {
                _SendList = value; 
            }
        }

        public List<string> RecurrenceItemList
        {
            get { return RecurrenceList; }
            
        }
        public bool IsDailyTabHidden
        {
            get
            {
                return !_IsDailyTabHidden;
            } set
            {
                _IsDailyTabHidden = value;
                OnPropertyChanged("IsDailyTabHidden");
            }
        }

        public bool IsWeeklyTabHidden
        {
            get
            {
                return !_IsWeeklyTabHidden;
            }
            set
            {
                _IsWeeklyTabHidden = value;
                OnPropertyChanged("IsWeeklyTabHidden");
            }
        }
        public bool IsMonthlyTabHidden
        {
            get
            {
                return !_IsMonthlyTabHidden;
            }
            set
            {
                _IsMonthlyTabHidden = value;
                OnPropertyChanged("IsMonthlyTabHidden");
            }
        }
        public bool IsYearlyTabHidden
        {
            get
            {
                return !_IsYearlyTabHidden;
            }
            set
            {
                _IsYearlyTabHidden = value;
                OnPropertyChanged("IsYearlyTabHidden");
            }
        }

        public IList<string> ContactList
        {
            get
            {
                return _ContactIdList;
            }
        }

        private string _contactListDetails;

        public string ContactListDetails
        {
            get { return _contactListDetails; }
            set { _contactListDetails = value;
                OnPropertyChanged("ContactListDetails");
            }
        }
        private void LoadTemplateList()
        {
            if (MessageTemplates != null)
            {
                MessageTemplates.Clear();
            }
            else
            {
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
                                LoadTemplateList();
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
                if (inputDialog.ShowDialog() == true)
                {
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
                                    LoadTemplateList();
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
    }
}
