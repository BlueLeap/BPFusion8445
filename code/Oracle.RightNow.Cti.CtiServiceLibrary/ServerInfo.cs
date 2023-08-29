using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Blueleap.Finesse
{
    class ServerInfo
    {
        private int port;
        private string serverDomain;

        public ServerInfo(int port, string domain)
        {
            this.port = port;
            this.serverDomain = domain;
        }

        public int getPort()
        {
            return this.port;
        }

        public string getIP()
        {
            return serverDomain;
        }
    }
}
