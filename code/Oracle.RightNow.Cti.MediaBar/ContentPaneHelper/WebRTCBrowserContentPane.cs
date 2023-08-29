using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using RightNow.AddIns.AddInViews;
using System.Windows.Forms.Integration;
using Oracle.RightNow.Cti.MediaBar;
using Oracle.RightNow.Cti.Properties;
using Oracle.RightNow.Cti.MediaBar.Properties;

namespace Oracle.RightNow.Cti.MediaBar
{
    public class WebRTCBrowserContentPane : IContentPaneControl
    {
        private const string UniqueId = "0864BB57-C9FD-40FC-89FF-C38726138EA7";
        private string URL;

        private Control _panelControl;

        public WebRTCBrowserContentPane(string url)
        {
            URL = url;
        }

        //[Import]
        public IGlobalContext GlobalContext { get; set; }

        public bool BeforeClose()
        {
            return true;
        }

        public void Closed()
        {
        }
        public System.Drawing.Image Image16
        {
            get { return null; }
        }

        public IList<IEditorRibbonButton> RibbonButtons
        {
            get { return new List<IEditorRibbonButton>(); }
        }

        public string Text
        {
            get { return "WebRTC Calling"; }
        }

        public string UniqueID
        {
            get { return UniqueId; }
        }

        public System.Windows.Forms.Control GetControl()
        {
            if (_panelControl == null)
            {
                WebBrowser browser = new WebBrowser();
                browser.Url = new Uri(URL);
                _panelControl = browser;
            }
            return _panelControl;
        }
    }
}
