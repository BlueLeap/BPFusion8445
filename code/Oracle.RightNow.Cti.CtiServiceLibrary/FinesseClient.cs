using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using Blueleap.Finesse;
using Blueleap.Finesse.Threading;
using Blueleap.Finesse.TCPSocket;
using Blueleap.Finesse.HTTP;
using Blueleap.Finesse.Constants;
using Blueleap.Finesse.Utilities;
using Blueleap.Finesse.Events;
using Blueleap.Finesse.XML;

namespace Blueleap.Finesse
{
    class FinesseClient : ClientSocket
    {
        private Finesse finesseObj; 
        private ISocketReceiver finesseRecv;
        //private ISocketSender finesseSend;
        private Agent agent;
        private HTTPHandler httpHandler;

        private bool isAlreadyAuth;     //  If the XMPP authentication process is required to receive the event, and the XMPP session is not terminated, then the XMPP authentication must be received only once.

        public FinesseClient(Logger logwrite, Finesse finesseObj) : base(logwrite)
        {
            this.finesseObj = finesseObj;
            this.isAlreadyAuth = false;
        }

        public void setXMPPAuth(bool isAlreadyAuth)
        {
            this.isAlreadyAuth = isAlreadyAuth;
        }


        public int reConnect()
        {
            if (sock != null && sock.Connected)
            {
                logwrite.write("reConnect", "Finesse Already Connected !!");
                return ERRORCODE.FAIL;
            }
            if (finesseConnect() == ERRORCODE.SUCCESS)
            {
                return connectXMPPAuth();
            }
            else
            {
                return ERRORCODE.FAIL;
            }

        }

        public int finesseConnect()
        {
            Boolean bisConnected = false;
            String serverIP = serverInfo.getIP();

            logwrite.write("startClient", "Finesse Try Connection [" + serverIP + "][" + serverInfo.getPort() + "]");

            if (connect(serverIP, 5222) == ERRORCODE.SUCCESS)
            {
                logwrite.write("startClient", "Finesse Connection SUCCESS!! [" + serverIP + "][" + serverInfo.getPort() + "]");

                bisConnected = true;
                finesseObj.setFinesseConnected(true);
            }
            else
            {
                finesseObj.setFinesseConnected(false);
                bisConnected = false;
            }

            return bisConnected ? ERRORCODE.SUCCESS : ERRORCODE.SOCKET_CONNECTION_FAIL;
        }

        public int finesseReConnect()
        {
            if (sock != null && sock.Connected)
            {
                logwrite.write("finesseReConnect", "Finesse Already Connected !!");
                return ERRORCODE.FAIL;
            }
            if (finesseConnect() == ERRORCODE.SUCCESS)
            {
                return connectXMPPAuth();
            }
            else
            {
                logwrite.write("finesseReConnect", "Finesse ReConnection FAIL !! ");
                return ERRORCODE.FAIL;
            }
        }

        public int startClient()
        {

            if (isConnected())
            {
                logwrite.write("startClient", "Finesse Already Connected !!");
                return ERRORCODE.SUCCESS;
            }

            return finesseConnect();

        }

        public int connectXMPPAuth()
        {
            this.agent = Agent.getInstance();


            if (!isAlreadyAuth)
            {
                if (startPreProcess() != ERRORCODE.SUCCESS)
                {
                    return ERRORCODE.LOGIN_FAIL;
                }
            }
            else
            {
                logwrite.write("login", "## Finesse Authentication is Already Success ##");
            }

            isAlreadyAuth = true;


            finesseRecv = new FinesseReceiver(sock, finesseObj, agent, this);
            ThreadStart recvts = new ThreadStart(finesseRecv.runThread);
            Thread recvThread = new Thread(recvts);
            recvThread.Start();


            callConnectionEvent();  

            return ERRORCODE.SUCCESS;
        }

