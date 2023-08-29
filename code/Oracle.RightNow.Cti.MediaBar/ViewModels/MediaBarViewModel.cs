// ===========================================================================================
//  Oracle RightNow Connect
//  CTI Sample Code
// ===========================================================================================
//  Copyright © Oracle Corporation.  All rights reserved.
// 
//  Sample code for training only. This sample code is provided "as is" with no warranties 
//  of any kind express or implied. Use of this sample code is pursuant to the applicable
//  non-disclosure agreement and or end user agreement and or partner agreement between
//  you and Oracle Corporation. You acknowledge Oracle Corporation is the exclusive
//  owner of the object code, source code, results, findings, ideas and any works developed
//  in using this sample code.
// ===========================================================================================

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Oracle.RightNow.Cti.MediaBar.Properties;
using Oracle.RightNow.Cti.MediaBar.Views;
using Oracle.RightNow.Cti.Model;
using System.Windows.Interop;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Oracle.RightNow.Cti.AddIn;
using RightNow.AddIns.AddInViews;
using RightNow.AddIns.Common;
using Blueleap.Finesse;
using Blueleap.Finesse.Events;
using Blueleap.Finesse.Events.Callback;
using Blueleap.Finesse.Constants;
using Oracle.RightNow.Cti.MediaBar.Helpers;
using PhoneNumbers;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Oracle.RightNow.Cti.MediaBar.ViewModels {
    [Export]
    public class MediaBarViewModel : RightNowMediaBar, IConnectionEventListner
    {        
        [Import]
        public IGlobalContext RightNowGlobalContext { get; set; }

        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private int _callCount;
        private int _emailCount;
        private int _webIncidentCount;

        private bool _canChangeState;
        private bool _canChangeConnectionState = true;
        private bool _canCompleteInteraction;
        private bool _hasInteractions;
        private bool _showDialOptions;
        private bool _showAssociateOptions;
        private bool _showWebRTCDialOptions;
        private bool _showSendSMSOptions;
        private bool _sendStateToFinesse;
        private StaffAccountInfo staffAccount;
        private string _firstNotReadystate;
        private bool _webRTCDial;
        private bool _canCreateCampaign;
        private string _customerDomain;
        private string _BLAccountUsername;
        private string _BLAccountPassword;
        private string _BLAccountURL;
        private bool _IsMediaBarLoading;

        public bool IsMediaBarLoading
        {
            get { return _IsMediaBarLoading; }
            set { _IsMediaBarLoading = value;
                OnPropertyChanged("IsMediaBarLoading");
                OnPropertyChanged("IsMediaBarActive");
            }
        }

        public bool IsMediaBarActive
        {
            get { return !IsMediaBarLoading; }
        }

        private string _loadingLabel;

        public string LoadingLabel
        {
            get { return _loadingLabel; }
            set { _loadingLabel = value;
                OnPropertyChanged("LoadingLabel");
            }
        }

        private string _IconURL;

        public string IconURL
        {
            get { return _IconURL; }
            set { _IconURL = value;
                OnPropertyChanged("IconURL");
            }
        }

        private string _WebRTCURL;

        public string WebRTCURL
        {
            get { return _WebRTCURL; }
            set { _WebRTCURL = value; }
        }


        public string MyInboxReportText { get { return MediaBarAddIn._MyInboxReportText; } }
        public string MyIncidentReportText { get { return MediaBarAddIn._MyIncidentReportText; } }
        public string ProcessCallReportIText { get { return MediaBarAddIn._ProcessCallReportIText; } }
        public string AnswersReportText { get { return MediaBarAddIn._AnswersReportText; } }
        public string DashboardReportText { get { return MediaBarAddIn._DashboardReportText; } }

        WebRTCBrowserContentPane _webRTCContentWindow;

        Finesse finesse;
        Dictionary<string, FinesseNotReadyReasonState> notReadyStates;
        private PhoneNumberUtil util;
        private Dictionary<string,string> mergeFields;
        public MediaBarViewModel()
        {
            initializeCommands();
            EnableContextSynchronization(SynchronizationContext.Current);
            staffAccount = null;
            _firstNotReadystate = "";
            _customerDomain = "";
            _BLAccountUsername = "";
            _BLAccountPassword = "";
            _BLAccountURL = "";
            LoadingLabel = "Loading MediaBar. Please wait.";
            finesse = new Finesse(this);
            util = PhoneNumberUtil.GetInstance();
            IsMediaBarLoading = true;
            _timer.Tick += timerTick;            
        }

       public AgentState CurrentAgentState
        {
            get
            {
                return _currentAgentState;
            }
            set
            {
                AgentState _prevAgentState = _currentAgentState;
                if (_currentAgentState != value)
                {
                    _currentAgentState = value;
                }
                
                string sendAgentState = ""; string notReadyReasonCode = ""; bool reasonDialogCancelled = false;
                if (_sendStateToFinesse == true)
                {
                    //PK: Clicked from DropDown
                    //Finesse Set state Not-Ready & Ready Only.
                    if (value == StandardAgentStates.NotReady)
                    {
                        sendAgentState = "NOT_READY";
                        if (notReadyStates.Count > 0)
                        {
                            var dialog = new Window();
                            dialog.ResizeMode = ResizeMode.CanResize;
                            dialog.ShowInTaskbar = false;
                            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            dialog.Height = 550;
                            dialog.Width = 350;
                            dialog.Title = "Not Ready - Reason Code(s)";
                            var UCdialog = new UCReasonCodeView();

                            IList<AgentState> dynamicAgentState = AgentStates.Where(p => p.SwitchMode == AgentSwitchMode.Dynamic).ToList();
                            ConsoleWindowHelper.SetupOwner(dialog);
                            UCdialog.DataContext = new UCReasonCodeViewModel(dynamicAgentState, (result, contact) =>
                            {
                                if (result)
                                {
                                    notReadyReasonCode = contact.Code;
                                    Application.Current.Dispatcher.BeginInvoke(
                                       DispatcherPriority.Background,
                                       new Action(() =>
                                       {
                                           _currentAgentState = AgentStates.Where(p => p.Code == contact.Code && p.SwitchMode == AgentSwitchMode.Dynamic).FirstOrDefault();
                                           synchronizeAndRaiseOnPropertyChanged("CurrentAgentState");
                                       })
                                   );
                                    
                                }
                                else
                                {
                                    //Do not change Not Ready state
                                    dialog.Close();
                                    reasonDialogCancelled = true;
                                }
                                dialog.Close();
                            });

                            dialog.Content = UCdialog;
                            dialog.ShowDialog();
                        }

                    }
                    else if (value == StandardAgentStates.Available)
                    {
                        sendAgentState = "READY";
                    }
                }

                //Do not change the state.
                if (reasonDialogCancelled)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                                  DispatcherPriority.Background,
                                  new Action(() =>
                                  {
                                      _currentAgentState = _prevAgentState;
                                      synchronizeAndRaiseOnPropertyChanged("CurrentAgentState");
                                  })
                              );
                    return;
                }

                if (_sendStateToFinesse == true && sendAgentState.Length > 0)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            //PK: Connecting is an intrmediate state do not send it to Finesse.
                            if (value.SwitchMode != AgentSwitchMode.Connecting)
                            {
                                int retVal = 0;
                                if (notReadyReasonCode != "")
                                {
                                    retVal = finesse.fnAgentState(sendAgentState, notReadyReasonCode);
                                }
                                else
                                {
                                    retVal = finesse.fnAgentState(sendAgentState);
                                }

                                if (retVal < 0)
                                {
                                    Logger.Logger.Log.Info("Set CurrentAgentState: Cannot set Agent State");
                                    MessageBox.Show(Messages.MessageCannotSetAgentState, Messages.MessageBoxTitle);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log.Info("Set CurrentAgentState: " + ex.InnerException.ToString());
                        }
                    });
                    InteractionProvider.Agent.SetState(value);
                }

                if (_sendStateToFinesse == false)
                {
                    _sendStateToFinesse = true;
                }

                CanMakeCall = _currentAgentState.SwitchMode != AgentSwitchMode.LoggedOut && _currentAgentState.SwitchMode != AgentSwitchMode.Connecting && (_currentAgentState.SwitchMode == AgentSwitchMode.NotReady || _currentAgentState.SwitchMode == AgentSwitchMode.Dynamic);
                synchronizeAndRaiseOnPropertyChanged("CurrentAgentState");
            }
        }

        public string FinesseAgentName
        {
            get;
            set;
        }

        public int FinesseAgentExtension
        {
            get
            {
                if (InteractionProvider.Credentials != null)
                {
                    return InteractionProvider.Credentials.Extension;
                }
                return 0;
            }
            set
            {
                if (InteractionProvider.Credentials != null)
                {
                    InteractionProvider.Credentials.Extension = value;
                    OnPropertyChanged("FinesseAgentExtension");
                }
            }
        }

        public string FinesseAgentID
        {
            get
            {
                if (InteractionProvider.Credentials != null)
                {
                    return InteractionProvider.Credentials.AgentID;
                }
                return String.Empty;
            }
            set
            {
                if (InteractionProvider.Credentials != null)
                {
                    InteractionProvider.Credentials.AgentID = value;
                    OnPropertyChanged("FinesseAgentID");
                }
            }
        }

        public string AgentName {
            get {
                if (InteractionProvider.Agent != null) {
                    return InteractionProvider.Agent.Name;
                }

                return string.Empty;
            }
        }

        public AgentState TempAgentState
        {
            get;
            set;
        }

        public string FinesseConnectionStatus
        {
            get;
            set;
        }

        public int CallCount {
            get {
                return _callCount;
            }
            set {
                if (_callCount != value) {
                    _callCount = value;
                    OnPropertyChanged("CallCount");
                }
            }
        }

        public bool HasInteractions {
            get {
                return _hasInteractions;
            }
            set {
                if (_hasInteractions != value) {
                    _hasInteractions = value;
                    OnPropertyChanged("HasInteractions");
                }
            }
        }

        public int EmailCount {
            get {
                return _emailCount;
            }
            set {
                if (_emailCount != value) {
                    _emailCount = value;
                    OnPropertyChanged("EmailCount");
                }
            }
        }

        public string Extension {
            get {
                if (InteractionProvider.Device != null) {
                    return InteractionProvider.Device.Address;
                }

                return string.Empty;
            }
        }

        public bool CanChangeState {
            get {
                return _canChangeState;
            }
            set {
                _canChangeState = value;
                OnPropertyChanged("CanChangeState");
            }
        }

        public bool CanCreateCampaign
        {
            get
            {
                return !MediaBarAddIn._HideSMSCampaign;
            }
            set
            {
                MediaBarAddIn._HideSMSCampaign = !value;
                OnPropertyChanged("CanCreateCampaign");
            }
        }

        public bool CanChangeConnectionState {
            get {
                return _canChangeConnectionState;
            }
            set {
                _canChangeConnectionState = value;
                OnPropertyChanged("CanChangeConnectionState");
            }
        }

        public string InteractionTime {
            get {
                if (CurrentInteraction != null) {
                    return (CurrentInteraction.Duration).ToString(@"hh\:mm\:ss");
                }

                return string.Empty;
            }
        }

        public string AnswerHangupImage {
            get {
                return (CurrentInteraction != null && CurrentInteraction.State == InteractionState.Ringing)
                       ? Resources.AnswerImageUri
                       : Resources.HangupImageUri;
            }
        }

        public string AnswerHangupTooltip {
            get {
                return (CurrentInteraction != null && CurrentInteraction.State == InteractionState.Ringing)
                       ? Resources.AnswerTooltip
                       : Resources.HangupTooltip;
            }
        }

        public string HoldRetrieveImage {
            get {
                return (CurrentInteraction != null && CurrentInteraction.State == InteractionState.Held)
                       ? Resources.RetrieveImageUri
                       : Resources.HoldImageUri;
            }
        }

        public string HoldRetrieveTooltip {
            get {
                return (CurrentInteraction != null && CurrentInteraction.State == InteractionState.Held)
                       ? Resources.RetriveTooltip
                       : Resources.HoldTooltip;
            }
        }

        public bool IsAgentLoggedIn {
            get {
                return CurrentAgentState != null && CurrentAgentState.SwitchMode != AgentSwitchMode.LoggedOut;
            }
        }

        public bool ShowWebRTCButton
        {
            get
            {
                return MediaBarAddIn._WebRTCEnable;
            }
            set
            {
                MediaBarAddIn._WebRTCEnable = value;
                OnPropertyChanged("ShowWebRTCButton");
            }
        }

        public bool ShowSMSButton
        {
            get
            {
                return MediaBarAddIn._SMSEnable;
            }
            set
            {
                //Master Override
                MediaBarAddIn._SMSEnable = value;
                OnPropertyChanged("ShowSMSButton");
            }
        }

        public bool HideDialPadWebRTC
        {
            get
            {
                return MediaBarAddIn._WebRTCDialPad;
            }
        }

        [Import]
        public IContactProvider ContactProvider { get; set; }

        #region Commands

        public ICommand LoginToggleCommand { get; set; }

        public ICommand AnswerHangUpCallCommand { get; set; }

        public ICommand HoldRetrieveCallCommand { get; set; }

        public ICommand CompleteInteractionCommand { get; set; }

        public ICommand ShowTransferDialogCommand { get; set; }

        public ICommand ShowDialOptionsCommand { get; set; }

        public ICommand DialCommand { get; set; }

        public ICommand ShowDialPadComand { get; set; }

        public ICommand ShowChangeAgentNumberComand { get; set; }

        public ICommand ShowAgentLoginCommand { get; set; }
        public ICommand AssociateContactCommand { get; set; }
        public ICommand ShowAssociateContactCommand { get; set; }

        public ICommand UnassociateContactCommand { get; set; }

        public ICommand WebRTCCallCommand { get; set; }

        public ICommand SendSMSCommand { get; set; }

        public ICommand SendSMSDialogCommand { get; set; }
        public ICommand CreateCampaignCommand { get; set; }
        public ICommand ManageCampaignCommand { get; set; }
        public ICommand MyInboxCommand { get; set; }
        public ICommand MyIncidentCommand { get; set; }
        public ICommand AnswerCommand { get; set; }
        public ICommand ProcessCallCommand { get; set; }
        public ICommand DashboardCommand { get; set; }

        public ICommand VersionCheck { get; set; }


        #endregion Commands

        public int WebIncidentCount {
            get {
                return _webIncidentCount;
            }
            set {
                if (_webIncidentCount != value) {
                    _webIncidentCount = value;
                    OnPropertyChanged("WebIncidentCount");
                }
            }
        }

        public bool CanCompleteInteraction {
            get {
                return _canCompleteInteraction;
            }
            set {
                if (_canCompleteInteraction != value) {
                    _canCompleteInteraction = value;
                    OnPropertyChanged("CanCompleteInteraction");
                }
            }
        }

        public bool ShowDialOptions {
            get {
                return _showDialOptions;
            }
            set {

                if (_showDialOptions != value) {
                    _showDialOptions = value;
                    OnPropertyChanged("ShowDialOptions");
                }
            }
        }

        public bool WebRTCDial
        {
            get
            {
                return _webRTCDial;
            }
            set
            {

                if (_webRTCDial != value)
                {
                    _webRTCDial = value;
                    OnPropertyChanged("WebRTCDial");
                }
            }
        }

        public bool ShowWebRTCDialOptions
        {
            get
            {
                return _showWebRTCDialOptions;
            }
            set
            {

                if (_showWebRTCDialOptions != value)
                {
                    _showWebRTCDialOptions = value;
                    OnPropertyChanged("ShowWebRTCDialOptions");
                }
            }
        }

        public bool ShowSendSMSOptions
        {
            get
            {
                return _showSendSMSOptions;
            }
            set
            {

                if (_showSendSMSOptions != value)
                {
                    _showSendSMSOptions = value;
                    OnPropertyChanged("ShowSendSMSOptions");
                }
            }
        }

        public bool ShowAssociateOptions
        {
            get
            {
                return _showAssociateOptions;
            }
            set
            {

                if (_showAssociateOptions != value)
                {
                    _showAssociateOptions = value;
                    OnPropertyChanged("ShowAssociateOptions");
                }
            }
        }

        public bool MyInboxVisible
        {
            get { return MediaBarAddIn._MyInboxReportID == 0 ? false: true; }
        }

       
        public bool MyIncidentVisible
        {
            get { return MediaBarAddIn._MyIncidentReportID == 0 ? false : true; }
        }       

        public bool ProcessCallVisible   
        {
            get { return MediaBarAddIn._ProcessCallReportID == 0 ? false : true; }
        }

        public bool DashboardVisible
        {
            get { return MediaBarAddIn._DashboardReportID == 0 ? false : true; }
        }

        public bool AnswersVisible
        {
            get { return MediaBarAddIn._AnswersReportID == 0 ? false : true; }
        }

        public override void OnImportsSatisfied() {
            base.OnImportsSatisfied();
            InteractionProvider.Agent.StateChanged += agentManagerStateChanged;
            InteractionProvider.Agent.NameChanged += agentNameChanged;
            InteractionProvider.Device.AddressChanged += deviceAddressChanged;
            this.SynchronizationContext.Post(o=>ToastManager.Initialize((IInteractionProvider)o), InteractionProvider);            
            _objectProvider = new RightNowObjectProvider(RightNowGlobalContext);
            IconURL = "/Oracle.RightNow.Cti.Mediabar;component/Images/blueleaplogo.png";
            resetMediaBar();
            if (MediaBarAddIn._EnableHTTPS == false)
            {
                //Override server info's default value to connect to Finesse.
                SERVERINFO.Finesse_PORT = "8082"; //UCCX default HTTP Port.
                SERVERINFO.Finesse_PROTOCOL = "http://"; //UCCX default HTTP Port.
            }

            if(!MediaBarAddIn._EnableCTILogin)
            {
                CanChangeConnectionState = false;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    string uname = ""; string pwd = ""; string url = "";
                    _objectProvider.GetCustomerDomainFromBLAccount(out uname, out pwd, out url);
                    _BLAccountPassword = pwd;
                    _BLAccountUsername = uname;
                    _BLAccountURL = url;
                    WebRTCURL = _BLAccountURL;
                    string token = "";
                    int retVal = getTokenRequest(String.Format("{0}{1}", _BLAccountURL, "/api/v1/token/"), _BLAccountUsername, _BLAccountPassword, out token);
                    if (retVal == 0)
                    {
                        BlueLeapUserAccount userAccount = new BlueLeapUserAccount();
                        if (GetAccountDetailsFromServer(_BLAccountUsername, token, out userAccount))
                        {
                            ShowWebRTCButton = true;
                            if (!String.IsNullOrEmpty(userAccount.MasterWebRTCEnable))
                            {
                                string enable = userAccount.MasterWebRTCEnable.Trim().ToLower();
                                if (enable.Equals("false") || enable.Equals("no") || enable.Equals("0"))
                                {
                                    ShowWebRTCButton = false;
                                }
                            }

                            ShowSMSButton = true;
                            if (!String.IsNullOrEmpty(userAccount.MasterSMSEnable))
                            {
                                string enable = userAccount.MasterSMSEnable.Trim().ToLower();
                                if (enable.Equals("false") || enable.Equals("no") || enable.Equals("0"))
                                {
                                    ShowSMSButton = false;
                                }
                            }

                            CanCreateCampaign = true;
                            if (!String.IsNullOrEmpty(userAccount.MasterCampaignEnable))
                            {
                                string enable = userAccount.MasterCampaignEnable.Trim().ToLower();
                                if (enable.Equals("false") || enable.Equals("no") || enable.Equals("0"))
                                {
                                    CanCreateCampaign = false;
                                }
                            }

                            CanChangeConnectionState = true;
                            if (!String.IsNullOrEmpty(userAccount.MasterCTIEnable))
                            {
                                string enable = userAccount.MasterCTIEnable.Trim().ToLower();
                                if (enable.Equals("false") || enable.Equals("no") || enable.Equals("0"))
                                {
                                    CanChangeConnectionState = false;
                                }
                            }

                            MediaBarAddIn._ShowAlphaNumericSendingOptions = true;
                            if (!String.IsNullOrEmpty(userAccount.ShowAlphaNumericSendingOptions))
                            {
                                string enable = userAccount.ShowAlphaNumericSendingOptions.Trim().ToLower();
                                if (enable.Equals("false") || enable.Equals("no") || enable.Equals("0"))
                                {
                                    MediaBarAddIn._ShowAlphaNumericSendingOptions = false;
                                }
                            }

                            MediaBarAddIn._ShowOptOutSendingOptions = true;
                            if (!String.IsNullOrEmpty(userAccount.ShowOptOutSendingOptions))
                            {
                                string enable = userAccount.ShowOptOutSendingOptions.Trim().ToLower();
                                if (enable.Equals("false") || enable.Equals("no") || enable.Equals("0"))
                                {
                                    MediaBarAddIn._ShowOptOutSendingOptions = false;
                                }
                            }

                            MediaBarAddIn._CTICreateIncident = false;
                            if (!String.IsNullOrEmpty(userAccount.CTICreateIncident))
                            {
                                string enable = userAccount.CTICreateIncident.Trim().ToLower();
                                if (enable.Equals("true") || enable.Equals("yes") || enable.Equals("1"))
                                {
                                    MediaBarAddIn._CTICreateIncident = false;
                                }
                            }

                            MediaBarAddIn._ShowSendingOptionsDefault = userAccount.ShowSendingOptionsDefault;
                            MediaBarAddIn._LocalCountryCode = userAccount.LocalCountryCode;
                            MediaBarAddIn._InternationalPrefix = userAccount.InternationalPrefix;
                            MediaBarAddIn._LocalAreaCode = userAccount.LocalAreaCode;
                            MediaBarAddIn._MobilePrefix = userAccount.MobilePrefix;
                            MediaBarAddIn._TrunkAccessCode = userAccount.TrunkAccessCode;
                            WebRTCURL = userAccount.WebRTCURL;
                            IconURL = userAccount.IconURL;

                            IsMediaBarLoading = false;

                        }
                        else
                        {
                            Application.Current.Dispatcher.BeginInvoke(
                            DispatcherPriority.Background,
                            new Action(() =>
                            {
                                LoadingLabel = "Cannot connect to the BlueLeap server. Please contact Administrator.";
                            })
                            );
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() =>
                        {
                            LoadingLabel = "Cannot connect to the BlueLeap server. Please contact Administrator.";
                        })
                        );
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        LoadingLabel = "Cannot load the Media Bar. Please contact Administrator.";
                    })
                    );
                }
            });

            Task.Factory.StartNew(() =>
            {
                try
                {
                    staffAccount = _objectProvider.GetStaffAccountInformation(RightNowGlobalContext.AccountId);

                    if (staffAccount != null)
                    {
                        Logger.Logger.Log.Debug("In GetCustomFields");
                        mergeFields = _objectProvider.GetContactFields("Contact");
                        mergeFields = mergeFields.Concat(_objectProvider.GetCustomObjects(staffAccount.ContactID == 0 ? 1 : staffAccount.ContactID)).ToDictionary(x => x.Key, x => x.Value);
                        mergeFields = mergeFields.OrderBy(key => key.Key).ToDictionary(x => x.Key, x => x.Value);
                        Logger.Logger.Log.Debug("Out GetCustomFields");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log.Debug("Cannot get StaffAccountInformation/Merge Fields", ex);
                }
            });
            _webRTCContentWindow = null;
        }

        protected override void InteractionConnectedHandler(object sender, InteractionEventArgs e) {
            base.InteractionConnectedHandler(sender, e);

            if (e.Interaction.IsRealTime)
                CanCompleteInteraction = false;
        }

        protected override void NewInteractionHandler(object sender, InteractionEventArgs e) {
            base.NewInteractionHandler(sender, e);
            if (!_timer.IsEnabled) {
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Start();
            }

            updateInteractionCount(new InteractionCountUpdateInfo {
                MediaType = e.Interaction.Type,
                UpdateType = UpdateType.Add
            });

            e.Interaction.StateChanged += interactionStateChangedHandler;
            //CanChangeConnectionState = false;
            HasInteractions = true;
        }
  
        private void interactionStateChangedHandler(object sender, InteractionStateChangedEventArgs e) {
            notifyInteractionStateProperties();
        }
  
        private void notifyInteractionStateProperties() {
            OnPropertyChanged("HoldRetrieveImage");
            OnPropertyChanged("HoldRetrieveTooltip");

            OnPropertyChanged("AnswerHangupImage");
            OnPropertyChanged("AnswerHangupTooltip");
        }

        protected override void CurrentInteractionChangedHandler(object sender, InteractionEventArgs e) {
            base.CurrentInteractionChangedHandler(sender, e);
            notifyInteractionStateProperties();

            if (CurrentInteraction != null && (!CurrentInteraction.IsRealTime || CurrentInteraction.State == InteractionState.Disconnected)) {
                CanCompleteInteraction = true;
            }
            else {
                CanCompleteInteraction = false;
            }
        }

        protected override void InteractionDisconnectedHandler(object sender, Oracle.RightNow.Cti.InteractionEventArgs e) {
            base.InteractionDisconnectedHandler(sender, e);
            CanCompleteInteraction = true;
        }

        protected override void InteractionCompletedHandler(object sender, Oracle.RightNow.Cti.InteractionEventArgs e) {
            base.InteractionCompletedHandler(sender, e);
            CanCompleteInteraction = false;
            updateInteractionCount(new InteractionCountUpdateInfo {
                MediaType = e.Interaction.Type,
                UpdateType = UpdateType.Subtract
            });

            HasInteractions = InteractionProvider.Interactions.Count != 0;
            //CanChangeConnectionState = !HasInteractions;
        }

        private void agentManagerStateChanged(object sender, AgentStateChangedEventArgs e) {
            OnPropertyChanged("IsAgentLoggedIn");
            CanChangeState = e.NewState.SwitchMode != AgentSwitchMode.LoggedOut;
        }

        private void agentNameChanged(object sender, EventArgs e) {
        }

        private void deviceAddressChanged(object sender, ExtensionChangedEventArgs e) {
            OnPropertyChanged("Extension");            
        }

        private void initializeCommands() {
            LoginToggleCommand = new DelegateCommand(o => ToggleLogin());
            AnswerHangUpCallCommand = new DelegateCommand(o => answerHangUpCall());
            HoldRetrieveCallCommand = new DelegateCommand(o => HoldRetrieveCall());
            CompleteInteractionCommand = new DelegateCommand(o => CompleteInteraction());
            ShowTransferDialogCommand = new DelegateCommand(showTransferDialog);
            ShowDialOptionsCommand = new DelegateCommand(o=> showDialOptions());
            DialCommand = new DelegateCommand(o => ValidateAndDial(o as Contact));
            ShowDialPadComand = new DelegateCommand(showDialPad);
            ShowAgentLoginCommand = new DelegateCommand(showAgentLoginDialog);
            ShowAssociateContactCommand = new DelegateCommand(o => showAssociateOptions());
            AssociateContactCommand = new DelegateCommand(o => associateContactCommand(o as Contact));
            UnassociateContactCommand = new DelegateCommand(o => unassociateContact());
            WebRTCCallCommand = new DelegateCommand(o => webRTCCall());
            SendSMSCommand = new DelegateCommand(o => sendSMS());
            SendSMSDialogCommand = new DelegateCommand(o => showSendSMSDialog(o as Contact));
            CreateCampaignCommand = new DelegateCommand(createCampaign);
            ManageCampaignCommand = new DelegateCommand(manageCampaign);
            MyInboxCommand = new DelegateCommand(myInbox);
            MyIncidentCommand = new DelegateCommand(myIncidents);
            AnswerCommand = new DelegateCommand(answer);
            ProcessCallCommand = new DelegateCommand(processCall);
            DashboardCommand = new DelegateCommand(dashboard);
            VersionCheck = new DelegateCommand(versionCheck);
            ShowChangeAgentNumberComand = new DelegateCommand(o => showAgentDialog());
            FinesseConnectionStatus = "Finesse: Not-Connected.";
            TempAgentState = StandardAgentStates.LoggedOut;

            var states = new List<AgentState>();
            // Add standard states. These are Finesse's Agents State
            states.Add(StandardAgentStates.Available);
            states.Add(StandardAgentStates.WrapUp);
            states.Add(StandardAgentStates.NotReady);
            states.Add(StandardAgentStates.NewReason);
            states.Add(StandardAgentStates.LoggedIn);
            states.Add(StandardAgentStates.LoggedOut);
            states.Add(StandardAgentStates.Reserved);
            states.Add(StandardAgentStates.Talking);
            states.Add(StandardAgentStates.Unknown);
            states.Add(StandardAgentStates.Connecting);

            AgentStates = states;
        }

        private void createCampaign(object obj)
        {
            var dialog = new Window();
            dialog.ShowInTaskbar = false;
            dialog.Width = 570;
            dialog.Height = 550;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ResizeMode = ResizeMode.CanResizeWithGrip;
            dialog.Title = "Create Campaign";
            var CCdialog = new CreateCampaign();

            ConsoleWindowHelper.SetupOwner(dialog);
            //IList<string> contactlist = _objectProvider.GetContactList();
            IList<string> contactlist = null;
            CCdialog.DataContext = new CreateCampaignViewModel((result, temp) =>
            {
                if (result)
                {
                    CreateCampaignViewModel model = (CreateCampaignViewModel)CCdialog.DataContext;
                    string validationResult = ValidateCampaignDialog(model);
                    if (validationResult == string.Empty)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            DateTime runTime = DateTime.MinValue;
                            try
                            {
                                if (model.ScheduleOption == CampaignScheduleOption.Later)
                                {
                                    runTime = model.RunTime.ToUniversalTime();// DateTime.ParseExact(model.RunTime.ToString(), "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture).ToUniversalTime();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Logger.Log.Debug("Error while converting dates", ex);
                            }

                            //DateTime.MinValue is used as a sentinal for Immediately.
                            if (_objectProvider.CreateUpdateSMSCampaign(model.CampaignName, runTime, model.UserInput, model._ContactListIDInt, model.OptOutOverride, model.AlphaNumericHeader, RightNowGlobalContext.AccountId))
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    MessageBox.Show(Messages.MessageCampaignCreatedSuccess, Messages.MessageBoxTitle);
                                })
                                );
                            }
                            else
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    MessageBox.Show(Messages.MessageCampaignCreatedError, Messages.MessageBoxTitle);
                                })
                                );
                            }
                        });
                        dialog.Close();
                    }
                    else
                    {
                        CCdialog.errLabel.Content = validationResult;
                        CCdialog.errLabel.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    dialog.Close();
                }
            }, contactlist, mergeFields,_objectProvider, staffAccount, RightNowGlobalContext.AccountId);

            dialog.Content = CCdialog;
            dialog.ShowDialog();
        }

        private void manageCampaign(object obj)
        {
            var dialog = new Window();
            dialog.ShowInTaskbar = false;
            dialog.Width = 1050;
            //dialog.Height = 768;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ResizeMode = ResizeMode.CanResizeWithGrip;
            dialog.Title = "Manage Campaign";
            var manageCampaign = new ManageCampaign(_objectProvider, mergeFields, staffAccount, RightNowGlobalContext.AccountId);

            ConsoleWindowHelper.SetupOwner(dialog);
            manageCampaign.DataContext = new ManageCampaignViewModel(result =>
            {
                dialog.Close();
                ManageCampaignViewModel model = (ManageCampaignViewModel)manageCampaign.DataContext;

            }, _objectProvider);

            dialog.Content = manageCampaign;
            dialog.ShowDialog();
        }

        private void myInbox(object obj)
        {
            RightNowGlobalContext.AutomationContext.RunReport(Convert.ToInt32(MediaBarAddIn._MyInboxReportID) /*Contacts report*/, new List<IReportFilter2> { });
        }

        private void myIncidents(object obj)
        {
            RightNowGlobalContext.AutomationContext.RunReport(MediaBarAddIn._MyIncidentReportID /*Contacts report*/, new List<IReportFilter2> { });
        }

        private void answer(object obj)
        {
            RightNowGlobalContext.AutomationContext.RunReport(MediaBarAddIn._AnswersReportID /*Contacts report*/, new List<IReportFilter2> { });
        }

        private void processCall(object obj)
        {
            RightNowGlobalContext.AutomationContext.RunReport(MediaBarAddIn._ProcessCallReportID /*Contacts report*/, new List<IReportFilter2> { });
        }

        private void dashboard(object obj)
        {
            RightNowGlobalContext.AutomationContext.RunReport(MediaBarAddIn._DashboardReportID /*Contacts report*/, new List<IReportFilter2> { });
        }

        private void versionCheck(object obj)
        {
            MessageBox.Show("BlueLeap Media Bar Version 22.1", Messages.MessageBoxTitle);
        }

        private void answerHangUpCall() {
            var call = InteractionProvider.CurrentInteraction as ICall;
            if (call != null) {
                if (call.State == InteractionState.Ringing)
                    call.Accept();
                else
                    call.HangUp();
            };
        }

        private void showDialPad(object obj) {
            var dialog = new TransferDialog();
            var contacts = ContactProvider.GetContacts();

            ConsoleWindowHelper.SetupOwner(dialog);
            dialog.DataContext = new TransferDialogViewModel(contacts, (result, contact) => {
                try
                {
                    if (result)
                    {
                        string errmsg = ""; string dialNumber = "";
                        if (ValidateNumber(contact, out dialNumber, out errmsg))
                        {
                            Dial(contact, dialNumber, false, true);
                        }
                        else
                        {                            
                            return;
                        }
                    }
                } catch (Exception ex)
                {
                    MessageBox.Show(Messages.MessagePerformOutboundDial, Messages.MessageBoxTitle);
                    Logger.Logger.Log.Error("showDialPad Exception :::::: ", ex);
                }
                if (WebRTCDial)
                {
                    WebRTCDial = false;
                }
                dialog.Close();
            }, WebRTCDial ? "WebRTC Dial" : "Dial", WebRTCDial, MediaBarAddIn._LockInternationalCalls);

            dialog.ShowDialog();
        }

        private bool ValidateDialNumber(string number, out string msg)
        {
            if (string.IsNullOrEmpty(number))
            {
                msg = "Please enter the number!.";
                return false;
            }
            else
            {
                //string pattern = @"(?:\+?61)?(?:\(0\)[23478]|\(?0?[23478]\)?)\d{8}";
                string pattern = @"[\+]{1}";
                
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                Match match = regex.Match(number);
                while (match.Success)
                {
                    string phoneNumber = match.Groups[0].Value;
                    match = match.NextMatch();
                    msg = "";
                    return true;
                }
                msg = "Not a valid number!.";
                return false;
            }
        }

        private void showDialOptions() {
            ShowDialOptions = true;
            WebRTCDial = false;
        }

        private void showAssociateOptions()
        {
            ShowAssociateOptions = true;
        }

        private void showTransferDialog(object obj) {
            //Not Used
            var dialog = new TransferDialog();
            var contacts = ContactProvider.GetContacts();

            ConsoleWindowHelper.SetupOwner(dialog);

            dialog.DataContext = new TransferDialogViewModel(contacts, (result, contact) => {
                if (result) {
                    TransferCall(contact);
                }

                dialog.Close();
            });

            dialog.ShowDialog();
        }
 
        private void updateInteractionCount(InteractionCountUpdateInfo info) {
            Application.Current.Dispatcher.Invoke(new Action<InteractionCountUpdateInfo>((updateInfo) => {
                var updateValue = updateInfo.UpdateType == UpdateType.Add ? 1 : -1;

                switch (updateInfo.MediaType)
                {
                    case MediaType.Voice:
                        CallCount += updateValue;
                        break;
                    case MediaType.Email:
                        EmailCount += updateValue;
                        break;
                    case MediaType.Chat:
                        break;
                    case MediaType.Web:
                        WebIncidentCount += updateValue;
                        break;
                    default:
                        break;
                }
            }), info);
        }

        private void timerTick(object sender, EventArgs e) {
            OnPropertyChanged("InteractionTime");
        }

        private class InteractionCountUpdateInfo {
            public MediaType MediaType { get; set; }

            public UpdateType UpdateType { get; set; }
        }

        private enum UpdateType {
            Add,
            Subtract
        }

        //To set MediaBar to the Start Logged Out State.
        private void resetMediaBar()
        {
            FinesseAgentID = "Agent";
            FinesseAgentExtension = 0;
            finesse.fnDisconnect();
            CurrentAgentState = StandardAgentStates.LoggedOut;
            CanChangeState = CurrentAgentState.SwitchMode != AgentSwitchMode.LoggedOut;
            FinesseConnectionStatus = "Finesse: Not-Connected.";
            OnPropertyChanged("IsAgentLoggedIn");
            OnPropertyChanged("FinesseConnectionStatus");
            CanAssociateContact = false;
            CanUnAssociateContact = false;
            _sendStateToFinesse = false;            
    }
    private void showAgentLoginDialog(object obj)
        {            
            if (IsAgentLoggedIn)
            {
                if (MessageBox.Show(Messages.MessageMediaBarDisconnected, Messages.MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    resetMediaBar();
                }
                return;
            }

            var dialog = new AgentLogin();
            var contacts = ContactProvider.GetContacts();

            WindowInteropHelper helper = new WindowInteropHelper(dialog);
            if (System.Windows.Forms.Application.OpenForms.Count > 0)
            {
                helper.Owner = System.Windows.Forms.Application.OpenForms[0].Handle;
            }
            else
            {

            }

            if (staffAccount == null)
            {
                staffAccount = _objectProvider.GetStaffAccountInformation(RightNowGlobalContext.AccountId);
            }

            if (staffAccount != null && staffAccount.FinesseAutoLogin) {
                dialog.agentIdText.Text = staffAccount.AcdId;
                dialog.passwordText.Password = staffAccount.AcdPassword;
                dialog.extensionText.Text = staffAccount.Extension;
                dialog.chkRemember.IsChecked = staffAccount.FinesseAutoLogin;
            }
            ConsoleWindowHelper.SetupOwner(dialog);
            dialog.DataContext = new AgentLoginViewModel(contacts, (result, contact) =>
            {
                if (result)
                {
                    string validationResult = Validate(dialog);
                    if (validationResult == string.Empty)
                    {
                        if(staffAccount == null)
                        {
                            staffAccount = new StaffAccountInfo();
                        }

                        bool previousCheckedState = staffAccount.FinesseAutoLogin;
                        staffAccount.AcdId = dialog.agentIdText.Text.Trim();
                        staffAccount.AcdPassword = dialog.passwordText.Password.Trim();
                        staffAccount.Extension = dialog.extensionText.Text.Trim();
                        staffAccount.FinesseAutoLogin = (bool)dialog.chkRemember.IsChecked;
                        FinesseAgentID = string.IsNullOrEmpty(staffAccount.AcdId) ? "" : staffAccount.AcdId;
                        FinesseAgentExtension = int.Parse(staffAccount.Extension);
                        InteractionProvider.Credentials.Password = staffAccount.AcdPassword;
                        FinesseConnectionStatus = String.Format("Finesse Agent {0}: Connecting...", InteractionProvider.Credentials.AgentID);
                        this.CurrentAgentState = StandardAgentStates.Connecting;
                        staffAccount.FinesseIP = MediaBarAddIn._FinesseDomain;
                        //staffAccount.FinesseIP = "devuccx.vu.edu.au";
                        //staffAccount.FinesseIP = "hq-uccx.abc.inc";
                        OnPropertyChanged("FinesseConnectionStatus");

                        if ((bool)dialog.chkRemember.IsChecked == true || (bool)dialog.chkRemember.IsChecked == false && previousCheckedState == true)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                _objectProvider.UpdateStaffAccountInformation(staffAccount);
                            });
                        } else
                        {
                            //If checkbox is unchecked and if 
                        }

                        staffAccount.FinesseAutoLogin = (bool)dialog.chkRemember.IsChecked;

                        Task.Factory.StartNew(() =>
                        {
                            if(finesse.fnConnect(staffAccount.FinesseIP, 3) == 0)
                            {
                                if(finesse.fnLogin(staffAccount.AcdId, staffAccount.AcdPassword, staffAccount.Extension, "5000") < 0)
                                {
                                    Application.Current.Dispatcher.BeginInvoke(
                                        DispatcherPriority.Background,
                                        new Action(() =>
                                        {
                                            resetMediaBar();
                                            MessageBox.Show(Messages.MessageCannotSubscribeToFinesse, Messages.MessageBoxTitle);
                                            
                                        })
                                    );
                                } else
                                {
                                    //After successful login adding the list of notready agent state to the list of agent states
                                    notReadyStates = finesse.getNotReadyReasonCodes();
                                    foreach (var item in notReadyStates)
                                    {
                                        AgentStates.Add(new AgentState
                                        {
                                            AgentSelectable = false,
                                            Code = item.Value.Code,
                                            Description = item.Value.Label,
                                            Id = Convert.ToInt32(item.Key),
                                            IsOutboundEnabled = false,
                                            Name = "Not Ready",
                                            SwitchMode = AgentSwitchMode.Dynamic,
                                        });
                                    }

                                    if(_firstNotReadystate != null && _firstNotReadystate != "")
                                    {
                                        //For a tricky situation when we get the agent status too soon before we popluate the list items.
                                        CurrentAgentState = AgentStates.Where(p => p.Id == Convert.ToInt32(_firstNotReadystate) && p.SwitchMode == AgentSwitchMode.Dynamic).FirstOrDefault();
                                        if (CurrentAgentState == null)
                                        {
                                            Application.Current.Dispatcher.BeginInvoke(
                                            DispatcherPriority.Background,
                                            new Action(() =>
                                                {
                                                    //PK: Should not be the case.
                                                    CurrentAgentState = StandardAgentStates.NotReady;
                                                })
                                            );
                                        }
                                    }
                                }
                            } else
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Background,
                                    new Action(() =>
                                    {
                                        resetMediaBar();
                                        MessageBox.Show(String.Format(Messages.MessageCannotConnectToFinesse, staffAccount.FinesseIP ), Messages.MessageBoxTitle);
                                     })
                                );
                            }
                        });
                    }
                    else
                    {
                        dialog.errLabel.Text = validationResult;
                        dialog.errLabel.Visibility = Visibility.Visible;
                        return;
                    }
                }

                dialog.Close();
            }, isQueueEnabled: false);

            dialog.ShowDialog();
        }

        private void associateContactCommand(Contact contact)
        {
            if (MessageBox.Show(String.Format(Messages.MessageContactReplace, contact.Name, _previousCallerNumber), Messages.MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if(!updateContactRecord(contact,_previousCallerNumber))
                {
                    MessageBox.Show(Messages.MessageContactReplaceFailure, Messages.MessageBoxTitle);
                }
            }
        }

        private bool unassociateContact()
        {
            try
            {
                if (_contact != null && !String.IsNullOrEmpty(_previousCallerNumber))
                {
                    if (_contact.PhMobile != null && _contact.PhMobile.Trim().Equals(_previousCallerNumber))
                    {
                        if (MessageBox.Show(String.Format(Messages.MessageUnassociateContact, "mobile", _contact.PhMobile), Messages.MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            _contact.PhMobile = "";
                            updateContactListAfterUnassoc();
                            SaveAndRefreshWorkspace();
                        }
                    }
                    else if (_contact.PhOffice != null && _contact.PhOffice.Trim().Equals(_previousCallerNumber))
                    {
                        if (MessageBox.Show(String.Format(Messages.MessageUnassociateContact, "office", _contact.PhOffice), Messages.MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            _contact.PhOffice = "";
                            updateContactListAfterUnassoc();
                            SaveAndRefreshWorkspace();
                        }
                    }
                    else if (_contact.PhHome != null && _contact.PhHome.Trim().Equals(_previousCallerNumber))
                    {
                        if (MessageBox.Show(String.Format(Messages.MessageUnassociateContact, "home", _contact.PhOffice), Messages.MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            _contact.PhHome = "";
                            updateContactListAfterUnassoc();
                            SaveAndRefreshWorkspace();
                        }
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show(Messages.MessageUnassociateContactFailure, Messages.MessageBoxTitle);
                Logger.Logger.Log.Error("RightNowObjectProvider:", ex);
                return false;
            }
            return true;
        }

        private bool webRTCCall()
        {
            WebRTCDial = true;
            ShowWebRTCDialOptions = true;
            ShowDialOptions = false;
            return true;
        }

        private bool sendSMS()
        {
            ShowSendSMSOptions = true;
            ShowWebRTCDialOptions = false;
            ShowDialOptions = false;
            return true;
        }

        private bool showAgentDialog()
        {
            var getAgentPhoneNumberDialog = new GetAgentPhoneNumber();
            ConsoleWindowHelper.SetupOwner(getAgentPhoneNumberDialog);
            if(staffAccount == null)
            {
                staffAccount = new StaffAccountInfo();
            }
            IList<long> ids = _objectProvider.GetContactIdFromAccountEmail((staffAccount.Email));
            if (ids.Count == 1)
            {
                staffAccount.Phone = _objectProvider.GetPhoneNumberFromContactID(ids[0]);
            }

            getAgentPhoneNumberDialog.chkRemember.IsChecked = true;
            getAgentPhoneNumberDialog.DataContext = new GetAgentPhoneNumberViewModel(result =>
            {
                if (result)
                {
                    GetAgentPhoneNumberViewModel model = (GetAgentPhoneNumberViewModel)getAgentPhoneNumberDialog.DataContext;
                    if (String.IsNullOrEmpty(model.DestinationNumber.Trim()))
                    {
                        getAgentPhoneNumberDialog.errLabel.Text = "Please enter your phone number.";
                        getAgentPhoneNumberDialog.errLabel.Visibility = Visibility.Visible;
                        return;
                    }

                    string rawAgentPhoneNumber = model.DestinationNumber.Trim();
                    rawAgentPhoneNumber = RightNowMediaBar.RemoveSpecialCharacters(rawAgentPhoneNumber);
                    if (checkNumberInInternationalFormat(rawAgentPhoneNumber) && checkForLockedNumber(rawAgentPhoneNumber))
                    {
                        getAgentPhoneNumberDialog.Close();
                        MessageBox.Show(String.Format(Messages.MessageNoPermissionToUseCountryCode, rawAgentPhoneNumber.Substring(1, 2)), Messages.MessageBoxTitle);
                    }
                    else
                    {                                               
                        bool remember = (bool)getAgentPhoneNumberDialog.chkRemember.IsChecked;
                        getAgentPhoneNumberDialog.Close();
                        if (remember)
                        {
                            if (String.IsNullOrEmpty(staffAccount.Email))
                            {
                                MessageBox.Show(Messages.MessageEmailNotAvailableInAccount, Messages.MessageBoxTitle);
                            }
                            else
                            {
                                if (ids.Count > 1)
                                {
                                    MessageBox.Show(String.Format(Messages.MessageMoreThen1ContactAvailable, staffAccount.Email), Messages.MessageBoxTitle);
                                }
                                else if (ids.Count == 0)
                                {
                                    MessageBox.Show(String.Format(Messages.MessageNoContactAvailable, staffAccount.Email), Messages.MessageBoxTitle);
                                }
                                else
                                {
                                    Task.Factory.StartNew(() =>
                                    {
                                        string processedAgentNumber = ""; bool isLandlineNumber;
                                        DoMagicOnNumber(rawAgentPhoneNumber, out processedAgentNumber, out isLandlineNumber);
                                        if (processedAgentNumber == null)
                                        {
                                            Application.Current.Dispatcher.BeginInvoke(
                                            DispatcherPriority.Background,
                                                new Action(() =>
                                                {
                                                    MessageBox.Show(String.Format(Messages.MessageCannotFormatNumber, rawAgentPhoneNumber), Messages.MessageBoxTitle);
                                                }
                                                ));
                                        }
                                        else
                                        {
                                            if (_objectProvider.UpdateAccountContactPhone(processedAgentNumber, ids[0]) == false)
                                            {
                                                Application.Current.Dispatcher.BeginInvoke(
                                            DispatcherPriority.Background,
                                                new Action(() =>
                                                {
                                                    MessageBox.Show(Messages.MessageCannotUpdatePhone, Messages.MessageBoxTitle);
                                                }
                                             ));
                                            }
                                            else
                                            {
                                                if (remember)
                                                {
                                                    staffAccount.Phone = processedAgentNumber;
                                                }
                                            }
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                getAgentPhoneNumberDialog.Close();
            }, staffAccount.Phone);
            getAgentPhoneNumberDialog.ShowDialog();
            return true;
        }

        private bool showSendSMSDialog(Contact incontact)
        {
            ShowSendSMSOptions = false; bool OptOutOverRide = false;
            if (incontact != null && _objectProvider.IsContactHasOptOutFlagSet(incontact.Id))
            {
                OptOutOverRide = true;
                //if (MessageBox.Show(Messages.MessageContactSelectedOptOut, Messages.MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                //{
                   
                //    //Task.Factory.StartNew(() =>
                //    //{
                //    //    if (_objectProvider.ChangeOptOutFlag(Convert.ToInt32(incontact.Id), false) == false)
                //    //    {
                //    //        Application.Current.Dispatcher.BeginInvoke(
                //    //        DispatcherPriority.Background,
                //    //        new Action(() =>
                //    //        {
                //    //            MessageBox.Show(Messages.MessageCannotChangeOptOutFlag, Messages.MessageBoxTitle);
                //    //        }));
                //    //    }
                //    //});
                //}
                //else
                //{
                //    return false;
                //}
            }
            Contact destContact = null;

            if (incontact != null)
            {
                destContact = new Contact
                {
                    Name = incontact.Name,
                    Category = incontact.Category,
                    Description = incontact.Description,
                    Id = incontact.Id,
                    Number = incontact.Number,
                    TransferType = incontact.TransferType,
                    Type = incontact.Type
                };              
            }

            var SMSDialog = new SmsSenderView();
            SMSDialog.ResizeMode = ResizeMode.CanResizeWithGrip;
            ConsoleWindowHelper.SetupOwner(SMSDialog);
            SMSDialog.DataContext = new SmsSenderViewModel(destContact, (result, contact) =>
            {
                SmsSenderViewModel model = (SmsSenderViewModel)SMSDialog.DataContext;
                if (contact != null)
                {
                    if(OptOutOverRide && !model.OptOutOverride)
                    {
                        model.ShowErrorLabel = true;
                        SMSDialog.errLabel.Content = "Cannot send message as contact is set to SMS opt out.";
                        return;
                    }
                    if (String.IsNullOrEmpty(SMSDialog.txtDestinationNumber.Text))
                    {
                        model.ShowErrorLabel = true;
                        SMSDialog.errLabel.Content = "Please enter the destination number.";
                        return;
                    }
                    else if (String.IsNullOrEmpty(SMSDialog.txtMessage.Text))
                    {
                        model.ShowErrorLabel = true;
                        SMSDialog.errLabel.Content = "Please enter some message.";
                        return;
                    }

                    Logger.Logger.Log.Debug("fnSendSMS ** call fnSendSMS() **");
                    try
                    {
                        string rawDestContactNumber = "";
                        String processedContactDestinationNumber = ""; bool isLandlineNumber;
                        if (destContact != null)
                        {
                            rawDestContactNumber = destContact.Number.Trim();
                        } else
                        {
                            rawDestContactNumber = SMSDialog.txtDestinationNumber.Text.Trim();
                        }

                        if (checkNumberInInternationalFormat(rawDestContactNumber) && checkForLockedNumber(rawDestContactNumber))
                        {
                            MessageBox.Show(String.Format(Messages.MessageNoPermissionToUseCountryCode, rawDestContactNumber.Substring(1, 2)), Messages.MessageBoxTitle);
                        }
                        else
                        {
                            DoMagicOnNumber(rawDestContactNumber, out processedContactDestinationNumber, out isLandlineNumber);

                            if (processedContactDestinationNumber == null)
                            {
                                MessageBox.Show(String.Format(Messages.MessageCannotFormatNumber, rawDestContactNumber), Messages.MessageBoxTitle);
                                return;
                            }
                            else
                            {
                                if (isLandlineNumber || processedContactDestinationNumber.Substring(3, 1).Equals(MediaBarAddIn._LocalAreaCode))
                                {
                                    MessageBox.Show(String.Format(Messages.MessageCannotSendSMSToNumber, rawDestContactNumber), Messages.MessageBoxTitle);
                                    return;
                                }
                                //else
                                //{
                                //    destContact.Number = processedContactDestinationNumber;
                                //}
                            }
                        }

                        if (_objectProvider.CreateSMSInteraction(contact.Id, RightNowGlobalContext.AccountId.ToString(), processedContactDestinationNumber, _openIncidentID, SMSDialog.txtMessage.Text.Trim(), model.AlphaNumericHeader, model.OptOutOverride) == false)
                        {
                            MessageBox.Show(Messages.MessageSMSSendFailureInteraction, Messages.MessageBoxTitle);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Log.DebugFormat("fnSendSMS Exception :::::: ", ex.Message.ToString());
                        MessageBox.Show(Messages.MessageSMSSendFailure, Messages.MessageBoxTitle);
                    }
                }
                SMSDialog.Close();
            }, _objectProvider, RightNowGlobalContext.AccountId);
            SMSDialog.ShowDialog();
            return true;
        }

        private void ValidateAndDial(Contact contact)
        {
            string error = ""; string dialNumber = "";
            if (ValidateNumber(contact, out dialNumber, out error))
            {
                Dial(contact, dialNumber);
            }
            else
            {
                MessageBox.Show(error,Messages.MessageBoxTitle);
            }
        }

        private bool ValidateNumber(Contact contact, out string dialNumber, out string errMsg)
        {
            errMsg = ""; dialNumber = contact.Number;

            if (string.IsNullOrEmpty(dialNumber) || dialNumber.Length < 4)
            {
                errMsg = "Not a valid number.";
                return false;
            }
            if (dialNumber.Length == 4)
            {
                //Extension Number. Do Nothing.
            }
            else
            {
                string pattern = @"[\"+MediaBarAddIn._InternationalPrefix+"]{1}";
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                Match match = regex.Match(dialNumber);
                if (match.Success)
                {
                    //Valid Interational Format Number. Proceed with Dial. Do not do anything.
                }
                else
                {
                    //Prefix 0 to Dial number.
                    dialNumber = string.Format("{0}{1}", MediaBarAddIn._OutsidePrefix,dialNumber);
                }
            }
            return true;
        }


        /// <summary>
        /// Initiates an outbound dial request.
        /// </summary>
        /// <param name="contact">The contact to be dialed.</param>
        private void Dial(Contact contact, string contactNumber, bool checkContactNumber = true, bool dialledUsingDialPad = false)
        {
            bool AgentCancelledGetPhoneOption = false;
            ShowWebRTCDialOptions = false;

            if (WebRTCDial)
            {
                WebRTCDial = false;
                string studentID = "";
                if (contact.Id != null)
                {
                    studentID = _objectProvider.GetStudentIdFromContact(contact.Id);
                }
                
                //It could be null. Because, the background thread started from OnImportSatisfied might not had been completed. Thus we might not have the staffAccount object.
                if (staffAccount == null)
                {
                    staffAccount = _objectProvider.GetStaffAccountInformation(RightNowGlobalContext.AccountId);
                }

                //If it is still null. Create a new object
                if (staffAccount == null)
                {
                    staffAccount = new StaffAccountInfo();
                }

                if (String.IsNullOrEmpty(staffAccount.Email))
                {
                    MessageBox.Show(Messages.MessageEmailNotAvailableInAccount, Messages.MessageBoxTitle);
                }
                else
                {
                    IList<long> ids = _objectProvider.GetContactIdFromAccountEmail((staffAccount.Email));
                    long AgentContactID = 0;
                    if (ids.Count == 1)
                    {
                        AgentContactID = ids[0];
                    }
                    //else
                    String rawAgentPhoneNumber = ""; bool rememberAgentNumber = false;
                    string rawDestContactNumber = contact.Number.Trim();
                    if (String.IsNullOrEmpty(staffAccount.Phone))
                    {

                        var getAgentPhoneNumberDialog = new GetAgentPhoneNumber();
                        ConsoleWindowHelper.SetupOwner(getAgentPhoneNumberDialog);
                        //getAgentPhoneNumberDialog.txtPhoneNumber.Text = staffAccount.Phone;
                        getAgentPhoneNumberDialog.DataContext = new GetAgentPhoneNumberViewModel(result =>
                        {
                            if (result)
                            {
                                GetAgentPhoneNumberViewModel model = (GetAgentPhoneNumberViewModel)getAgentPhoneNumberDialog.DataContext;
                                if (String.IsNullOrEmpty(model.DestinationNumber.Trim()))
                                {
                                    getAgentPhoneNumberDialog.errLabel.Text = "Please enter your phone number.";
                                    getAgentPhoneNumberDialog.errLabel.Visibility = Visibility.Visible;
                                    return;
                                }

                                rawAgentPhoneNumber = model.DestinationNumber.Trim();
                                rawAgentPhoneNumber = RightNowMediaBar.RemoveSpecialCharacters(rawAgentPhoneNumber);    
                                if (checkNumberInInternationalFormat(rawAgentPhoneNumber) && checkForLockedNumber(rawAgentPhoneNumber))
                                {
                                    MessageBox.Show(String.Format(Messages.MessageNoPermissionToUseCountryCode, rawAgentPhoneNumber.Substring(1, 2)), Messages.MessageBoxTitle);
                                    getAgentPhoneNumberDialog.Close();
                                    return;
                                }
                                else
                                {
                                    rememberAgentNumber = (bool)getAgentPhoneNumberDialog.chkRemember.IsChecked;
                                    getAgentPhoneNumberDialog.Close();
                                }
                            }
                            else
                            {
                                AgentCancelledGetPhoneOption = true;
                            }
                            getAgentPhoneNumberDialog.Close();

                        }, staffAccount.Phone);
                        getAgentPhoneNumberDialog.ShowDialog();
                    }
                    else
                    {
                        rawAgentPhoneNumber = staffAccount.Phone;
                    }

                    if (!AgentCancelledGetPhoneOption)
                    {

                        if (checkNumberInInternationalFormat(rawAgentPhoneNumber) && checkForLockedNumber(rawAgentPhoneNumber))
                        {
                            MessageBox.Show(String.Format(Messages.MessageNoPermissionToUseCountryCode, rawAgentPhoneNumber.Substring(1, 2)), Messages.MessageBoxTitle);
                        }
                        else if (checkNumberInInternationalFormat(rawDestContactNumber) && checkForLockedNumber(rawDestContactNumber))
                        {
                            MessageBox.Show(String.Format(Messages.MessageNoPermissionToUseCountryCode, rawDestContactNumber.Substring(1, 2)), Messages.MessageBoxTitle);
                        }
                        else
                        {
                            String processedAgentPhoneNumber, processedContactDestinationNumber; bool isAgentNoALandlineNumber; bool ContactNoALandlineNumber;

                            DoMagicOnNumber(rawAgentPhoneNumber, out processedAgentPhoneNumber, out isAgentNoALandlineNumber);
                            DoMagicOnNumber(rawDestContactNumber, out processedContactDestinationNumber, out ContactNoALandlineNumber);

                            if (processedContactDestinationNumber == null)
                            {
                                MessageBox.Show(String.Format(Messages.MessageCannotFormatNumber, rawDestContactNumber), Messages.MessageBoxTitle);
                            }
                            else if (processedAgentPhoneNumber == null)
                            {
                                MessageBox.Show(String.Format(Messages.MessageCannotFormatNumber, rawAgentPhoneNumber), Messages.MessageBoxTitle);
                            }
                            else
                            {
                                if (dialledUsingDialPad)
                                {
                                    _previousCallerNumber = processedContactDestinationNumber;
                                    hideUnhideAssocButtons();
                                }

                                string customerDomain = _customerDomain; string uname = ""; string pwd = ""; string url = "";
                                if (String.IsNullOrEmpty(customerDomain))
                                {
                                    _objectProvider.GetCustomerDomainFromBLAccount( out uname, out pwd, out url);
                                    _BLAccountPassword = pwd;
                                    _BLAccountUsername = uname;
                                    _BLAccountURL = url;
                                } 
                                string fName="", lName = "";
                                _objectProvider.GetFirstNameLastNameFromContact((int)AgentContactID, out fName, out lName);

                                string token = "";
                                getTokenRequest(String.Format("{0}{1}", _BLAccountURL, "/api/v1/token/"), _BLAccountUsername, _BLAccountPassword, out token);

                                MakeWebRTCCall(processedAgentPhoneNumber, processedContactDestinationNumber, /*RightNowGlobalContext.AccountId.ToString()*/AgentContactID, contact.Id, studentID, System.Uri.EscapeUriString(String.Format("{0} {1}", fName, lName)), token);
                                if (rememberAgentNumber)
                                {
                                    if (ids.Count > 1)
                                    {
                                        MessageBox.Show(String.Format(Messages.MessageMoreThen1ContactAvailable, staffAccount.Email), Messages.MessageBoxTitle);
                                    }
                                    else if (ids.Count == 0)
                                    {
                                        MessageBox.Show(String.Format(Messages.MessageNoContactAvailable, staffAccount.Email), Messages.MessageBoxTitle);
                                    }
                                    else
                                    {
                                        Task.Factory.StartNew(() =>
                                        {
                                            if (_objectProvider.UpdateAccountContactPhone(processedAgentPhoneNumber, ids[0]) == false)
                                            {
                                                Application.Current.Dispatcher.BeginInvoke(
                                            DispatcherPriority.Background,
                                                new Action(() =>
                                                {
                                                    MessageBox.Show(Messages.MessageCannotUpdatePhone, Messages.MessageBoxTitle);
                                                }
                                             ));
                                            }
                                            else
                                            {
                                                if (rememberAgentNumber)
                                                {
                                                    staffAccount.Phone = processedAgentPhoneNumber;
                                                }
                                            }
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    _agentInCall = false;
                    if (finesse.fnMakeCall(contactNumber.Trim()) == 0)
                    {
                        _agentInCall = true;
                    }
                });
            }
        }

        private string Validate(AgentLogin dialog)
        {
            string result = string.Empty;
            string missingFields = string.Empty;
            if (dialog.extensionText.Text.Trim() == "")
            {
                if (missingFields == string.Empty)
                {
                    missingFields = "Extension";
                }
                else
                {
                    missingFields = missingFields + "," + "Extension";
                }
            }

            if (dialog.agentIdText.Text.Trim() == "")
            {
                if (missingFields == string.Empty)
                {
                    missingFields = "Agent Id";
                }
                else
                {
                    missingFields = missingFields + "," + "Agent Id";
                }
            }

            if (dialog.passwordText.Password.Trim() == "")
            {
                if (missingFields == string.Empty)
                {
                    missingFields = "Password";
                }
                else
                {
                    missingFields = missingFields + "," + "Password";
                }
            }

            if (missingFields != string.Empty)
            {
                result = "Missing field(s): " + missingFields;
            }

            return result;
        }

        private string ValidateCampaignDialog(CreateCampaignViewModel dialog)
        {
            string result = string.Empty;
            string missingFields = string.Empty;
            if (String.IsNullOrEmpty(dialog.CampaignName))
            {
                if (missingFields == string.Empty)
                {
                    missingFields = "Message Campaign Name";
                }
                else
                {
                    missingFields = missingFields + "," + "Message Campaign Name";
                }
            }

            if (String.IsNullOrEmpty(dialog.UserInput))
            {
                if (missingFields == string.Empty)
                {
                    missingFields = "Message Body";
                }
                else
                {
                    missingFields = missingFields + "," + "Message Body";
                }
            }

            if (dialog._ContactListIDInt == 0)
            {
                if (missingFields == string.Empty)
                {
                    missingFields = "Input ID (Try clicking the Fetch Button)";
                }
                else
                {
                    missingFields = missingFields + "," + "Input ID (Try clicking the Fetch Button)";
                }
            }

            if (missingFields != string.Empty)
            {
                result = "Missing field(s): " + missingFields;
            }

            return result;
        }

        public void MakeWebRTCCall(string agentnumber, string studentnumber, long agentContactID, string contactid, string studentid, string agentName, string token)
        {
            Logger.Logger.Log.Debug("fnMakeWebRTCCall call fnMakeWebRTCCall() **");

            try
            {
                var url = String.Format("{0}?Caller={1}&Recipient={2}&CallerID={3}&ContactID={4}&StudentID={5}&IncidentID={6}&AccountID={7}&CallerName={8}&Authorization={9}&CallSource=WebRTC%20Media%20Bar", WebRTCURL, agentnumber, studentnumber, agentContactID != 0 ? agentContactID.ToString() : String.Empty, contactid, studentid, _openIncidentID == 0 ? String.Empty : _openIncidentID.ToString(), RightNowGlobalContext.AccountId, agentName, token);
                Logger.Logger.Log.Debug(String.Format("fnMakeWebRTCCall URL :::::: {0}", url));
                if(_webRTCContentWindow != null)
                {
                    RightNowGlobalContext.AutomationContext.CloseEditor(_webRTCContentWindow.UniqueID);
                }
                _webRTCContentWindow = new WebRTCBrowserContentPane(url);   //browser control        
                RightNowGlobalContext.AutomationContext.OpenEditor(_webRTCContentWindow);
                //calling content pane here
                //var req = (HttpWebRequest)WebRequest.Create(url);
                //req.Timeout = Timeout.Infinite;
                //req.KeepAlive = true;
                //WebResponse res = req.GetResponse();
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.DebugFormat("fnMakeWebRTCCall Exception :::::: ", ex.Message.ToString());
                MessageBox.Show(Messages.MessageWebRTCConnectFailure, Messages.MessageBoxTitle);
            }
        }
        public void GetEventOnCallWrapUp(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table)
        {
            Logger.Logger.Log.Info("GetEventOnCallWrapup");
        }

        public void GetEventOnCallHeld(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table)
        {

        }

        public void GetEventOnCallInitiating(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table)
        {

        }

        public void GetEventOnCallInitiated(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table)
        {

        }

        public void GetEventOnCallFailed(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table)
        {

        }
        public void GetEventOnPassCheck(string ret, string data)
        {

        }


        public void GetEventOnCallActive(Event evt)
        {
            Logger.Logger.Log.Info("GetEventOnCallActive" + evt.getEvtMsg());
        }

        public void GetEventOnCallAlerting(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table)
        {
            Application.Current.Dispatcher.BeginInvoke(
                     DispatcherPriority.Background,
                     new Action(() =>
                     {
                         //PK: Screen popup of contact. Does not work in a Thread.
                         Hashtable callVariables = table;
                         string studentId = callVariables["callVariable2"].ToString();
                         string fromphoneNumber = callVariables["callVariable4"].ToString();
                         if(String.IsNullOrEmpty(fromphoneNumber))
                         {
                             fromphoneNumber = fromAddress;
                         }

                         string outboundPreview = "";
                         int reportId = 109823;
                         try
                         {
                             reportId = Convert.ToInt32(MediaBarAddIn._reportID);
                         }
                         catch (Exception ex){}

                         try
                         {
                             Logger.Logger.Log.Info("GetEventOnCallAlerting > Trying to get Outbound Preview Calls");
                             outboundPreview = callVariables["callVariable6"].ToString();
                             Logger.Logger.Log.Info(String.Format("Got the Outbound preview call string as {0}", outboundPreview));

                         }
                         catch (Exception ex){
                             Logger.Logger.Log.Info("Exception getting outbound preview string");
                         }

                         if (!String.IsNullOrEmpty(outboundPreview) && outboundPreview.ToLower().Contains(MediaBarAddIn._OutboundCallVariableString))
                         {
                             //Do nothing with the incomming number.
                             Logger.Logger.Log.Info("This is an outbound call. Do not remove the 0 from an incoming number.");
                         }
                         else {
                             if (!string.IsNullOrEmpty(fromphoneNumber))
                             {
                                 Logger.Logger.Log.Info("This not an outbound call. Remove the 0 from an incoming number.");
                                 string pattern = @"[\" + MediaBarAddIn._InternationalPrefix + "]{1}";
                                 Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                                 Match match = regex.Match(fromphoneNumber);
                                 if (match.Success)
                                 {
                                     //Valid Interational Format Number. Do not do anything.
                                 }
                                 else
                                 {
                                     //Delete leading 0 from the Dial Number
                                     if (fromphoneNumber.Substring(0, 1).Equals("0"))
                                     {
                                         fromphoneNumber = fromphoneNumber.Substring(1);
                                     }
                                 }
                             }

                         }

                         long openedContactID = -1;
                         OpenContact(studentId, fromphoneNumber, reportId, out openedContactID);
                         if (openedContactID != -1 && MediaBarAddIn._CTICreateIncident)
                         {
                             long newIncidentID = -1;
                             _objectProvider.CreateIncident(openedContactID, out newIncidentID);
                             if(newIncidentID != -1 && MediaBarAddIn._AutoOpenIncidentForIncomingSMS)
                             {
                                 RightNowGlobalContext.AutomationContext.EditWorkspaceRecord(WorkspaceRecordType.Incident, newIncidentID);
                             }
                         }
                         Logger.Logger.Log.Info("GetEventOnCallAlerting" + callType);
                     })
                );
        }
        public void GetEventOnCallEstablished(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table)
        {

        }
        public void GetEventOnCallDropped(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table)
        {

        }

        public void GetEventOnAgentStateChange(string state, string reasonCode, string evtMessage)
        {
            _agentInCall = false;
            TempAgentState = StandardAgentStates.NotReady;
            if (state.Equals("NOT_READY"))
            {
                //Handling of Not-Ready States
                TempAgentState = StandardAgentStates.NotReady;
                //Reason code is not ready on the UI so store the reason code to update when the states are available.
                if (notReadyStates == null )
                {
                    if (!string.IsNullOrEmpty(reasonCode))
                    {
                        _firstNotReadystate = reasonCode;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(reasonCode))
                    {
                        TempAgentState = AgentStates.Where(p => p.Id == Convert.ToInt32(reasonCode) && p.SwitchMode == AgentSwitchMode.Dynamic).FirstOrDefault();
                        if (TempAgentState == null)
                        {
                            //Should not be the case.
                            TempAgentState = StandardAgentStates.NotReady;
                        }
                    }
                }
                FinesseConnectionStatus = String.Format("Finesse Agent {0}: Not-Ready.", InteractionProvider.Credentials.AgentID);
            }
            else if (state.Equals("READY"))
            {
                TempAgentState = StandardAgentStates.Available;
                FinesseConnectionStatus = String.Format("Finesse Agent {0}: Ready.", InteractionProvider.Credentials.AgentID);
            }
            else if (state.Equals("HOLD"))
            {
                TempAgentState = StandardAgentStates.Hold;
                FinesseConnectionStatus = String.Format("Finesse Agent {0}: Ready.", InteractionProvider.Credentials.AgentID);
            }
            else if (state.Equals("RESERVED"))
            {
                TempAgentState = StandardAgentStates.Reserved;
                FinesseConnectionStatus = String.Format("Finesse Agent {0}: Reserved.", InteractionProvider.Credentials.AgentID);
            }
            else if (state.Equals("WORK"))
            {
                TempAgentState = StandardAgentStates.WrapUp;
                FinesseConnectionStatus = String.Format("Finesse Agent {0}: WrapUp.", InteractionProvider.Credentials.AgentID);
            }

            else if (state.Equals("LOGOUT"))
            {
                TempAgentState = StandardAgentStates.LoggedOut;
                FinesseConnectionStatus = String.Format("Finesse Agent {0}: LoggedOut.", InteractionProvider.Credentials.AgentID);
            }
            else if (state.Equals("LOGIN"))
            {
                FinesseConnectionStatus = String.Format("Finesse Agent {0}: LoggedIn.", InteractionProvider.Credentials.AgentID);
                TempAgentState = StandardAgentStates.LoggedIn;                
            }
            else if (state.Equals("TALKING"))
            {
                _agentInCall = true;
                FinesseConnectionStatus = String.Format("Finesse Agent {0}: In Call (Talking).", InteractionProvider.Credentials.AgentID);
                TempAgentState = StandardAgentStates.Talking;
            }
            else
            {
                FinesseConnectionStatus = String.Format("Finesse Agent {0}: Not-Ready.", InteractionProvider.Credentials.AgentID);
                TempAgentState = StandardAgentStates.NotReady;
            }
            if (this.CurrentAgentState != TempAgentState)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        AgentState previousAgentState = this.CurrentAgentState;
                        _sendStateToFinesse = false;
                        this.CurrentAgentState = TempAgentState;
                        OnPropertyChanged("IsAgentLoggedIn");
                        OnPropertyChanged("FinesseConnectionStatus");
                        CanChangeState = CurrentAgentState.SwitchMode != AgentSwitchMode.LoggedOut;
                        if(CurrentAgentState == StandardAgentStates.LoggedOut)
                        {
							//PK: Can be removed for Auto Connect after Desktop Login.
                            resetMediaBar();
                            //Connecting failed. Probable reason not logged in from phone.
                            if (previousAgentState == StandardAgentStates.Connecting)
                            {
                                MessageBox.Show(Messages.MessageFinesseLoginMessage, Messages.MessageBoxTitle);
                            } else
                            {
                                MessageBox.Show(Messages.MessageFinesseSignedOut, Messages.MessageBoxTitle);
                            }
                        }
                    })
                );
            } 
        }

        public void GetEventOnCallError(string errorMessage)
        {

        }
        public void GetEventOnError(Event evt)
        {
            Application.Current.Dispatcher.BeginInvoke(
                   DispatcherPriority.Background,
                   new Action(() =>
                   {
                       resetMediaBar();
                       MessageBox.Show(Messages.MessageFinesseUnhandledCommunicationError + evt, Messages.MessageBoxTitle);
                   })
               );
        }

        public void GetEventOnConnection(string finesseIP, String evt)
        {
            Logger.Logger.Log.Info("GetEventOnConnection event" + evt);
        }

        public void GetEventOnDisConnection(string finesseIP, String evt)
        {
            Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        this.CurrentAgentState = StandardAgentStates.LoggedOut;
                        OnPropertyChanged("IsAgentLoggedIn");
                        CanChangeState = CurrentAgentState.SwitchMode != AgentSwitchMode.LoggedOut;
                    })
                );
            Logger.Logger.Log.Info("GetEventOnDisConnection " + finesseIP + " event" + evt);
        }
        public void GetEventOnFinesseConnectionProblem(Event evt)
        {
            Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        this.CurrentAgentState = StandardAgentStates.LoggedOut;
                        OnPropertyChanged("IsAgentLoggedIn");
                        CanChangeState = CurrentAgentState.SwitchMode != AgentSwitchMode.LoggedOut;
                        MessageBox.Show(Messages.MessageFinesseCommunicationError, Messages.MessageBoxTitle);
                    })
                );
            Logger.Logger.Log.Info("GetEventOnFinesseConnectionProblem event" + evt);
        }


        public void GetEventOnFinesseConnectionProblem(string finesseIP)
        {
            Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        this.CurrentAgentState = StandardAgentStates.LoggedOut;
                        OnPropertyChanged("IsAgentLoggedIn");
                        CanChangeState = CurrentAgentState.SwitchMode != AgentSwitchMode.LoggedOut;
                        MessageBox.Show(Messages.MessageFinesseCommunicationError, Messages.MessageBoxTitle);
                    })
                );
            Logger.Logger.Log.Info("GetEventOnFinesseConnectionProblem event" + finesseIP);
        }

        public void WriteCallLog(LastInteraction interaction, bool isOutboundCall = true)
        {
            try
            {                
                Hashtable hashTable = new Hashtable();
                hashTable.Add("CV1", interaction.cv1);
                hashTable.Add("CV2", interaction.cv2);
                hashTable.Add("CV3", interaction.cv3);
                hashTable.Add("CV4", interaction.cv4);
                hashTable.Add("CV5", interaction.cv5);
                hashTable.Add("CV6", interaction.cv6);
                hashTable.Add("CV7", interaction.cv7);
                hashTable.Add("CV8", interaction.cv8);
                hashTable.Add("CV9", interaction.cv9);
                hashTable.Add("CV10", interaction.cv10);
                
                if (!String.IsNullOrEmpty(interaction.studentid))
                {
                    IList<long> listIds = _objectProvider.GetContactIdFromStudentId(interaction.studentid);
                    if (listIds.Count == 1)
                    {
                        hashTable.Add("Contact_id", listIds.FirstOrDefault().ToString());
                        hashTable.Add("Student_id", interaction.studentid);
                    } else
                    {
                        if (_inCallContact != null)
                        {
                            hashTable.Add("Contact_id", _inCallContact.ID.ToString());
                            string studentID = _objectProvider.GetStudentIdFromContact(_inCallContact.ID.ToString());
                            if (!string.IsNullOrEmpty(studentID))
                            {
                                hashTable.Add("Student_id", studentID);
                            }
                        } else
                        {
                            if(isOutboundCall && _contact != null)
                            {
                                hashTable.Add("Contact_id", _contact.ID.ToString());
                                string studentID = _objectProvider.GetStudentIdFromContact(_contact.ID.ToString());
                                if (!string.IsNullOrEmpty(studentID))
                                {
                                    hashTable.Add("Student_id", studentID);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_inCallContact != null)
                    {
                        hashTable.Add("Contact_id", _inCallContact.ID.ToString());
                        string studentID = _objectProvider.GetStudentIdFromContact(_inCallContact.ID.ToString());
                        if (!string.IsNullOrEmpty(studentID))
                        {
                            hashTable.Add("Student_id", studentID);
                        }
                    }
                    else if (isOutboundCall && _contact != null)
                    {
                        hashTable.Add("Contact_id", _contact.ID.ToString());
                        string studentID = _objectProvider.GetStudentIdFromContact(_contact.ID.ToString());
                        if (!string.IsNullOrEmpty(studentID))
                        {
                            hashTable.Add("Student_id", studentID);
                        }
                    } else 
                    {
                        hashTable.Add("Contact_id", "");
                    }
                }

                if(_inCallContact != null)
                {
                    _inCallContact = null;
                }

                _agentInCall = false;
                hashTable.Add("Call_id", interaction.dialogid);
                hashTable.Add("CallStatus", interaction.status);
                hashTable.Add("Start", interaction.starttime);
                hashTable.Add("Finish", interaction.finishtime);
                hashTable.Add("IncomingNumber", interaction.fromnumber);

               if (_inCallOpenIncidentID != 0)
                {
                    hashTable.Add("IncidentId", _inCallOpenIncidentID);
                } else
                {
                    hashTable.Add("IncidentId", _openIncidentID);
                }

                _inCallOpenIncidentID = 0;
                if (!String.IsNullOrEmpty(interaction.tonumber))
                {
                    if(interaction.tonumber.Length > 15)
                    {
                        interaction.tonumber = interaction.tonumber.Substring(5);
                    }
                }

                IList<long> ids = _objectProvider.GetContactIdFromAccountEmail((staffAccount.Email));
                int AgentContactID = 0;
                if (ids.Count == 1)
                {
                    AgentContactID = (int)ids[0];
                    string fName, lName;
                    if (_objectProvider.GetFirstNameLastNameFromContact(AgentContactID, out fName, out lName))
                    {
                        hashTable.Add("Agent", String.Format("{0} {1}", fName, lName));
                    }
                }

                hashTable.Add("OutgoingNumber", interaction.tonumber);
                DateTime dt1 = DateTime.Parse(interaction.starttime);
                DateTime dt2 = DateTime.Parse(interaction.finishtime);
                TimeSpan span = dt2 - dt1;
                int ms = (int)span.TotalSeconds;
                hashTable.Add("Duration", ms);
                _objectProvider.CreateInteractionLogObject(hashTable);
            } catch (Exception ex)
            {
                Logger.Logger.Log.Info("WriteCallLog" + ex.Message.ToString());
            }
        }

        private bool checkNumberInInternationalFormat(string destContactNumber)
        {
            string pattern = @"[\" + MediaBarAddIn._InternationalPrefix + "]{1}";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(destContactNumber);
            if (match.Success)
            {
                return true;
            }
            return false;
        }

        private bool checkForLockedNumber(string destContactNumber)
        {
            //Check the number against the Local Country Code
            if (MediaBarAddIn._LockInternationalCalls && !destContactNumber.Substring(1, 2).Equals(MediaBarAddIn._LocalCountryCode))
            {
                return true;
            }
            return false;
        }


        private bool DoMagicOnNumber(string destNumber, out string processedNumber, out bool isLandLineNumber)
        {
            bool retVal = true; processedNumber = null; isLandLineNumber = false;
            string cc = util.GetRegionCodeForCountryCode(Convert.ToInt32(MediaBarAddIn._LocalCountryCode));
            string destContactNumber = destNumber;
            if(destContactNumber.Substring(0, 4).Equals("0011"))
            {
                destContactNumber = String.Format("+{0}", destContactNumber.Substring(5));
            }
            else if(destContactNumber.Substring(0, 1).Equals("+"))
            {
                //Use as it is
                //destContactNumber = String.Format("+{0}", destContactNumber.Substring(5));
            }
            else if (destContactNumber.Length >= 11  && !destContactNumber.Substring(0, 1).Equals("+"))
            {
                //Add a a plus.
                destContactNumber = String.Format("+{0}", destContactNumber);
            }
            else if ((destContactNumber.Length == 10) && (destContactNumber.Substring(0, 2).Equals("04") || destContactNumber.Substring(0, 2).Equals("05")))
            {
                //Add LocalCountryCode & remove leading zero
                destContactNumber = String.Format("{0}{1}", MediaBarAddIn._LocalCountryCode, destContactNumber.Substring(1));
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else if ((destContactNumber.Length == 10) && (destContactNumber.Substring(0, 2).Equals("02") || destContactNumber.Substring(0, 2).Equals("03") ||
                destContactNumber.Substring(0, 2).Equals("07") || destContactNumber.Substring(0, 2).Equals("08")))
            {
                isLandLineNumber = true;
                //Add LocalCountryCode & remove leading zero
                destContactNumber = String.Format("{0}{1}", MediaBarAddIn._LocalCountryCode, destContactNumber.Substring(1));
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else if ((destContactNumber.Length == 9) && (destContactNumber.Substring(0, 1).Equals("4")))
            {
                //Perhaps just leading zero left off mobile - Add LocalCountryCode and see if it works -> 61 4 12345678
                //Add LocalCountryCode & remove leading zero
                destContactNumber = String.Format("{0}{1}", MediaBarAddIn._LocalCountryCode, destContactNumber);
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }

            }
            else if ((destContactNumber.Length == 9) && (destContactNumber.Substring(0, 1).Equals("2") || destContactNumber.Substring(0, 1).Equals("3") ||
                destContactNumber.Substring(0, 1).Equals("7") || destContactNumber.Substring(0, 1).Equals("8")))
            {
                isLandLineNumber = true;
                //Probably just leading zero left off - Add LocalCountryCode and see if it works -> 61 123456789
                destContactNumber = String.Format("{0}{1}", MediaBarAddIn._LocalCountryCode, destContactNumber);
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else if ((destContactNumber.Length == 9))
            {
                //Same as above but here for simplicity
                destContactNumber = String.Format("{0}{1}", MediaBarAddIn._LocalCountryCode, destContactNumber);
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else if ((destContactNumber.Substring(0, 1).Equals(MediaBarAddIn._TrunkAccessCode))) {
                destContactNumber = String.Format("{0}{1}", MediaBarAddIn._LocalCountryCode, destContactNumber.Substring(1));
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else
            {
                if (destContactNumber.Length == 8)
                {
                    isLandLineNumber = true;
                    destContactNumber = String.Format("{0}{1}{2}", MediaBarAddIn._LocalCountryCode, MediaBarAddIn._LocalAreaCode, destContactNumber);
                    try
                    {
                        var e164contactDestNumber = util.Parse(destContactNumber, cc);
                        destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        Logger.Logger.Log.Debug("Exception while converting LL number to E164", ex);
                    }
                } else if (destContactNumber.Length == 10 && MediaBarAddIn._LocalCountryCode == "1")
                {
                    //10 Digit Number Handling for US.
                    isLandLineNumber = false;
                    destContactNumber = String.Format("+{0}{1}", MediaBarAddIn._LocalCountryCode, destContactNumber);
                    //try
                    //{
                    //    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    //    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                    //}
                    //catch (Exception ex)
                    //{
                    //    retVal = false;
                    //    Logger.Logger.Log.Debug("Exception while converting LL number to E164", ex);
                    //}
                }
                else
                {
                    try
                    {
                        var e164contactDestNumber = util.Parse(destContactNumber, cc);
                        destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        Logger.Logger.Log.Debug("Exception while converting any number E164", ex);
                    }
                }
            }
            if (retVal)
            {
                processedNumber = destContactNumber;
            }
            return retVal;
        }
        public int getTokenRequest(string serverURL, string username, string password, out string token)
        {
            token = "";
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverURL);
                request.Method = "GET";

                string basicEncode = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));

                request.Headers.Add("Authorization", "Basic " + basicEncode);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                int code = Convert.ToInt32(response.StatusCode);


                Stream webStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(webStream);

                string responseStr = reader.ReadToEnd();

                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                WebRTCToken d = json_serializer.Deserialize<WebRTCToken>(responseStr);

                token = d.token;
                reader.Close();

            }
            catch (Exception e)
            {
                Logger.Logger.Log.Debug("getTokenRequest", e);
                return -1;
            }

            return 0;
        }
        public bool GetAccountDetailsFromServer(string BLAccoundID, string token, out BlueLeapUserAccount userAccDetails)
        {
            bool retVal = true; userAccDetails = null;
            try
            {
                string serverURL = String.Format("{0}{1}{2}/", _BLAccountURL, "/api/v1/getuseraccount/", BLAccoundID);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverURL);
                request.Method = "GET";
                request.Headers.Add("Authorization", token);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                int code = Convert.ToInt32(response.StatusCode);


                Stream webStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(webStream);

                string responseStr = reader.ReadToEnd();

                JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                userAccDetails = json_serializer.Deserialize<BlueLeapUserAccount>(responseStr);

                reader.Close();

            }
            catch (Exception e)
            {
                Logger.Logger.Log.Debug("GetAccountDetailsFromServer:", e);
                retVal = false;
            }
            return retVal;
        }
    }
}