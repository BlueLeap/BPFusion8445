using System;
using System.AddIn;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Oracle.RightNow.Cti.MediaBar;
using Oracle.RightNow.Cti.Properties;
using RightNow.AddIns.AddInViews;
using RightNow.AddIns.Common;
namespace Oracle.RightNow.Cti.AddIn {
    [AddIn("BlueLeap Media Bar", Publisher = "Blueleap", Version = "2.0.0.0")]
    public class MediaBarAddIn : IGlobalDockWindowControl {
        private IGDWContext _dockWindowContext;
        private CompositionContainer _container;
        public static string _reportID;
        public static string _FinesseDomain;
        public static string _OutsidePrefix;
        public static string _InternationalPrefix;
        public static bool _WebRTCEnable;
        public static bool _SMSEnable;
        public static string _OutboundCallVariableString;
        public static bool _EnableLogging;
        public static bool _EnableHTTPS;
        public static bool _WebRTCDialPad;
        public static bool _LockInternationalCalls;
        public static string _LocalCountryCode;
        public static string _TrunkAccessCode;
        public static string _MobilePrefix;
        public static string _LocalAreaCode;
        public static string _WebRTCURL;
        public static bool _HideSMSCampaign;
        public static int _SMSMaxLength;
        public static bool _EnableCTILogin;
        public static string _BLContactListSearchByID;
        public static bool _ShowAlphaNumericSendingOptions;
        public static bool _ShowOptOutSendingOptions;
        public static string _ShowSendingOptionsDefault;
        public static string _ManageCampaignReportID;
        public static int _MyInboxReportID;
        public static int _MyIncidentReportID;
        public static int _AnswersReportID;
        public static int _ProcessCallReportID;
        public static int _DashboardReportID;
        public static string _MyInboxReportText;
        public static string _MyIncidentReportText;
        public static string _ProcessCallReportIText;
        public static string _AnswersReportText;
        public static string _DashboardReportText;
        public static bool _CTICreateIncident;
        public static bool _AutoOpenIncidentForIncomingSMS;
        public bool Initialize(IGlobalContext context) {
            GlobalContext = context;
            _reportID = this.ReportID;
            _FinesseDomain = this.FinesseDomain;
            _OutsidePrefix = this.OutsidePrefix;
            _InternationalPrefix = "+";
            _OutboundCallVariableString = this.OutboundPreviewCallVariableString;
            _LocalCountryCode = "61";
            _LocalAreaCode = "3";
            _TrunkAccessCode = "0";
            _MobilePrefix = "4";
            _SMSMaxLength = this.SMSMaxLength;
            _WebRTCEnable = true;
            _BLContactListSearchByID = this.BLContactListSearchByID;
            _ManageCampaignReportID = this.ManageCampaignReportID;
            _MyInboxReportText = MyInboxReportText;
            _MyIncidentReportText = MyIncidentReportText;
            _ProcessCallReportIText = ProcessCallReportIText;
            _AnswersReportText = AnswersReportText;
            _DashboardReportText = DashboardReportText;
            _CTICreateIncident = false;
            _AutoOpenIncidentForIncomingSMS = false;
            _MyInboxReportID = 100030;
            try
            {
                _MyInboxReportID = Convert.ToInt32(MyInboxReportID);
            } catch (Exception ex) { }

            _MyIncidentReportID = 100110;
            try
            {
                _MyIncidentReportID = Convert.ToInt32(MyIncidentReportID);
            }
            catch (Exception ex) { }

            _AnswersReportID = 100111;
            try
            {
                _AnswersReportID = Convert.ToInt32(AnswersReportID);
            }
            catch (Exception ex) { }

            _ProcessCallReportID = 13027;
            try
            {
                _ProcessCallReportID = Convert.ToInt32(ProcessCallReportID);
            }
            catch (Exception ex) { }

            _DashboardReportID = 100109;
            try
            {
                _DashboardReportID = Convert.ToInt32(DashboardReportID);
            }
            catch (Exception ex) { }

            //if (!String.IsNullOrEmpty(this.WebRTCEnable))
            //{
            //    string webRTCEnable = this.WebRTCEnable.Trim().ToLower();
            //    if(webRTCEnable.Equals("false") || webRTCEnable.Equals("no") || webRTCEnable.Equals("0"))
            //    {
            //        _WebRTCEnable = false;
            //    }
            //}

            _EnableLogging = true;
            if (!String.IsNullOrEmpty(this.EnableLogging))
            {
                string logginEnable = this.EnableLogging.Trim().ToLower();
                if (logginEnable.Equals("false") || logginEnable.Equals("no") || logginEnable.Equals("0"))
                {
                    _EnableLogging = false;
                }
            }

            _EnableHTTPS = true;

            if (!String.IsNullOrEmpty(this.EnableHTTPS))
            {
                string httpsEnable = this.EnableHTTPS.Trim().ToLower();
                if (httpsEnable.Equals("false") || httpsEnable.Equals("no") || httpsEnable.Equals("0"))
                {
                    _EnableHTTPS = false;
                }
            }

            _SMSEnable = false;
            //if (!String.IsNullOrEmpty(this.EnableSMS))
            //{
            //    string enableSMS = this.EnableSMS.Trim().ToLower();
            //    if (enableSMS.Equals("false") || enableSMS.Equals("no") || enableSMS.Equals("0"))
            //    {
            //        _SMSEnable = false;
            //    }
            //}

            _WebRTCDialPad = true;
            if (!String.IsNullOrEmpty(this.WebRTCDialPad))
            {
                string enableDialPad = this.WebRTCDialPad.Trim().ToLower();
                if (enableDialPad.Equals("false") || enableDialPad.Equals("no") || enableDialPad.Equals("0"))
                {
                    _WebRTCDialPad = false;
                }
            }

            _LockInternationalCalls = true;
            if (!String.IsNullOrEmpty(this.LockInternationalCalls))
            {
                string lockInternationalCall = this.LockInternationalCalls.Trim().ToLower();
                if (lockInternationalCall.Equals("false") || lockInternationalCall.Equals("no") || lockInternationalCall.Equals("0"))
                {
                    _LockInternationalCalls = false;
                }
            }

            _HideSMSCampaign = true;
            //if (!String.IsNullOrEmpty(this.HideSMSCampaign))
            //{
            //    string hideSMSCampaign = this.HideSMSCampaign.Trim().ToLower();
            //    if (hideSMSCampaign.Equals("false") || hideSMSCampaign.Equals("no") || hideSMSCampaign.Equals("0"))
            //    {
            //        _HideSMSCampaign = false;
            //    }
            //}

            _EnableCTILogin = false;
            //if (!String.IsNullOrEmpty(this.EnableCTILogin))
            //{
            //    string ctienable = this.EnableCTILogin.Trim().ToLower();
            //    if (ctienable.Equals("false") || ctienable.Equals("no") || ctienable.Equals("0"))
            //    {
            //        _EnableCTILogin = false;
            //    }
            //}

            _ShowAlphaNumericSendingOptions = true;
            //if (!String.IsNullOrEmpty(this.ShowAlphaNumericSendingOptions))
            //{
            //    string alphaNumericEnable = this.ShowAlphaNumericSendingOptions.Trim().ToLower();
            //    if (alphaNumericEnable.Equals("false") || alphaNumericEnable.Equals("no") || alphaNumericEnable.Equals("0"))
            //    {
            //        _ShowAlphaNumericSendingOptions = false;
            //    }
            //}

            _ShowOptOutSendingOptions = true;
            _ShowSendingOptionsDefault = "Use displayed number";
            Logger.Logger.Configure(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!_EnableLogging)
            {
                Logger.Logger.LogLevel = "OFF";
            }
            return true;
        }

