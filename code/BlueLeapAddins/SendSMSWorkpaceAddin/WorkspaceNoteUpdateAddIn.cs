////////////////////////////////////////////////////////////////////////////////
// Copyright © 2006-2012, Oracle and/or its affiliates. All rights reserved.
// Sample code provided for training purposes only. This sample code is
// provided "as is" with no warranties of any kind express or implied.
//
// File: ReportDashboardAddIn.cs
//
// Comments: A sample dashboard add-in that displays a picture box on the report dashboard. The picture box URL 
//           can be set through the add-in manager configuration property.
//
// Notes: 
//          You must add this item to a dashboard report
//
// Pre-Conditions: 
//          1. You must have enabled add-ins for the appropriate profile
//          2. You must have uploaded the compiled .dll to the RightNow server
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RightNow.AddIns.AddInViews;
using BlueLeapAddins;

namespace BlueLeapAddins.SMSAddin
{
    public class WorkspaceNoteUpdateComponent : IWorkspaceComponent2
    {
        private IRecordContext _recContext;

        public static string _LocalCountryCode;
        public static string _InternationalPrefix;
        public static bool _LockInternationalCalls;
        public static string _LocalAreaCode;
        public static string _TrunkAccessCode;
        public static bool _ShowAlphaNumericSendingOptions;
        public WorkspaceNoteUpdateComponent(bool inDesignMode, IRecordContext recContext)
        {
            //You can't access the IRecordContext when in design mode
            if (inDesignMode == false)
            {
                _recContext = recContext;
            }

            _InternationalPrefix = WorkspaceNoteUpdateAddInFactory.InternationalPrefix;
            _LocalCountryCode = WorkspaceNoteUpdateAddInFactory.LocalCountryCode;
            _LocalAreaCode = WorkspaceNoteUpdateAddInFactory.LocalAreaCode;
            _TrunkAccessCode = WorkspaceNoteUpdateAddInFactory.TrunkAccessCode;

            _LockInternationalCalls = true;
            if (!String.IsNullOrEmpty(WorkspaceNoteUpdateAddInFactory.LockInternationalCalls))
            {
                string lockInternationalCall = WorkspaceNoteUpdateAddInFactory.LockInternationalCalls.Trim().ToLower();
                if (lockInternationalCall.Equals("false") || lockInternationalCall.Equals("no") || lockInternationalCall.Equals("0"))
                {
                    _LockInternationalCalls = false;
                }
            }

            _ShowAlphaNumericSendingOptions = true;
            if (!String.IsNullOrEmpty(WorkspaceNoteUpdateAddInFactory.ShowAlphaNumericSendingOptions))
            {
                string alphaNumericEnable = WorkspaceNoteUpdateAddInFactory.ShowAlphaNumericSendingOptions.Trim().ToLower();
                if (alphaNumericEnable.Equals("false") || alphaNumericEnable.Equals("no") || alphaNumericEnable.Equals("0"))
                {
                    _ShowAlphaNumericSendingOptions = false;
                }
            }
        }

        #region IWorkspaceComponent2 Members

        public bool ReadOnly
        {
            get;
            set;
        }

        public void RuleActionInvoked(string actionName)
        {
            //Not used
        }

        public string RuleConditionInvoked(string conditionName)
        {
            //Not used
            return "";
        }

        #endregion

        #region IAddInControl Members

        public Control GetControl()
        {
            SMSSendUserControl noteUpdateControl = new SMSSendUserControl(_recContext, WorkspaceNoteUpdateAddInFactory.RightNowGlobalContext);
            noteUpdateControl.SMSSendClicked += new SMSSendUserControl.SMSSendHandler(noteUpdateControl_SMSSendClicked);

            return noteUpdateControl;
        }

        void noteUpdateControl_SMSSendClicked()
        {
            IContact contactRecord = _recContext.GetWorkspaceRecord(RightNow.AddIns.Common.WorkspaceRecordType.Contact) as IContact;
            IList<INote> notes = contactRecord.Note;

            //INote newNote = AddInViewsDataFactory.Create<INote>();

            //newNote.Text = note;
            //newNote.Seq = 0;
            //newNote.ChanID = 1;

            //notes.Add(newNote);

            //contactRecord.Note = notes;

            _recContext.RefreshWorkspace();

        }

        #endregion
    }

    [AddIn("Workspace Note Update Add-In", Version="1.0.0.0")]
    public class WorkspaceNoteUpdateAddInFactory : IWorkspaceComponentFactory2
    {
        [ServerConfigProperty(DefaultValue = "3")]
        public static string LocalAreaCode { get; set; }

        [ServerConfigProperty(DefaultValue = "61")]
        public static string LocalCountryCode { get; set; }

        [ServerConfigProperty(DefaultValue = "+")]
        public static string InternationalPrefix { get; set; }

        [ServerConfigProperty(DefaultValue = "false")]
        public static string LockInternationalCalls { get; set; }

        [ServerConfigProperty(DefaultValue = "0")]
        public static string TrunkAccessCode { get; set; }
        [ServerConfigProperty(DefaultValue = "true")]
        public static string ShowAlphaNumericSendingOptions { get; set; }

        private IRecordContext _recContext;        

        public static IGlobalContext RightNowGlobalContext { get; set; }
        #region IWorkspaceComponentFactory2 Members

        public IWorkspaceComponent2 CreateControl(bool inDesignMode, IRecordContext context)
        {
            WorkspaceNoteUpdateComponent workspaceComponent = new WorkspaceNoteUpdateComponent(inDesignMode, context);           

            return workspaceComponent;
        }

        #endregion

        #region IFactoryBase Members

        public System.Drawing.Image Image16
        {
            get 
            {
                return BlueLeapAddins.Resources.AddIn16;
            }
        }

        public string Text
        {
            get 
            {
                return "BlueLeap Single Send SMS.";
            }
        }

        public string Tooltip
        {
            get 
            {
                return "Add-In that sends an SMS.";
            }
        }

        #endregion

        #region IAddInBase Members

        public bool Initialize(IGlobalContext context)
        {
            RightNowGlobalContext = context;
            //Initialize must return true for the add-in to execute
            return true;
        }

        #endregion
    }

}
