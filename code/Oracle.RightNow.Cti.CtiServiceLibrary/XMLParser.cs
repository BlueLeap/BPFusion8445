using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections;
using Blueleap.Finesse.Constants;
using Blueleap.Finesse.Events;

namespace Blueleap.Finesse.XML
{
    class XMLParser
    {
        private ArrayList list;
        private XmlDocument xmlDoucment;
        private XmlNodeList nodeList;
        private Logger logwrite;

        private Event evt;

        private Agent agent;


        public XMLParser(Logger logwrite, Agent agent)
        {
            list = new ArrayList();
            xmlDoucment = new XmlDocument();
            this.logwrite = logwrite;
            this.agent = agent;
        }

        public XMLParser(Logger logwrite)
        {
            this.logwrite = logwrite;
            xmlDoucment = new XmlDocument();
        }

        public Event parseXML(string xml)
        {

            try
            {
                if (xml == null)
                {
                    return null;
                }

                logwrite.write("### CHECK ###", xml);

                xmlDoucment.LoadXml(xml);

                evt = null;

                // Agent State Event
                nodeList = xmlDoucment.GetElementsByTagName("user");
                if (nodeList.Count > 0)
                {
                    evt = getUserEvent(nodeList);
                    evt.setEvtMsg(xml);
                    return evt;
                }
                // Call Event
                nodeList = xmlDoucment.GetElementsByTagName("Dialog");
                if (nodeList.Count > 0)
                {
                    evt = getdialogEvent(nodeList);
                    evt.setEvtMsg(xml);
                    return evt;
                }

                // Call Event
                nodeList = xmlDoucment.GetElementsByTagName("dialog");
                if (nodeList.Count > 0)
                {
                    evt = getdialogEvent(nodeList);
                    evt.setEvtMsg(xml);
                    return evt;
                }

                // Error Event
                nodeList = xmlDoucment.GetElementsByTagName("apiErrors");
                if (nodeList.Count > 0)
                {
                    evt = getErrorEvent(nodeList);
                    evt.setEvtMsg(xml);
                    return evt;
                }


            }
            catch (Exception e)
            {
                logwrite.write("parseXML", e.ToString());
            }

            return evt;
        }

        private Event getErrorEvent(XmlNodeList nodeList)
        {
            ErrorEvent evt = new ErrorEvent();
            evt.setEvtCode(EVENT_TYPE.ERROR);

             XmlNode nodeone = nodeList.Item(0);
             foreach (XmlNode node1 in nodeone.ChildNodes)
             {
                 if (node1.Name.Equals("apiError"))
                 {
                     foreach (XmlNode node2 in node1.ChildNodes)
                     {
                         if (node2.Name.Equals("errorMessage"))
                         {
                             evt.setErrorMessage(node2.InnerText.ToString());
                             //logwrite.write("### EVENT CHECK ###", "ErrorMessage : " + node2.InnerText.ToString());
                         }
                         if (node2.Name.Equals("errorType"))
                         {
                             evt.setErrorType(node2.InnerText.ToString());
                             //logwrite.write("### EVENT CHECK ###", "ErrorType : " + node2.InnerText.ToString());
                         }
                     }
                 }
             }

             return evt;
        }

        private Event getUserEvent(XmlNodeList nodeList)
        {
            AgentEvent evt = new AgentEvent();
            evt.setEvtCode(EVENT_TYPE.ON_AGENTSTATE_CHANGE);

            XmlNode nodeone = nodeList.Item(0);
            foreach (XmlNode node1 in nodeone.ChildNodes)
            {
                if (node1.Name.Equals("state"))
                {
                    evt.setAgentState(node1.InnerText.ToString());
                   // logwrite.write("### EVENT CHECK ###", "Agentstate : " + node1.InnerText.ToString());
                }
                if (node1.Name.Equals("reasonCode"))
                {
                    foreach (XmlNode node2 in node1.ChildNodes)
                    {
                        if (node2.Name.Equals("code"))
                        {
                            evt.setReasonCode(node2.InnerText.ToString());
                           // logwrite.write("### EVENT CHECK ###", "setReasonCode : " + node2.InnerText.ToString());
                        }
                    }
                }
            }
            return evt;
        }

