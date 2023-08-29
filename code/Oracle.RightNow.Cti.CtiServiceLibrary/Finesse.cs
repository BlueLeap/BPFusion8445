using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Xml;
using Blueleap.Finesse.Events;
using Blueleap.Finesse.Events.Callback;
using Blueleap.Finesse.Constants;
using System.Net;
using System.Threading;

namespace Blueleap.Finesse
{
    public class FinesseNotReadyReasonState
    {
        public string URI;
        public string Code;
        public string Label;
        public FinesseNotReadyReasonState(string uri, string code, string label)
        {
            URI = uri;
            Code = code;
            Label = label;
        }
    }    

    public class Finesse
    {
        private FinesseClient FinesseClient;
        private Logger logwrite;

        private bool isFinesseConnected = false;

        private Dictionary<string,FinesseNotReadyReasonState> reasonCodeTable;

        private IConnectionEventListner callback;

        private LastInteraction lastInteractionCall;
        public Finesse(IConnectionEventListner listner)
        {
            logwrite = Logger.getInstance();
            callback = listner;
            lastInteractionCall = null;
        }


        public int fnConnect(String finesseDomain, int loglevel)
        {
            logwrite.write("fnConnect", "\n call fnConnect() \n");

            StringBuilder sb = new StringBuilder();
            int finesseport = Convert.ToInt32(SERVERINFO.Finesse_PORT);

            ServerInfo finesseInfo = new ServerInfo(finesseport, finesseDomain);

            if (isFinesseConnected)
            {
                logwrite.write("fnConnect", "Finesse is Already Connected!!");
            }
            else
            {
                FinesseClient = new FinesseClient(logwrite, this);
                FinesseClient.setServerInfo(finesseInfo);
                if (FinesseClient.startClient() != ERRORCODE.SUCCESS)
                {
                    logwrite.write("fnConnect", "Finesse Cannot Connect");
                    isFinesseConnected = false;
                    return ERRORCODE.FAIL;
                }
                else
                {
                    isFinesseConnected = true;
                }
            }


            if (isFinesseConnected)
            {
            }

            return ERRORCODE.SUCCESS;
        }

        public int fnDisconnect()
        {
            logwrite.write("fnConnect", "\n call fnDisconnect() \n");

            Event evt = new Event();
            evt.setEvtCode(EVENT_TYPE.ON_DISCONNECTION);

            if (FinesseClient != null)
            {
                FinesseClient.disconnect();
                evt.setEvtMsg("Finesse Session Disconnected");
                evt.setCurFinesseIP(FinesseClient.getCurrentServerIP());
                raiseEvent(evt);
            }

            isFinesseConnected = false;

            return ERRORCODE.SUCCESS;

        }


        public int fnLogin(String agentID, String agentPwd, String extension, String peripheralID)
        {
            logwrite.write("fnConnect", "\t ** call fnLogin() ID [" + agentID + "] Password [" + agentPwd + "] extension [" + extension + "] ** ");

            reasonCodeTable = new Dictionary<string, FinesseNotReadyReasonState>(); 
            Agent agent = Agent.getInstance();
            agent.setAgentID(agentID);
            agent.setAgentPwd(agentPwd);
            agent.setExtension(extension);

            //PK: Login is not Finesse Login but only connecting and getting the current state.
            if (FinesseClient.login() == ERRORCODE.SUCCESS)
            {
                string reasonCodeXML = fnGetReasonCodeList();
                //PK: Code to get the reason code after login.
                setReasonCodeList(reasonCodeXML);
                return ERRORCODE.SUCCESS;
            }
            else
            {
                return ERRORCODE.FAIL;
            }
        }

        public string fnGetReasonCodeList()
        {
            logwrite.write("fnGetReasonCodeList", "\t ** call fnGetReasonCodeList() **");
            return FinesseClient.getReasonCodeList();
        }

        public int fnAgentState(string state)
        {
            logwrite.write("fnAgentState", "\t ** call fnAgentState(" + state + ") **");
            return FinesseClient.agentState(state);
        }

        public int fnAgentState(string state, string reasonCode)
        {
            logwrite.write("fnAgentState", "\t ** call fnAgentState(" + state + " , " + reasonCode + ") **");

            if (reasonCodeTable == null || reasonCodeTable.Count == 0)
            {
                string reasonCodeXML = fnGetReasonCodeList();
                setReasonCodeList(reasonCodeXML);
            }
            return FinesseClient.agentState(state, reasonCode);

        }
        public int fnMakeCall(string dialNumber)
        {
            logwrite.write("fnMakeCall", "\t ** call fnMakeCall() **");
            return FinesseClient.makeCall(dialNumber);
        }

