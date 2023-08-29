using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueleap.Finesse.Events.Callback
{
    public class LastInteraction
    {
        public string fromnumber;
        public string tonumber;
        public string studentid;
        public string contactid;
        public string source;
        public string status;
        public string dialogid;
        public string starttime;
        public string finishtime;
        public string cv1;
        public string cv2;
        public string cv3;
        public string cv4;
        public string cv5;
        public string cv6;
        public string cv7;
        public string cv8;
        public string cv9;
        public string cv10;
        public Hashtable callvariables;
        public LastInteraction()
        {
            fromnumber = "";
            tonumber = "";
            studentid = "";
            contactid = "";
            source = "Media Bar";
            status = "";
            dialogid = "";
            starttime = "";
            finishtime = "";
            cv1 = "";
            cv2 = "";
            cv3 = "";
            cv4 = "";
            cv5 = "";
            cv6 = "";
            cv7 = "";
            cv8 = "";
            cv9 = "";
            cv10 = "";
        }
    }
    public interface IConnectionEventListner
    {
        //CALL
        void GetEventOnConnection(string finesseIP, String evt);
        void GetEventOnDisConnection(string finesseIP, String evt);
        void GetEventOnCallAlerting(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table);
        void GetEventOnCallEstablished(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table);
        
        void GetEventOnCallDropped(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table);
        void GetEventOnCallWrapUp(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table);
        void GetEventOnCallHeld(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table);
        void GetEventOnCallInitiating(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table);
        void GetEventOnCallInitiated(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table);
        void GetEventOnCallFailed(string dialogID, string callType, string fromAddress, string toAddress, Hashtable table);
        
        void GetEventOnPassCheck(string ret, string data);
        
        
        void GetEventOnAgentStateChange(string state, string reasonCode, string evtMessage);
        
        void GetEventOnCallError(string errorMessage);
        void GetEventOnFinesseConnectionProblem(string finesseIP);

        void WriteCallLog(LastInteraction interaction, bool isOutboundCall = false);
    }
}