        private Event getdialogEvent(XmlNodeList nodeList) 
        {
            CallEvent evt = null;

            if (nodeList.Count > 0)
            {
                evt = new CallEvent();

                //evt.setEvtCode(EVENT_TYPE.ON_CALL);

                XmlNode nodeone = nodeList.Item(0);
                foreach (XmlNode node1 in nodeone.ChildNodes)
                {
                    if (node1.Name.Equals("fromAddress"))
                    {
                        evt.setFromAddress(node1.InnerText.ToString());
                      //  logwrite.write("### EVENT CHECK ###", "setFromAddress : " + node1.InnerText.ToString());
                    }

                    if (node1.Name.Equals("toAddress"))
                    {
                        evt.setToAddress(node1.InnerText.ToString());
                       // logwrite.write("### EVENT CHECK ###", "setToAddress : " + node1.InnerText.ToString());
                    }

                    if (node1.Name.Equals("id"))
                    {
                        evt.setDialogID(node1.InnerText.ToString());
                       // logwrite.write("### EVENT CHECK ###", "setDialogID : " + node1.InnerText.ToString());
                    }
                    if (node1.Name.Equals("mediaProperties"))
                    {
                        foreach (XmlNode node2 in node1.ChildNodes)
                        {
                            if (node2.Name.Equals("callType"))
                            {
                                evt.setCallType(node2.InnerText.ToString());
                                logwrite.write("### EVENT CHECK ###", "setCallType : " + node2.InnerText.ToString());
                            }
                            if (node2.Name.Equals("callvariables"))
                            {
                                evt.setCallVariable(getCallVarValues(node2));
                                logwrite.write("### EVENT CHECK ###", "setCallVariable : " + getCallVarValues(node2).ToString());
                            }
                        }
                    }
                    if (node1.Name.Equals("participants"))
                    {

                        ArrayList list = getCallState(node1);

                        for (int i = 0; i < list.Count; i++)
                        {
                            Hashtable table = (Hashtable)list[i];
                            string extension = (string)table["mediaAddress"];
                            string state = (string)table["state"];
                            evt.setCallState(extension, state);
                        }

                        Hashtable callStateList = evt.getCallStateTable();
                        string myState = "";
                        int isAllActive = 0;
                        int isAllDrop = 0;


                        foreach (DictionaryEntry item in callStateList)
                        {
                            if (item.Key.Equals(agent.getExtension()))
                            {
                                myState = (string) item.Value;

                                if (!myState.Equals(EVENT_TYPE.ACTIVE) && !myState.Equals(EVENT_TYPE.DROPPED))
                                {
                                    evt.setEvtCode(myState);
                                    break;
                                }
                            }
                            
                            if (item.Value.Equals(EVENT_TYPE.ACTIVE))
                            {
                                isAllActive++;
                            }
                            if (item.Value.Equals(EVENT_TYPE.DROPPED))
                            {
                                isAllDrop++;
                            }
                            
                        }

                        if (callStateList.Count == isAllActive)
                        {
                            evt.setEvtCode(EVENT_TYPE.ESTABLISHED);
                        }
                        if (callStateList.Count == isAllDrop)
                        {
                            evt.setEvtCode(EVENT_TYPE.WRAP_UP);
                        }
                        if (isAllDrop > 0)
                        {
                            evt.setEvtCode(EVENT_TYPE.DROPPED);
                        }

                        if (evt.getEvtCode() == null || evt.getEvtCode().Length == 0)
                        {
                            logwrite.write("########", " !! " + callStateList.Count + " , " + isAllActive + " , " + isAllDrop);
                        }

                        logwrite.write("########", "EVENT CODE -> " + evt.getEvtCode());

                    }

                }
            }

            return evt;
        }

        public string getData(string xml, string tagName)
        {
            try
            {
                if (xml == null)
                {
                    return null;
                }
                    
                logwrite.write("### getData CHECK ###", xml);

                xmlDoucment.LoadXml(xml);

                nodeList = xmlDoucment.GetElementsByTagName(tagName);

                if (nodeList.Count <= 0)
                {
                    return null;
                }

                XmlNode node = nodeList.Item(0);

                return node.InnerText.ToString();

            }
            catch (Exception e)
            {
                logwrite.write("getData", e.ToString());
                return null;
            }
        }


        private ArrayList getCallState(XmlNode node)
        {
            ArrayList list = new ArrayList();
            
            foreach (XmlNode node1 in node.ChildNodes)
            {
                Hashtable table = new Hashtable();
                foreach (XmlNode node2 in node1.ChildNodes)
                {
                    table.Add(node2.Name, node2.InnerText.ToString());
                }
                list.Add(table);
            }

            return list;
        }


        private Hashtable getCallVarValues(XmlNode node)
        {
            Hashtable table = new Hashtable();
            string key = "";
            string value = "";
            foreach (XmlNode tempNode in node.ChildNodes)
            {
                foreach (XmlNode tempNode2 in tempNode.ChildNodes)
                {
                    if (tempNode2.Name.Equals("name")) 
                    {
                        key = tempNode2.InnerText.ToString();
                    }
                    if (tempNode2.Name.Equals("value"))
                    {
                        value = tempNode2.InnerText.ToString();
                    }
                }
                logwrite.write("XMLParser getCallVarValues: ", "Key >>> " + key + " >>> Value >>>> " + value);
                table.Add(key, value);
            }
            return table;
        }

    }
}