        private void initializeContainer()
        {
            var baseCatalog = new AggregateCatalog();
            var info = Directory.GetParent(Path.GetDirectoryName(typeof(MediaBarAddIn).Assembly.Location));

            DirectoryInfo catalogRoot = null;
            GlobalContext.LogMessage("Multi-Channel Toolkit: Initializing container.");
            // Change to support network directories....
            if (info.Parent.FullName.StartsWith("\\\\"))
            {
                GlobalContext.LogMessage(string.Format("Multi-Channel Toolkit: UNC path deployment ({0}). Moving add-in files.", info.Parent));
                var temp = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
                GlobalContext.LogMessage(string.Format("Target container direcory:", temp));

                var directories = info.Parent.GetDirectories("*", SearchOption.AllDirectories);

                foreach (var dir in directories)
                {
                    string targetDir = dir.FullName.Replace(info.Parent.FullName, temp.FullName);
                    Directory.CreateDirectory(targetDir);

                    var directoryFies = dir.GetFiles();

                    foreach (var file in directoryFies)
                    {
                        try
                        {
                            File.Copy(file.FullName, Path.Combine(targetDir, file.Name));
                        }
                        catch (Exception exc)
                        {
                            GlobalContext.LogMessage(string.Format("Failed to copy file {0}. Error: {1}", file.Name, exc.Message));
                        }
                    }
                }

                catalogRoot = temp;
            }
            else
            {
                GlobalContext.LogMessage(string.Format("Multi-Channel Toolkit: Local path deployment ({0}).", info.Parent));

                catalogRoot = info.Parent;
            }

            addDirectoryToCatalog(catalogRoot, baseCatalog);

            _container = new CompositionContainer(baseCatalog);
            try
            {
                _container.ComposeParts(this);
            }
            catch (CompositionException exc)
            {
                GlobalContext.LogMessage(exc.ToString());
                throw;
            }
        }

