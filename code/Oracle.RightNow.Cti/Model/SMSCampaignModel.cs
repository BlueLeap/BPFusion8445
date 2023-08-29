using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Oracle.RightNow.Cti.Model
{
    public class SMSCampaignModel 
    {
        //private string _Status;
        //public string isQueued { get; set; }
        public int ID { get; set; }
        private string _Status;

        public string Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                if (_Status.Equals("Executing"))
                {
                    EditVisiblity = false;
                    StopVisiblity = true;
                }
            }
        }
        
        public string CampaignName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? RunTime { get; set; }
        public string MessageBody { get; set; }
        public string Alpha { get; set; }
        public string Override { get; set; }
        public int Total { get; set; }
        public int OptOut { get; set; }
        public int AgentID { get; set; }
        public int ListID { get; set; }
        public string AgentName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }

        public int CampaignSenderID { get; set; }

        public bool EditVisiblity { get; private set;}
        public bool StopVisiblity { get; private set; }

        public bool CancelVisiblity { get {
                return !StopVisiblity;
            } }

        public SMSCampaignModel()
        {
            StartTime = null;
            FinishTime = null;
            RunTime = null;
            DateCreated = DateTime.MinValue;
            EditVisiblity = true;
            StopVisiblity = false;
        }
    }
}
