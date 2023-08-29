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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Oracle.RightNow.Cti.Model;
using RightNow.AddIns.AddInViews;
using RightNow.AddIns.Common;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Oracle.RightNow.Cti.MediaBar {
    public class RightNowMediaBar : NotifyingObject, IPartImportsSatisfiedNotification {
        protected RightNowObjectProvider _objectProvider;
        protected AgentState _currentAgentState; //PK: Changed to Protected
        private int _enabledButtons;

        private bool _canAssociateToRecord;
        private bool _canMakeCall;
        private bool _canAssociateContact;
        private bool _canUnAssociateContact;
        private InteractionManager _interactionManager;
        
        //To hold current opened contact after incomming calls.
        protected IContact _contact;
        protected IIncident _incident;
        private IRecordContext _contactRecordContext;

        protected string _previousCallerNumber;

        protected string _reportID;

        protected string _oldReportTabIdentifier;

        protected int _openIncidentID;

        protected int _inCallOpenIncidentID;
        protected bool _agentInCall;
        protected IContact _inCallContact;

        public RightNowMediaBar() {
            Interactions = new ObservableCollection<IInteraction>();
            ContextContacts = new ObservableCollection<Contact>();
            AssocContextContacts = new ObservableCollection<Contact>();
            _reportID = "";
            _oldReportTabIdentifier = "";
            _openIncidentID = 0;
            _inCallOpenIncidentID = 0;
            _agentInCall = false;
            _inCallContact = null;
        }

        public SynchronizationContext SynchronizationContext { get; set; }

        [Import]
        internal IGlobalContext RightNowGlobalContext { get; set; }

        public IList<AgentState> AgentStates {
           get; set; 
        }

        //PK: Moved to Cti.MediaBar

        //public AgentState CurrentAgentState {
        //    get {
        //        return _currentAgentState;
        //    }
        //    set {
        //        if (setAgentState(value)) {
        //            InteractionProvider.Agent.SetState(value);
        //        }
        //    }
        //}

        public IInteraction CurrentInteraction {
            get {
                return InteractionProvider.CurrentInteraction;
            }
            set {
                InteractionProvider.CurrentInteraction = value;
            }
        }

        [Import]
        public InteractionManager InteractionManager {
            get {
                return _interactionManager;
            }
            set {
                _interactionManager = value;
                _interactionManager.SynchronizationContext = this.SynchronizationContext;
            }
        }

        public IInteractionProvider InteractionProvider {
            get {
                return InteractionManager.InteractionProvider;
            }
        }

        public ObservableCollection<IInteraction> Interactions {
            get;
            set;
        }

        public int EnabledButtons {
            get {
                return _enabledButtons;
            }
            set {
                _enabledButtons = value;
                OnPropertyChanged("EnabledButtons");
            }
        }

        public bool CanAssociateToRecord {
            get {
                return _canAssociateToRecord;
            }
            set {
                if (_canAssociateToRecord != value) {
                    _canAssociateToRecord = value;
                    OnPropertyChanged("CanAssociateToRecord");
                }
            }
        }

        public bool CanMakeCall {
            get {
                return _canMakeCall;
            }
            set {
                if (_canMakeCall != value) {
                    _canMakeCall = value;
                    OnPropertyChanged("CanMakeCall");
                }
            }
        }

        public bool CanAssociateContact
        {
            get
            {
                return _canAssociateContact;
            }
            set
            {
                if (_canAssociateContact != value)
                {
                    _canAssociateContact = value;
                    OnPropertyChanged("CanAssociateContact");
                }
            }
        }

        public bool CanUnAssociateContact
        {
            get
            {
                return _canUnAssociateContact;
            }
            set
            {
                if (_canUnAssociateContact != value)
                {
                    _canUnAssociateContact = value;
                    OnPropertyChanged("CanUnAssociateContact");
                }
            }
        }

        public void EnableButtons(MediaBarButtons buttons)
        {
            synchronizeAndInvoke(o => EnabledButtons |= (int)o, buttons);
        }

        public void DisableButtons(MediaBarButtons buttons) {
            synchronizeAndInvoke(o => EnabledButtons &= ~((int)o), buttons);
        }

        public virtual void OnImportsSatisfied() {
            if (InteractionManager != null) {
                // Interaction events
                InteractionProvider.CurrentInteractionChanged += CurrentInteractionChangedHandler;
                InteractionProvider.InteractionConnected += InteractionConnectedHandler;
                InteractionProvider.InteractionCompleted += InteractionCompletedHandler;
                InteractionProvider.InteractionDisconnected += InteractionDisconnectedHandler;
                InteractionProvider.NewInteraction += NewInteractionHandler;

                //Agent events
                InteractionProvider.Agent.StateChanged += agentStateChangedHandler;

                setAgentState(InteractionProvider.Agent.CurrentState);
            }
            _objectProvider = new RightNowObjectProvider(RightNowGlobalContext);
            if (RightNowGlobalContext != null) {
                RightNowGlobalContext.AutomationContext.CurrentEditorTabChanged += currentEditorTabChanged;
            }
        }

        private void currentEditorTabChanged(object sender, EditorTabChangedEventArgs e) {
            if(e.NewTabIdentifier != null && e.NewTabIdentifier.Contains(_reportID))
            {
                _oldReportTabIdentifier = e.NewTabIdentifier;
            }
            ContextContacts.Clear();
            AssocContextContacts.Clear();
            _incident = null;
            _openIncidentID = 0;
            _contact = null;
            WorkspaceRecordType? workspaceType = null;
            if (RightNowGlobalContext.AutomationContext.CurrentWorkspace != null)
                workspaceType = RightNowGlobalContext.AutomationContext.CurrentWorkspace.WorkspaceType;

            if (CurrentInteraction != null && workspaceType != null)
            {
                CanAssociateToRecord = workspaceType.Value == WorkspaceRecordType.Contact || workspaceType.Value == WorkspaceRecordType.Incident;
                CanAssociateContact = workspaceType.Value == WorkspaceRecordType.Contact || workspaceType.Value == WorkspaceRecordType.Incident;
                CanUnAssociateContact = workspaceType.Value == WorkspaceRecordType.Contact || workspaceType.Value == WorkspaceRecordType.Incident;
            }
            else
            {
                CanAssociateToRecord = false;
                CanAssociateContact = false;
                CanUnAssociateContact = false;
            }

            
            //For Incident workspace GetWorkspaceRecord as Icontact works all Good. So I will try to get an Icontact out of Incident to see if it works.

            if (workspaceType != null /*&& workspaceType.Value == WorkspaceRecordType.Contact*/)
            {
                var contact = RightNowGlobalContext.AutomationContext.CurrentWorkspace.GetWorkspaceRecord(WorkspaceRecordType.Contact) as IContact;
                if (contact == null) {
                    RightNowGlobalContext.AutomationContext.CurrentWorkspace.DataLoaded += currentWorkspaceDataLoaded;
                    RightNowGlobalContext.AutomationContext.CurrentWorkspace.Closing += currentWorkspaceClosing;

                    contact = RightNowGlobalContext.AutomationContext.CurrentWorkspace.GetWorkspaceRecord(WorkspaceRecordType.Contact) as IContact;
                    if (contact != null) {
                        RightNowGlobalContext.AutomationContext.CurrentWorkspace.DataLoaded -= currentWorkspaceDataLoaded;
                        loadContextNumbers(contact);
                    }
                }
                else
                    loadContextNumbers(contact);
            }
            /*else */
            if (workspaceType != null /*&& workspaceType.Value == WorkspaceRecordType.Incident*/)
            {
                var incident = RightNowGlobalContext.AutomationContext.CurrentWorkspace.GetWorkspaceRecord(WorkspaceRecordType.Incident) as IIncident;
                if (incident == null)
                {
                    RightNowGlobalContext.AutomationContext.CurrentWorkspace.DataLoaded += currentWorkspaceDataLoaded;

                    incident = RightNowGlobalContext.AutomationContext.CurrentWorkspace.GetWorkspaceRecord(WorkspaceRecordType.Incident) as IIncident;
                    if (incident != null)
                    {
                        RightNowGlobalContext.AutomationContext.CurrentWorkspace.DataLoaded -= currentWorkspaceDataLoaded;
                        _incident = incident;
                        _openIncidentID = _incident.ID;
                        if (_agentInCall)
                        {
                            _inCallOpenIncidentID = _incident.ID;
                        }
                    }
                }
                else
                {
                    _incident = incident;
                    _openIncidentID = _incident.ID;
                    if (_agentInCall)
                    {
                        _inCallOpenIncidentID = _incident.ID;
                    }                    
                }
            }
        }

        protected void hideUnhideAssocButtons()
        {
            //_contact can be NULL when calling from WebRTC button and having no contact record opened up.
            if (_contact != null)
            {
                if (!String.IsNullOrEmpty(_previousCallerNumber))
                {
                    CanAssociateContact = true;
                    if ((_contact.PhMobile != null && _contact.PhMobile.Trim().Equals(_previousCallerNumber)) ||
                        (_contact.PhOffice != null && _contact.PhOffice.Trim().Equals(_previousCallerNumber)) ||
                        (_contact.PhHome != null && _contact.PhHome.Trim().Equals(_previousCallerNumber)))
                    {
                        CanUnAssociateContact = true;
                    }
                    else
                    {
                        CanUnAssociateContact = false;
                    }
                }
                else
                {
                    CanUnAssociateContact = false;
                    CanUnAssociateContact = false;
                }
            } else
            {
                CanUnAssociateContact = false;
                CanUnAssociateContact = false;
            }
        }

        private void loadContextNumbers(IContact contact)
        {
            //PK: For Assoc/Unassoc Feature.
            _contact = contact;
            if(_agentInCall)
            {
                _inCallContact = contact;
            }
            hideUnhideAssocButtons();          
            addContextContact("Home", contact.PhHome, contact.ID);
            addContextContact("Mobile", contact.PhMobile, contact.ID);
            addContextContact("Office", contact.PhOffice, contact.ID);
        }

        private void currentWorkspaceDataLoaded(object sender, EventArgs e) {
            ContextContacts.Clear();
            AssocContextContacts.Clear();            
            RightNowGlobalContext.AutomationContext.CurrentWorkspace.DataLoaded -= currentWorkspaceDataLoaded;            
            var contact = RightNowGlobalContext.AutomationContext.CurrentWorkspace.GetWorkspaceRecord(WorkspaceRecordType.Contact) as IContact;
            if (contact != null) {
                loadContextNumbers(contact);
            }
            var incident = RightNowGlobalContext.AutomationContext.CurrentWorkspace.GetWorkspaceRecord(WorkspaceRecordType.Incident) as IIncident;
            if (incident != null)
            {
                _incident = incident;
                _openIncidentID = _incident.ID;
                if (_agentInCall)
                {
                    _inCallOpenIncidentID = _incident.ID;
                }                
            }
        }

        private void currentWorkspaceClosing(object sender, CancelEventArgs e)
        {
            try
            {
                RightNowGlobalContext.AutomationContext.CurrentWorkspace.Closing -= currentWorkspaceClosing;
                var incident = RightNowGlobalContext.AutomationContext.CurrentWorkspace.GetWorkspaceRecord(WorkspaceRecordType.Incident) as IIncident;
                if (incident != null && _incident != null)
                {
                    if (incident.ID == _incident.ID)
                    {
                        _incident = null;
                        _openIncidentID = 0;
                        
                    }
                    if(incident.ID == _inCallOpenIncidentID)
                    {
                        _inCallOpenIncidentID = 0;
                    }
                }

                var contact = RightNowGlobalContext.AutomationContext.CurrentWorkspace.GetWorkspaceRecord(WorkspaceRecordType.Contact) as IContact;
                if (contact != null && _contact != null)
                {
                    if(contact.ID == _contact.ID)
                    {
                        _contact = null; 
                    }
                }

                if (contact != null && _inCallContact != null)
                {
                    if (contact.ID == _inCallContact.ID)
                    {
                        _inCallContact = null;
                    }
                }



            } catch (Exception ex)
            {
                Logger.Logger.Log.Debug("Exception during currentWorkspaceClosing ", ex);
            }
            e.Cancel = false;
        }
        public static string RemoveSpecialCharacters(string str)
        {
            string retVal = "";
            if (!string.IsNullOrWhiteSpace(str))
            {
                retVal = Regex.Replace(str, "[^0-9+]+", "", RegexOptions.Compiled);
            }
            return retVal;
        }

        private void addContextContact(string p1, string p2, int contactId)
        {
            if (!string.IsNullOrEmpty(p2))
            {
                var name = string.Format("{0} ({1})", p1, p2);
                ContextContacts.Add(new Contact
                {
                    Id = contactId.ToString(),
                    Name = name,
                    Number = RemoveSpecialCharacters(p2),
                    Type = p1,
                    TransferType = TransferTypes.OutboundDialing,
                    Description = name
                });
            }

            var assocname = string.Format("{0} ({1})", p1, String.IsNullOrEmpty(p2) ? "" : p2);
            AssocContextContacts.Add(new Contact
            {
                Id = contactId.ToString(),
                Name = assocname,
                Number = RemoveSpecialCharacters(p2),
                Type = p1,
                TransferType = TransferTypes.OutboundDialing,
                Description = assocname
            });
        }

        /// <summary>
        /// Toggles the agent login state.
        /// </summary>
        public void ToggleLogin() {
            Task.Factory.StartNew(() => {
                if (InteractionProvider.Agent.CurrentState != null &&
                    InteractionProvider.Agent.CurrentState.SwitchMode == AgentSwitchMode.LoggedOut) 
                    InteractionProvider.Login();
                else 
                    InteractionProvider.Logout();
            });
        }


        /// <summary>
        /// Answers the current interaction
        /// </summary>
        public void AnswerCall() {
            Task.Factory.StartNew(() => {
                var call = InteractionProvider.CurrentInteraction as ICall;
                if (call != null && call.State == InteractionState.Ringing) {
                    call.Accept();
                }
            });
        }

        /// <summary>
        /// Disconnects the current interaction, if it is a call.
        /// </summary>
        public void HangUpCall() {
            Task.Factory.StartNew(() => {
                var call = InteractionProvider.CurrentInteraction as ICall;
                if (call != null) {
                    call.HangUp();
                }
            });
        }

        /// <summary>
        /// Transfers the current interaction, if it is a call.
        /// </summary>
        public void TransferCall(Contact contact) {
            Task.Factory.StartNew(c => {
                
                var call = InteractionProvider.CurrentInteraction as ICall;
                if (call != null) {
                    InteractionProvider.Device.InitiateTransfer(call, contact.TransferType, contact.Number);
                }
            }, contact, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// If the current interaction is a call, it will be placed on hold if active or retrieved if on hold.
        /// </summary>
        public void HoldRetrieveCall() {
            Task.Factory.StartNew(() => {
                var call = InteractionProvider.CurrentInteraction as ICall;
                if (call != null) {
                    if (call.State == InteractionState.Active)
                        call.Hold();
                    else if (call.State == InteractionState.Held)
                        call.Retrieve();
                }
            });
        }

        public ObservableCollection<Contact> ContextContacts { get; set; }
        public ObservableCollection<Contact> AssocContextContacts { get; set; }

        public void CompleteInteraction() {
            Task.Factory.StartNew(() => {
                InteractionProvider.CompleteInteraction();
            });
        }

        public virtual void InitiateTransfer() {
        }

        public virtual void InitiateConference() {
        }

        protected void EnableContextSynchronization(SynchronizationContext context) {
            SynchronizationContext = context;
        }

        private void agentStateChangedHandler(object sender, AgentStateChangedEventArgs e) {
            switch (e.NewState.SwitchMode)
            {
                case AgentSwitchMode.LoggedIn:
                    CanMakeCall = true;
                    break;
                case AgentSwitchMode.Ready:
                    break;
                case AgentSwitchMode.NotReady:
                    break;
                case AgentSwitchMode.LoggedOut:
                    CanMakeCall = false;
                    break;
                case AgentSwitchMode.WrapUp:
                    break;
                case AgentSwitchMode.Talking:
                    break;
                case AgentSwitchMode.NewReason:
                    break;
                case AgentSwitchMode.Connecting:
                    break;
                case AgentSwitchMode.Hold:
                    break;
                default:
                    break;
            }

            setAgentState(e.NewState);
        }
  
        protected virtual void CurrentInteractionChangedHandler(object sender, InteractionEventArgs e) {
            synchronizeAndRaiseOnPropertyChanged("CurrentInteraction");
            synchronizeButtonStates();
        }

        protected virtual void InteractionCompletedHandler(object sender, InteractionEventArgs e) {
            synchronizeAndInvoke(o => Interactions.Remove((IInteraction)o), e.Interaction);
            synchronizeButtonStates();
            CanAssociateToRecord = CanAssociateToRecord && Interactions.Count > 0;
        }
  
        private void synchronizeButtonStates() {
            synchronizeAndInvoke(o => {
                if (CurrentInteraction == null || CurrentInteraction.State == InteractionState.Disconnected) {
                    EnabledButtons = (int)MediaBarButtons.None;
                }
                else {
                    switch (CurrentInteraction.Type)
                    {
                        case MediaType.Voice:
                            if (((ICall)CurrentInteraction).State == InteractionState.Ringing)
                                EnabledButtons = (int)MediaBarButtons.AnswerHangup;
                            else
                                EnabledButtons = (int)MediaBarButtons.Voice;
                            break;
                    }
                }
            });
        }

        protected virtual void InteractionConnectedHandler(object sender, InteractionEventArgs e) {
            synchronizeButtonStates();
        }

        protected virtual void InteractionDisconnectedHandler(object sender, InteractionEventArgs e) {
            synchronizeAndInvoke(o => EnabledButtons &= ~((int)MediaBarButtons.All));
        }
  
        protected virtual void NewInteractionHandler(object sender, InteractionEventArgs e) {
            synchronizeAndInvoke(o => Interactions.Add((IInteraction)o), e.Interaction);
        }

        //PK: Changed to Protected
        protected bool setAgentState(AgentState state) {
            if (_currentAgentState != state) {
                _currentAgentState = state;
                synchronizeAndRaiseOnPropertyChanged("CurrentAgentState");
                return true;
            }

            return false;
        }

        private void synchronizeAndInvoke(Action<object> action, object state = null) {
            if (SynchronizationContext != null) {
                SynchronizationContext.Send(new SendOrPostCallback(action), state);
            }
            else {
                action(state);
            }
        }
  
        protected void synchronizeAndRaiseOnPropertyChanged(string propertyName) {
            if (SynchronizationContext != null) {
                SynchronizationContext.Send(new SendOrPostCallback(o => OnPropertyChanged(o.ToString())), propertyName);
            }
            else {
                OnPropertyChanged(propertyName);
            }
        }

        //For updating the contact list binding from Unassoc button.
        protected bool updateContactListAfterUnassoc()
        {
            ContextContacts.Clear();
            AssocContextContacts.Clear();
            addContextContact("Home", _contact.PhHome, _contact.ID);
            addContextContact("Mobile", _contact.PhMobile, _contact.ID);
            addContextContact("Office", _contact.PhOffice, _contact.ID);
            return true;
        }
        protected bool updateContactRecord(Contact contact, string updatedNumber)
        {
            try
            {
                if (contact.Type.Equals("Home"))
                {
                    _contact.PhHome = _previousCallerNumber;

                }
                else if (contact.Type.Equals("Mobile"))
                {
                    _contact.PhMobile = _previousCallerNumber;
                }
                else if (contact.Type.Equals("Office"))
                {
                    _contact.PhOffice = _previousCallerNumber;
                }
                ContextContacts.Clear();
                AssocContextContacts.Clear();                
                addContextContact("Home", _contact.PhHome, _contact.ID);
                addContextContact("Mobile", _contact.PhMobile, _contact.ID);
                addContextContact("Office", _contact.PhOffice, _contact.ID);
                RightNowGlobalContext.AutomationContext.CurrentWorkspace.ExecuteEditorCommand(EditorCommand.Save);
                RightNowGlobalContext.AutomationContext.CurrentWorkspace.RefreshWorkspace();
            } catch (Exception ex)
            {
                Logger.Logger.Log.Error("RightNowObjectProvider:", ex);
                CanUnAssociateContact = false;
                return false;
            }
            CanUnAssociateContact = true;
            return true;
        }

        protected void OpenContact(string studentId, String rawNumber, int reportID, out long openedContactID)
        {
            openedContactID = -1;
            _previousCallerNumber = rawNumber;
            if(_previousCallerNumber.Trim().ToLower().Equals("unknown"))
            {
                _previousCallerNumber = "";
            }

            _reportID = Convert.ToString(reportID);
            //First try to get a record from student Id.
            CanUnAssociateContact = false;
            IList<long> ids = new List<long>();

            if (!string.IsNullOrEmpty(studentId))
            {
                ids = _objectProvider.GetObjectIds("Contact O", string.Format("O.CustomFields.c.student_id = {0}", studentId));
            }

            var idCount = ids.Count();
            if (idCount > 1)
            {
                //Got more then 1 record. Get a Report with Id 109823.
                var filter = new ReportFilter
                {
                    Expression = "contacts.c$student_id",
                    OperatorType = ReportFilterOperatorType.Equals,
                    Value = studentId
                };

                if (!_oldReportTabIdentifier.Equals(""))
                {
                    RightNowGlobalContext.AutomationContext.CloseEditor(_oldReportTabIdentifier);
                }
                RightNowGlobalContext.AutomationContext.RunReport(reportID, new List<IReportFilter2> { filter });
            }
            else if (idCount == 1)
            {
                //Got only 1 Record. Open the record.
                RightNowGlobalContext.AutomationContext.EditWorkspaceRecord(WorkspaceRecordType.Contact, ids[0]);
            }
            else
            {
                ids = new List<long>();
                if (!string.IsNullOrEmpty(rawNumber))
                {
                    //No match. Unable to find record(s) with a maching student Id. Find by RawNumber
                    //Distinct added as Contact having Mobile & Office as the same number were counted as 2 records and a report was displayed instead of the contact record.
                    ids = _objectProvider.GetObjectIds("Contact O", string.Format("O.Phones.RawNumber = '{0}'", rawNumber)).Distinct().ToList();
                }

                idCount = ids.Count();

                if (idCount > 1)
                {
                    var filter = new ReportFilter
                    {
                        Expression = "contacts.any_phone_raw",
                        OperatorType = ReportFilterOperatorType.Equals,
                        Value = rawNumber
                    };
                    if (!_oldReportTabIdentifier.Equals(""))
                    {
                        RightNowGlobalContext.AutomationContext.CloseEditor(_oldReportTabIdentifier);
                    }
                    RightNowGlobalContext.AutomationContext.RunReport(reportID /*Contacts report*/, new List<IReportFilter2> { filter });
                }
                else if (idCount == 1)
                {
                    //Exactly 1 match found for Raw Number.
                    openedContactID = ids[0];
                    CanUnAssociateContact = true;
                    RightNowGlobalContext.AutomationContext.EditWorkspaceRecord(WorkspaceRecordType.Contact, ids[0]);
                }
                else
                {
                    //No match found for raw number
                    RightNowGlobalContext.AutomationContext.RunReport(reportID /*Contacts report*/, new List<IReportFilter2> { });
                }
            }
        }

        protected void SaveAndRefreshWorkspace()
        {
            RightNowGlobalContext.AutomationContext.CurrentWorkspace.ExecuteEditorCommand(EditorCommand.Save);
            RightNowGlobalContext.AutomationContext.CurrentWorkspace.RefreshWorkspace();
        }
    }

    internal class ReportFilter : IReportFilter2
    {
        public string Expression { get; set; }

        public ReportFilterOperatorType OperatorType { get; set; }

        public string Value { get; set; }
    }
}