        public int checkAgentState()
        {

            if (httpHandler == null)
            {
                httpHandler = new HTTPHandler(logwrite);
            }
            string agentState = "";
            string agentReasonCode = "";
            string agentStateXml = httpHandler.checkAgentState((string)currentServer["IP"], agent);
            if (agentStateXml != null)
            {
                XMLParser xmlParser = new XMLParser(logwrite, agent);

                agentStateXml = agentStateXml.Replace("\n", "");

                agentState = xmlParser.getData(agentStateXml, "state");
                agentReasonCode = xmlParser.getData(agentStateXml, "code");

                logwrite.write("login", "CURRENT AGENT STATE : " + agentState + " , REASON CODE : " + agentReasonCode);

                //if (!agentState.Equals(AGENTSTATE.LOGOUT))
                {
                    AgentEvent evt = new AgentEvent();
                    evt.setEvtMsg(agentStateXml);
                    evt.setAgentState(agentState);
                    evt.setReasonCode(agentReasonCode);
                    evt.setEvtCode(EVENT_TYPE.ON_AGENTSTATE_CHANGE);
                    finesseObj.raiseEvent(evt);
                }

                return ERRORCODE.SUCCESS;
            }
            else
            {
                return ERRORCODE.FAIL;
            }
        }

        public int login()
        {

            if (connectXMPPAuth() != ERRORCODE.SUCCESS)
            {
                return ERRORCODE.FAIL;
            }

            return checkAgentState();

        }

        public int makeCall(string dialNumber)
        {
            if (httpHandler == null)
            {
                httpHandler = new HTTPHandler(logwrite);
            }

            return httpHandler.makeCallRequest((string)currentServer["IP"], agent, dialNumber);
        }

        public void callConnectionEvent()
        {
            ErrorEvent evt = new ErrorEvent();
            evt.setEvtCode(EVENT_TYPE.ON_CONNECTION);
            evt.setCurFinesseIP((string)currentServer["IP"]);
            evt.setEvtMsg("Finesse Connection Success!!");
            finesseObj.raiseEvent(evt);
        }

        public string getReasonCodeList()
        {
            if (httpHandler == null)
            {
                httpHandler = new HTTPHandler(logwrite);
            }
            return httpHandler.reasonCodeRequest((string)currentServer["IP"], agent);
        }

        public int agentState(string state)
        {
            if (httpHandler == null)
            {
                httpHandler = new HTTPHandler(logwrite);
            }

            return httpHandler.agentStateChangeRequest((string)currentServer["IP"], agent, state);
        }

        public int agentState(string state, string reasonCode)
        {
            if (httpHandler == null)
            {
                httpHandler = new HTTPHandler(logwrite);
            }

            return httpHandler.agentStateChangeRequest((string)currentServer["IP"], agent, state, reasonCode);
        }

