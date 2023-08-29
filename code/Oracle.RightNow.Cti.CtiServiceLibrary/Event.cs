using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Blueleap.Finesse.Events
{
    public class Event
    {
        private string evtCode;
        private string evtType;
        private string evtMsg;

        private string dialogID;
        private string callType;
        private string agentState;
        private string reasonCode;

        private string callState;

        private Hashtable callVarTable;
        private string currentFinesseIP;


        public void setCurFinesseIP(string ip)
        {
            this.currentFinesseIP = ip;
        }
        public string getCurFinesseIP()
        {
            return this.currentFinesseIP;
        }

        public void setCallVariable(Hashtable table)
        {
            this.callVarTable = table;
        }
        public Hashtable getCallVariable()
        {
            return this.callVarTable;
        }

        public void setCallState(string callState)
        {
            this.callState = callState;
        }
        public string getCallState()
        {
            return this.callState;
        }

        public void setAgentState(string agentState)
        {
            this.agentState = agentState;
        }
        public string getAgentState()
        {
            return this.agentState;
        }
        public void setReasonCode(string reasonCode)
        {
            this.reasonCode = reasonCode;
        }
        public string getReasonCode()
        {
            return this.reasonCode;
        }

        public void setDialogID(string dialogID)
        {
            this.dialogID = dialogID;
        }
        public string getDialogID()
        {
            return dialogID;
        }
        public void setCallType(string callType)
        {
            this.callType = callType;
        }
        public string getCallType()
        {
            return this.callType;
        }


        public void setEvtCode(string evtCode)
        {
            this.evtCode = evtCode;
        }

        public string getEvtCode()
        {
            return this.evtCode;
        }

        public void setEvtType(string evtType)
        {
            this.evtType = evtType;
        }

        public string getEvtType()
        {
            return evtType;
        }

        public void setEvtMsg(string evtMsg)
        {
            this.evtMsg = evtMsg;
        }

        public string getEvtMsg()
        {
            return this.evtMsg;
        }

    }

    class AgentEvent : Event
    {
        private string agentState;
        private string reasonCode;

        public void setAgentState(string agentState)
        {
            this.agentState = agentState;
        }
        public string getAgentState()
        {
            return this.agentState;
        }
        public void setReasonCode(string reasonCode)
        {
            this.reasonCode = reasonCode;
        }
        public string getReasonCode()
        {
            return this.reasonCode;
        }
    }

    class CallEvent : Event
    {
        private string fromAddress;
        private string toAddress;

        private string dialogID;
        private string callType;

        private Hashtable callVarTable;

        //private string callState;

        private Hashtable callStateTable;

        public CallEvent()
        {
            callStateTable = new Hashtable();
        }

        public void setCallState(string number, string state)
        {
            if (callStateTable.ContainsKey(number))
            {
                callStateTable.Remove(number);
            }
            callStateTable.Add(number, state);
        }

        public Hashtable getCallStateTable()
        {
            return this.callStateTable;
        }


        public void setFromAddress(string fromAddress)
        {
            this.fromAddress = fromAddress;
        }
        public string getFromAddress()
        {
            return this.fromAddress;
        }

        public void setToAddress(string toAddress)
        {
            this.toAddress = toAddress;
        }
        public string getToAddress()
        {
            return this.toAddress;
        }

        public void setCallVariable(Hashtable table)
        {
            this.callVarTable = table;
        }
        public Hashtable getCallVariable()
        {
            return this.callVarTable;
        }

        public void setDialogID(string dialogID)
        {
            this.dialogID = dialogID;
        }
        public string getDialogID()
        {
            return dialogID;
        }
        public void setCallType(string callType)
        {
            this.callType = callType;
        }
        public string getCallType()
        {
            return this.callType;
        }


    }

    class ErrorEvent : Event
    {
        private string errorMessage;
        private string errorType;
        private string serverType;


        public void setServerType(string serverType)
        {
            this.serverType = serverType;
        }
        public string getServerType()
        {
            return this.serverType;
        }
        public void setErrorMessage(string message)
        {
            this.errorMessage = message;
        }
        public string getErrorMessage()
        {
            return errorMessage;
        }

        public void setErrorType(string errorType)
        {
            this.errorType = errorType;
        }
        public string getErrorType()
        {
            return errorType;
        }

    }
}