        private void addDirectoryToCatalog(DirectoryInfo directoryInfo, AggregateCatalog catalog) {
            catalog.Catalogs.Add(new DirectoryCatalog(directoryInfo.FullName));

            foreach (var directory in directoryInfo.GetDirectories()) {
                addDirectoryToCatalog(directory, catalog);
            }
        }

        public Control GetControl() {
            if (_container == null) {
                initializeContainer();
            }
            return MediaBar.GetView(_dockWindowContext);
        }

        public void SetGDWContext(IGDWContext context) {
            _dockWindowContext = context;
            _dockWindowContext.Docking = DockingType.Top;
            _dockWindowContext.Title = Resources.MediaBarTitle;
        }

        public void ShortcutActivated() {
        }

        [Export]
        public IGlobalContext GlobalContext { get; set; }

        [Import]
        public IMediaBarProvider MediaBar { get; set; }

        public string GroupName {
            get {
                return Resources.MediaBarGroupName;
            }
        }

        public int Order {
            get {
                return 0;
            }
        }

        public Keys Shortcut {
            get {
                return Keys.None;
            }
        }

        [ServerConfigProperty(DefaultValue = "devuccx.vu.edu.au")]
        public string FinesseDomain { get; set; }

        [ServerConfigProperty(DefaultValue = "109823")]
        public string ReportID { get; set; }

        [ServerConfigProperty(DefaultValue = "0")]
        public string OutsidePrefix { get; set; }
        //[ServerConfigProperty(DefaultValue = "+")]
        //public string InternationalPrefix { get; set; }
        //[ServerConfigProperty(DefaultValue = "true")]
        //public string WebRTCEnable { get; set; }

        [ServerConfigProperty(DefaultValue = "callbackin")]
        public string OutboundPreviewCallVariableString { get; set; }

        [ServerConfigProperty(DefaultValue = "true")]
        public string EnableLogging { get; set; }

        [ServerConfigProperty(DefaultValue = "true")]
        public string EnableHTTPS { get; set; }
        //[ServerConfigProperty(DefaultValue = "true")]
        //public string EnableSMS { get; set; }

        [ServerConfigProperty(DefaultValue = "true")]
        public string WebRTCDialPad { get; set; }

        [ServerConfigProperty(DefaultValue = "false")]
        public string LockInternationalCalls { get; set; }

        //[ServerConfigProperty(DefaultValue = "61")]
        //public string LocalCountryCode { get; set; }
        //[ServerConfigProperty(DefaultValue = "0")]
        //public string TrunkAccessCode { get; set; }
        //[ServerConfigProperty(DefaultValue = "4")]
        //public string MobilePrefix { get; set; }
        //[ServerConfigProperty(DefaultValue = "3")]
        //public string LocalAreaCode { get; set; }
        //[ServerConfigProperty(DefaultValue = "http://ec2-54-198-10-41.compute-1.amazonaws.com/VU_Uni/WebCall.php")]
        //public string WebRTC_URL { get; set; }
        //[ServerConfigProperty(DefaultValue = "false")]
        //public string HideSMSCampaign { get; set; }
        [ServerConfigProperty(DefaultValue = "500")]
        public int SMSMaxLength { get; set; }
        //[ServerConfigProperty(DefaultValue = "true")]
        //public string EnableCTILogin { get; set; }
        [ServerConfigProperty(DefaultValue = "109838")]
        public string BLContactListSearchByID { get; set; }
        //[ServerConfigProperty(DefaultValue = "false")]
        //public string ShowAlphaNumericSendingOptions { get; set; }
        [ServerConfigProperty(DefaultValue = "109905")]
        public string ManageCampaignReportID { get; set; }
        [ServerConfigProperty(DefaultValue = "0")]
        public string MyInboxReportID { get; set; }
        [ServerConfigProperty(DefaultValue = "0")]
        public string MyIncidentReportID { get; set; }
        [ServerConfigProperty(DefaultValue = "0")]
        public string ProcessCallReportID { get; set; }
        [ServerConfigProperty(DefaultValue = "0")]
        public string AnswersReportID { get; set; }
        [ServerConfigProperty(DefaultValue = "0")]
        public string DashboardReportID { get; set; }
        [ServerConfigProperty(DefaultValue = "My Inbox")]
        public string MyInboxReportText { get; set; }
        [ServerConfigProperty(DefaultValue = "My Incidents")]
        public string MyIncidentReportText { get; set; }
        [ServerConfigProperty(DefaultValue = "Process Call")]
        public string ProcessCallReportIText { get; set; }
        [ServerConfigProperty(DefaultValue = "Answers")]
        public string AnswersReportText { get; set; }
        [ServerConfigProperty(DefaultValue = "Monitoring Dashboard")]
        public string DashboardReportText { get; set; }

    }
}