        private void setReasonCodeList(string xml)
        {
            XmlDocument xmlDoucment = new XmlDocument();
            XmlNodeList nodeList;
            try
            {
                xmlDoucment.LoadXml(xml);

                nodeList = xmlDoucment.GetElementsByTagName("ReasonCodes");
                XmlNode rootNode = nodeList.Item(0);

                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    string key = ""; string value = ""; string uri = ""; string label = "";
                    foreach (XmlNode node2 in node.ChildNodes)
                    {

                        if (node2.Name.Equals("uri"))
                        {
                            uri = node2.InnerText.ToString();
                            string tempStr = node2.InnerText.ToString();
                            char[] delimiter = { '/' };
                            string[] arr = tempStr.Split(delimiter);
                            value = arr[4];       
                        }
                        if (node2.Name.Equals("code"))
                        {
                            key = node2.InnerText.ToString();  
                        }
                        if (node2.Name.Equals("label"))
                        {
                            label = node2.InnerText.ToString();
                        }
                    }
                    reasonCodeTable.Add(key, new FinesseNotReadyReasonState (uri, value, label));
                }

            }
            catch (Exception e)
            {
                logwrite.write("setReasonCodeList", e.ToString());
            }
        }

        public void setFinesseConnected(bool isConnected)
        {
            this.isFinesseConnected = isConnected;
        }
        public bool getFinesseConnected()
        {
            return this.isFinesseConnected;
        }
        public void raiseEvent(Event evt)
        {
            if (evt == null)
            {
                logwrite.write("raiseEvent", ":::::::::::::::::::::::: evt NULL ::::::::::::::::::::::::");
                return;
            }

            AgentEvent agentEvent = null;
            CallEvent callEvent = null;
            ErrorEvent errorEvent = null;

            if (evt is AgentEvent)
            {
                agentEvent = (AgentEvent)evt;
                raiseAgentEvent(agentEvent);
            }
            else if (evt is CallEvent)
            {
                callEvent = (CallEvent)evt;
                raiseCallEvent(callEvent);
            }
            else if (evt is ErrorEvent)
            {
                errorEvent = (ErrorEvent)evt;
                raiseErrorEvent(errorEvent);
            }
        }

