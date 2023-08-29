using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Blueleap.Finesse.Constants;
using System.Net.Sockets;
namespace Blueleap.Finesse.Threading
{
    class FinesseSender : ISocketSender
    {
        private Logger logwrite;
        private TcpClient sock = null;
        private FinesseClient finesseClient;

        public FinesseSender(Logger logwrite, FinesseClient finesseClient)
        {
            this.logwrite = logwrite;
            this.finesseClient = finesseClient;
        }


        public void pingCheck()
        {

        }

        public void runThread()
        {

            ServerInfo serverInfo = finesseClient.getServerInfo();

            int port = serverInfo.getPort();

            bool connectSuccess = false;

            while (!connectSuccess)
            {
                string ip = serverInfo.getIP();

                sock = new TcpClient();

                var result = sock.BeginConnect(ip, port, null, null);

                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(CONNECTION.CONNECTION_TIMEOUT));

                if (sock != null && sock.Connected)
                {
                    connectSuccess = true;
                    logwrite.write("Finesse Sender", "Connection SUCCESS IP [" + ip + "] PORT [" + port + "]");
                    break;
                }
                else
                {
                    logwrite.write("Finesse Sender", "Connection Fail IP [" + ip + "] PORT [" + port + "]");
                }
            }

            if (sock != null)
            {
                sock.Close();
            }

            finesseClient.finesseReConnect();
        }
    }
}
