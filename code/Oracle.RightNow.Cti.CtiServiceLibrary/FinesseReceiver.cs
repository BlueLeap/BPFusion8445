using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Blueleap.Finesse.Events;
using Blueleap.Finesse.Constants;
using Blueleap.Finesse.XML;

namespace Blueleap.Finesse.Threading
{
    class FinesseReceiver : ISocketReceiver
    {
        private TcpClient sock = null;
        private NetworkStream writeStream;

        private StreamReader reader;
        private Logger logwrite;
        private Finesse finesseObj;

        private XMLParser xmlParser;

        private Agent agent;
        private FinesseClient finesseClient;

        public FinesseReceiver(StreamReader reader, Finesse finesseObj)
        {
            this.reader = reader;
            this.logwrite = Logger.getInstance();
            this.finesseObj = finesseObj;   
            this.xmlParser = new XMLParser(logwrite, null);
        }

        public FinesseReceiver(TcpClient sock, Finesse finesseObj, Agent agent, FinesseClient finesseClient)
        {
            this.sock = sock;
            this.writeStream = sock.GetStream();
            Encoding encode = System.Text.Encoding.GetEncoding("UTF-8");
            this.reader = new StreamReader(writeStream, encode);
            this.logwrite = Logger.getInstance();
            this.finesseObj = finesseObj;  
            this.xmlParser = new XMLParser(logwrite, agent);
            this.agent = agent;
            this.finesseClient = finesseClient;
        }

        public void runThread()
        {
            try
            {

                logwrite.write("FinesseReceiver runThread", " Finesse Recv Thread Start !!");

                writeStream.ReadTimeout = Timeout.Infinite;

                Event evt = null;

                if (writeStream == null)
                {
                    logwrite.write("FinesseReceiver runThread", "writeStream null");
                }
                
                int BUFFERSIZE = sock.ReceiveBufferSize;
                byte[] buffer = new byte[BUFFERSIZE];
                int bytelen = 0;

                StringBuilder sb = new StringBuilder();


                
                while ((bytelen = writeStream.Read(buffer, 0, buffer.Length)) > 0)
                {

                    string message = Encoding.UTF8.GetString(buffer, 0, bytelen);
                    message = message.Replace("&lt;", "<");
                    message = message.Replace("&gt;", ">");
                    message = message.Replace("\n", "");
                    message = message.Trim();

                    //Console.WriteLine(message);
                    logwrite.write("FinesseReceiver runThread", message.Replace("\n", ""));

                    int endIndex = 0;
                    int subLength = 0;

                    while (message.Length > 0)
                    {
                        //Console.WriteLine("message Len : " + message.Length);
                        if (message.StartsWith("<message"))
                        {
                            endIndex = message.IndexOf("</message>");
                            if (endIndex > -1)
                            {
                                subLength = endIndex + "</message>".Length;
                                string resultStr = message.Substring(0, subLength);

                                evt = xmlParser.parseXML(resultStr);
                                finesseObj.raiseEvent(evt);
                                //Console.WriteLine("\n\n1. result -> " + resultStr);
                                message = message.Substring(subLength, message.Length - subLength);
                            }
                            else
                            {
                                sb.Append(message);
                                break;
                            }
                        }
                        else
                        {
                            endIndex = message.IndexOf("</message>");
                            if (endIndex > -1)
                            {
                                subLength = endIndex + "</message>".Length;
                                string resultStr = message.Substring(0, subLength);

                                if (sb.ToString().Length > 0)
                                {
                                    sb.Append(resultStr);
                                    resultStr = sb.ToString().Replace("&gt;", ">").Replace("&lt;", "<");
                                    evt = xmlParser.parseXML(resultStr);
                                    finesseObj.raiseEvent(evt);
                                    //Console.WriteLine("\n\n2. result -> " + sb.ToString());
                                    sb = new StringBuilder();
                                }
                                message = message.Substring(subLength, message.Length - subLength);
                            }
                            else
                            {
                                if (sb.ToString().Length > 0)
                                {
                                    sb.Append(message);
                                    break;
                                }
                                else
                                {
                                    sb = new StringBuilder();
                                    break;
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }

                if (writeStream != null)
                {
                    writeStream.Close();
                    writeStream = null;
                }
                logwrite.write("FinesseReceiver runThread", e.ToString());

            }
            finally
            {

                finesseClient.sessionClose();
                finesseClient.setXMPPAuth(false);   

                if (!finesseClient.getDisconnectReq())
                {
                    logwrite.write("FinesseReceiver runThread", "########## Finesse Session Closed !! ##########");

                    Event evt = new ErrorEvent();
                    evt.setEvtCode(EVENT_TYPE.ON_DISCONNECTION);
                    evt.setEvtMsg("Finesse Session Disconnected");
                    evt.setCurFinesseIP(finesseClient.getCurrentServerIP());
                    finesseObj.raiseEvent(evt);


                    if (finesseClient.finesseReConnect() == ERRORCODE.SUCCESS)
                    {
                        logwrite.write("FinesseReceiver runThread", " TRY TO CHECK AGENT PREVIOUS STATE");
                        finesseClient.checkAgentState();
                    }
                    else
                    {
                        ISocketSender finesseSender = new FinesseSender(logwrite, finesseClient);
                        ThreadStart ts = new ThreadStart(finesseSender.runThread);
                        Thread thread = new Thread(ts);
                        thread.Start();
                    }
                }
            }

        }

        private string getRootDoc(string xml)
        {
            string returnStr = null;

            int index = xml.IndexOf("</message>");

            if(index > 0) {

                string tempStr = xml.Substring(0, index + "</message>".Length);

                if (tempStr.Length > 0)
                {
                    return tempStr;
                }

            }
            return returnStr;
        }


    }
}