        private int startPreProcess()
        {

            try
            {

                int tempindex = 0;

                FinesseDomain domain = FinesseDomain.getInstance();

                Util util = new Util();
                string strID = "blueleapmediabar";
                Random random = new Random();
                int ranNum = random.Next(1, 10);

                string strMsg = @"<?xml version='1.0' ?><stream:stream to='" + (string)currentServer["IP"] + "' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' version='1.0'>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }

                strMsg = @"<auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='PLAIN' xmlns:ga='http://www.google.com/talk/protocol/auth' ga:client-uses-full-bind-result='true'>" + util.AuthBase64_IDAndPw(agent.getAgentID().ToLower(), agent.getAgentPwd()) + "</auth>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }

                strMsg = @"<stream:stream to='" + (string)currentServer["IP"] + "' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' version='1.0'>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }

                strMsg = @"<iq type='set' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "'><bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'><resource>blueleap</resource></bind></iq>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }
                ranNum++;

                strMsg = @"<iq type='set' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "'><session xmlns='urn:ietf:params:xml:ns:xmpp-session'/></iq>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }
                ranNum++;

                strMsg = @"<iq type='get' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "' to='" + domain.getFinesseDomain() + "'><query xmlns='http://jabber.org/protocol/disco#items'/></iq>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }
                ranNum++;


                strMsg = @"<iq type='get' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "' to='" + domain.getFinesseDomain() + "'><query xmlns='http://jabber.org/protocol/disco#info'/></iq>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }
                ranNum++;


                strMsg = @"<iq type='get' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "'><vCard xmlns='vcard-temp'/></iq>";
                strMsg += @"<iq type='get' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "'><query xmlns='jabber:iq:roster'/></iq>";
                strMsg += @"<iq type='get' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "' to='" + domain.getFinesseDomain() + "'><query xmlns='http://jabber.org/protocol/disco#items' node='http://jabber.org/protocol/commands'/></iq>";
                strMsg += @"<iq type='get' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "' to='proxy.eu.jabber.org'><query xmlns='http://jabber.org/protocol/bytestreams'/></iq>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }
                ranNum++;

                strMsg = @"<iq type='get' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "' to='proxy." + domain.getFinesseDomain() + "'><query xmlns='http://jabber.org/protocol/bytestreams'/></iq>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }
                ranNum++;

                strMsg = @"<presence><priority>1</priority><c xmlns='http://jabber.org/protocol/caps' node='http://pidgin.im/' hash='sha-1' ver='I22W7CegORwdbnu0ZiQwGpxr0Go='/><x xmlns='vcard-temp:x:update'><photo/></x></presence>";
                strMsg += @"<iq type='set' id='" + strID + util.lpad(Convert.ToString(ranNum), "a", 3) + "'><pubsub xmlns='http://jabber.org/protocol/pubsub'><publish node='http://jabber.org/protocol/tune'><item><tune xmlns='http://jabber.org/protocol/tune'/></item></publish></pubsub></iq>";
                send(strMsg);
                if (recv(tempindex++) != ERRORCODE.SUCCESS) { return ERRORCODE.FAIL; }
                ranNum++;
            }
            catch (Exception e)
            {
                Task.Factory.StartNew(() =>
                {
                    logwrite.write("startPreProcess", e.ToString());
                    ErrorEvent evt = new ErrorEvent();
                    evt.setEvtCode(EVENT_TYPE.CANNOT_CONNECT_FINESSE);
                    evt.setCurFinesseIP((string)currentServer["IP"]);
                    evt.setEvtMsg("Cannot connnect to Finesse");
                    finesseObj.raiseEvent(evt);
                });
                return ERRORCODE.FAIL;
            }

            return ERRORCODE.SUCCESS;
        }


        private void send(String msg)
        {

            if (sock == null || !sock.Connected)
            {
                if (finesseReConnect() == ERRORCODE.SUCCESS)
                {
                    logwrite.write("send", msg);
                    writer.WriteLine(msg);
                    writer.Flush();
                }
            }
            else
            {
                logwrite.write("send", msg);
                writer.WriteLine(msg);
                writer.Flush();
            }
        }

        private int recv(int tempIndex)
        {

            //int BUFFERSIZE = sock.ReceiveBufferSize;
            byte[] buffer = new byte[32768];
            //int bytes = writeStream.Read(buffer, 0, buffer.Length);

            writeStream.ReadTimeout = 3000;

            int read;

            try
            {

                read = writeStream.Read(buffer, 0, buffer.Length);
                if (read > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, read);
                    logwrite.write("recv", message);

                    if (message.Contains("stream:stream") && tempIndex == 0)
                    {
                        int startIndex = message.IndexOf("<stream:stream");
                        int messageLen = message.Length;
                        string tempStr = message.Substring(startIndex, messageLen - startIndex);

                        startIndex = tempStr.IndexOf("from=");
                        tempStr = tempStr.Substring(startIndex, tempStr.Length - startIndex);

                        startIndex = 0;
                        int endIndex = 0;
                        int tempInt = 0;
                        for (int i = 0; i < tempStr.Length; i++)
                        {
                            string str = tempStr.Substring(i, 1);
                            if (str.Equals("\""))
                            {
                                tempInt++;
                                if (tempInt == 1)
                                {
                                    startIndex = i + 1;
                                }
                                else if (tempInt == 2)
                                {
                                    endIndex = i;
                                    break;
                                }

                            }
                        }

                        tempStr = tempStr.Substring(startIndex, endIndex - startIndex);
                        logwrite.write("recv", " ** Finesse Domain ** : [" + tempStr + "]");

                        FinesseDomain domain = FinesseDomain.getInstance();
                        domain.setFinesseDomain(tempStr);

                    }
                }
                else
                {
                    logwrite.write("recv", "return bytes size -> " + read);
                }
            }
            catch (Exception e)
            {
                if (sock != null)
                {
                    sessionClose();
                }
                return ERRORCODE.FAIL;
            }
            return ERRORCODE.SUCCESS;
        }
    }
}
