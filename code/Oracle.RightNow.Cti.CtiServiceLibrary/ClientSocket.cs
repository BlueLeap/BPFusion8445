using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using Blueleap.Finesse.Constants;

namespace Blueleap.Finesse.TCPSocket
{
    class ClientSocket
    {
        protected TcpClient sock = null;

        protected NetworkStream writeStream;
        protected StreamReader reader;
        protected StreamWriter writer;

        protected ServerInfo serverInfo;   
        protected Logger logwrite = null;

        private bool isDisconnectReq;

        private bool isSocketConnected;

        protected Hashtable currentServer;

        protected ClientSocket(Logger logwrite)
        {
            this.logwrite = logwrite;
            this.isDisconnectReq = false;
            this.currentServer = new Hashtable();
        }

        public string getCurrentServerIP()
        {
            return (string)currentServer["IP"];
        }
        public int getCurrentServerPort()
        {
            return (int)currentServer["PORT"];
        }

        public void setDisconnectReq(bool isDisconnectReq)
        {
            this.isDisconnectReq = isDisconnectReq;
        }
        public bool getDisconnectReq()
        {
            return this.isDisconnectReq;
        }

        public void setServerInfo(ServerInfo serverInfo)
        {
            this.serverInfo = serverInfo;
        }

        protected int connect(String ip, int port)
        {
            try
            {
                isSocketConnected = false;

                sock = new TcpClient();

                IAsyncResult result = sock.BeginConnect(ip, port, null, null);

                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(CONNECTION.CONNECTION_TIMEOUT), true);

                if (success)
                {
                    writeStream = sock.GetStream();

                    //writeStream.ReadTimeout = 3000;

                    writer = new StreamWriter(writeStream);

                    Encoding encode = System.Text.Encoding.GetEncoding("UTF-8");
                    reader = new StreamReader(writeStream, encode);

                    if (currentServer.ContainsKey("IP"))
                    {
                        currentServer.Remove("IP");
                    }
                    if (currentServer.ContainsKey("PORT"))
                    {
                        currentServer.Remove("PORT");
                    }

                    currentServer.Add("IP", ip);
                    currentServer.Add("PORT", port);

                }
                else
                {
                    return ERRORCODE.SOCKET_CONNECTION_FAIL;
                }
            }
            catch (Exception e)
            {
                logwrite.write("connect", e.ToString());

                if (sock != null)
                {
                    sock = null;
                }

                return ERRORCODE.SOCKET_CONNECTION_FAIL;
            }



            return ERRORCODE.SUCCESS;
        }


        public int disconnect()
        {
            if (sock != null)
            {
                isDisconnectReq = true;
                sock.Close();
                sock = null;
            }
            return ERRORCODE.SUCCESS;
        }

        public int sessionClose()
        {
            logwrite.write("sessionClose", "TCP Session Closed!!!");
            if (sock != null)
            {
                sock.Close();
                sock = null;
            }

            return ERRORCODE.SUCCESS;
        }

        public bool isConnected()
        {
            if (sock != null && sock.Connected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ServerInfo getServerInfo()
        {
            return this.serverInfo;
        }


    }
}
