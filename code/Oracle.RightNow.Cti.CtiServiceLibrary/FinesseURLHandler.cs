using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blueleap.Finesse.Constants;

namespace Blueleap.Finesse.HTTP
{
    class FinesseURLHandler
    {
        public string protocol = SERVERINFO.Finesse_PROTOCOL;
        public string port = SERVERINFO.Finesse_PORT;

        public string getUserURL(string serverIP , Agent agent)
        {
            return protocol + serverIP + ":" + port + "/finesse/api/User/" + agent.getAgentID();
        }
        public string getDialogURL(string serverIP, Agent agent)
        {
            return protocol + serverIP + ":" + port + "/finesse/api/User/" + agent.getAgentID() + "/Dialogs";
        }

        public string getAnswerURL(string serverIP, Agent agent, string dialogID)
        {
            return protocol + serverIP + ":" + port + "/finesse/api/Dialog/" + dialogID;
        }

        public string getCallDialogURL(string serverIP, Agent agent, string dialogID)
        {
            return protocol + serverIP + ":" + port + "/finesse/api/Dialog/" + dialogID;
        }

        public string getReasonCodeURL(string serverIP, Agent agent)
        {
            return protocol + serverIP + ":" + port + "/finesse/api/User/" + agent.getAgentID() + "/ReasonCodes?category=NOT_READY";
        }
    }
}