        private void raiseAgentEvent(AgentEvent evt)
        {
            string evtCode = evt.getEvtCode();
            string evtMessage = evt.getEvtMsg();

            evtMessage = evtMessage.Replace("\n", "");

            switch (evtCode)
            {

                case EVENT_TYPE.ON_CONNECTION:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: GetEventOnConnection ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    callback.GetEventOnConnection(evt.getCurFinesseIP(), evtMessage);
                    break;

                case EVENT_TYPE.ON_DISCONNECTION:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: GetEventOnDisConnection ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    callback.GetEventOnDisConnection(evt.getCurFinesseIP(), evtMessage);
                    break;

                case EVENT_TYPE.ON_AGENTSTATE_CHANGE:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: GetEventOnAgentStateChange ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "STATE : " + evt.getAgentState() + " , REASONCODE : " + evt.getReasonCode());
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    callback.GetEventOnAgentStateChange(evt.getAgentState(), evt.getReasonCode(), evtMessage);
                    break;

                default:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: UNKWON EVENT ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    break;
            }
        }
        private void raiseCallEvent(CallEvent evt)
        {
            string evtCode = evt.getEvtCode();
            string evtMessage = evt.getEvtMsg();

            evtMessage = evtMessage.Replace("\n", "");

            switch (evtCode)
            {

                case EVENT_TYPE.ON_CONNECTION:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: GetEventOnConnection ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    callback.GetEventOnConnection(evt.getCurFinesseIP(), evtMessage);
                    break;

                case EVENT_TYPE.ON_DISCONNECTION:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: GetEventOnDisConnection ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    callback.GetEventOnDisConnection(evt.getCurFinesseIP(), evtMessage);
                    break;

                case EVENT_TYPE.ALERTING:
                    writeCallEventLog("GetEventOnCallAlerting", evt, evt.getCallStateTable());
                    setActiveDialogID(evt);
                    callback.GetEventOnCallAlerting(evt.getDialogID(), evt.getCallType(), evt.getFromAddress(), evt.getToAddress(), evt.getCallVariable());
                    break;

                case EVENT_TYPE.FAILED:
                    writeCallEventLog("GetEventOnCallFailed", evt, evt.getCallStateTable());
                    setActiveDialogID(evt);
                    callback.GetEventOnCallFailed(evt.getDialogID(), evt.getCallType(), evt.getFromAddress(), evt.getToAddress(), evt.getCallStateTable());
                    if (lastInteractionCall == null)
                    {
                        lastInteractionCall = new LastInteraction();
                        lastInteractionCall.starttime = DateTime.Now.ToString();
                    }
                    lastInteractionCall.dialogid = evt.getDialogID();
                    lastInteractionCall.status = "FAILED";
                    lastInteractionCall.finishtime = DateTime.Now.ToString();
                    lastInteractionCall.tonumber = evt.getCallType().Equals("ACD_IN") ? "" : evt.getToAddress();
                    fillCallVariables(evt.getCallVariable(), ref lastInteractionCall);
                    callback.WriteCallLog(lastInteractionCall, evt.getCallType().Equals("OUT"));
                    lastInteractionCall = null;
                    break;

                case EVENT_TYPE.ESTABLISHED:
                    writeCallEventLog("GetEventOnCallEstablished", evt, evt.getCallStateTable());
                    setActiveDialogID(evt);
                    if(lastInteractionCall == null)
                    {
                        lastInteractionCall = new LastInteraction();
                        lastInteractionCall.starttime = DateTime.Now.ToString();
                        lastInteractionCall.dialogid = evt.getDialogID();
                        lastInteractionCall.starttime = DateTime.Now.ToString();
                        lastInteractionCall.status = "ESTABLISHED";
                        lastInteractionCall.tonumber = evt.getCallType().Equals("OUT") ? "" : evt.getToAddress();
                        lastInteractionCall.finishtime = DateTime.Now.ToString();
                        fillCallVariables(evt.getCallVariable(), ref lastInteractionCall);
                    }
                    //else
                    //{
                    //    callback.WriteCallLog(lastInteractionCall);
                    //}
                    callback.GetEventOnCallEstablished(evt.getDialogID(), evt.getCallType(), evt.getFromAddress(), evt.getToAddress(), evt.getCallStateTable());
                    break;

                case EVENT_TYPE.HELD:
                    writeCallEventLog("GetEventOnCallHeld", evt, evt.getCallStateTable());
                    //setActiveDialogID(evt);
                    callback.GetEventOnCallHeld(evt.getDialogID(), evt.getCallType(), evt.getFromAddress(), evt.getToAddress(), evt.getCallStateTable());
                    break;

                case EVENT_TYPE.INITIATING:
                    writeCallEventLog("GetEventOnCallInitiating", evt, evt.getCallStateTable());
                    setActiveDialogID(evt);
                    callback.GetEventOnCallInitiating(evt.getDialogID(), evt.getCallType(), evt.getFromAddress(), evt.getToAddress(), evt.getCallStateTable());
                    break;

                case EVENT_TYPE.INITIATED:
                    writeCallEventLog("GetEventOnCallInitiated", evt, evt.getCallStateTable());
                    setActiveDialogID(evt);
                    callback.GetEventOnCallInitiated(evt.getDialogID(), evt.getCallType(), evt.getFromAddress(), evt.getToAddress(), evt.getCallStateTable());
                    break;


                case EVENT_TYPE.WRAP_UP:
                    writeCallEventLog("GetEventOnCallWrapUp", evt, evt.getCallStateTable());
                    // checkTable(callEvent.getCallVariable());
                    removeDialogID(evt);                    
                    callback.GetEventOnCallWrapUp(evt.getDialogID(), evt.getCallType(), evt.getFromAddress(), evt.getToAddress(), evt.getCallStateTable());
                    if (lastInteractionCall != null)
                    {
                        lastInteractionCall.status = "Completed";
                        lastInteractionCall.tonumber = evt.getCallType().Equals("ACD_IN") ? "" : evt.getToAddress();
                        lastInteractionCall.finishtime = DateTime.Now.ToString();
                        callback.WriteCallLog(lastInteractionCall, evt.getCallType().Equals("OUT"));
                        lastInteractionCall = null;
                    }
                    break;

                case EVENT_TYPE.DROPPED:
                    writeCallEventLog("GetEventOnCallDropped", evt, evt.getCallStateTable());
                    // checkTable(callEvent.getCallVariable());
                    removeDialogID(evt);
                    callback.GetEventOnCallDropped(evt.getDialogID(), evt.getCallType(), evt.getFromAddress(), evt.getToAddress(), evt.getCallStateTable());
                    Agent agent = Agent.getInstance();
                    Hashtable callStateTable = evt.getCallStateTable();
                    if (evt.getCallType().ToLower().Equals("out") && callStateTable[agent.getExtension()].ToString().ToLower().Equals("dropped"))
                    {
                        lastInteractionCall.status = "Completed";
                        lastInteractionCall.tonumber = evt.getToAddress();
                        lastInteractionCall.finishtime = DateTime.Now.ToString();
                        callback.WriteCallLog(lastInteractionCall, true);
                        lastInteractionCall = null;
                    }
                    break;

                default:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: UNKWON EVENT ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    setActiveDialogID(evt);
                    break;
            }
        }
        private void raiseErrorEvent(ErrorEvent evt)
        {

            string evtCode = evt.getEvtCode();
            string evtMessage = evt.getEvtMsg();

            evtMessage = evtMessage.Replace("\n", "");

            switch (evtCode)
            {

                case EVENT_TYPE.CANNOT_CONNECT_FINESSE:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::: EventOnFinesseConnectionProblem ::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    callback.GetEventOnFinesseConnectionProblem(evt.getCurFinesseIP());
                    break;

                case EVENT_TYPE.ON_CONNECTION:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: GetEventOnConnection ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    callback.GetEventOnConnection(evt.getCurFinesseIP(), evtMessage);
                    break;

                case EVENT_TYPE.ON_DISCONNECTION:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: GetEventOnDisConnection ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    callback.GetEventOnDisConnection(evt.getCurFinesseIP(), evtMessage);
                    break;

                case EVENT_TYPE.ERROR:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: GetEventOnError ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "ERROR TYPE : " + evt.getErrorType() + " , ERROR MESSAGE : " + evt.getErrorMessage());
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    callback.GetEventOnCallError(evtMessage);
                    break;

                default:
                    logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: UNKWON EVENT ::::::::::::::::::::::::::::::::::::");
                    logwrite.write("raiseEvent", evtMessage);
                    logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                    break;
            }
        }

        private void writeCallEventLog(string eventName, CallEvent evt, Hashtable table)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("STATE : ");
            foreach (DictionaryEntry item in table)
            {
                sb.Append("[" + item.Key + " -> " + item.Value + "]");
            }
            logwrite.write("raiseEvent", ":::::::::::::::::::::::::::::::::::: " + eventName + " ::::::::::::::::::::::::::::::::::::");
            logwrite.write("raiseEvent", evt.getEvtMsg());
            logwrite.write("raiseEvent", "ID : " + evt.getDialogID());
            logwrite.write("raiseEvent", "CALLTYPE : " + evt.getCallType());
            logwrite.write("raiseEvent", "FromAddress : " + evt.getFromAddress());
            logwrite.write("raiseEvent", "ToAddress : " + evt.getToAddress());
            logwrite.write("raiseEvent", sb.ToString());
            logwrite.write("raiseEvent", "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
        }

        private void removeDialogID(CallEvent evt)
        {

            logwrite.write("removeDialogID", "Active DialogID : ");

        }

        private void setActiveDialogID(CallEvent evt)
        {
            logwrite.write("setDialogID", "Active DialogID : ");
        }

        private void checkTable(Hashtable table)
        {

            foreach (DictionaryEntry item in table)
            {
                logwrite.write("checkTable", "key : " + item.Key + " , " + item.Value);
            }
        }

        public Dictionary<string, FinesseNotReadyReasonState> getNotReadyReasonCodes()
        {
            return reasonCodeTable;
        }

        private void fillCallVariables(Hashtable callvariable, ref LastInteraction interaction)
        {
            if (callvariable != null)
            {
                interaction.studentid = callvariable["callVariable2"].ToString();
                interaction.fromnumber = callvariable["callVariable4"].ToString();
                interaction.cv1 = callvariable["callVariable1"].ToString();
                interaction.cv2 = callvariable["callVariable2"].ToString();
                interaction.cv3 = callvariable["callVariable3"].ToString();
                interaction.cv4 = callvariable["callVariable4"].ToString();
                interaction.cv5 = callvariable["callVariable5"].ToString();
                interaction.cv6 = callvariable["callVariable6"].ToString();
                interaction.cv7 = callvariable["callVariable7"].ToString();
                interaction.cv8 = callvariable["callVariable8"].ToString();
                interaction.cv9 = callvariable["callVariable9"].ToString();
                interaction.cv10 = callvariable["callVariable10"].ToString();
            }
        }
    }
}
