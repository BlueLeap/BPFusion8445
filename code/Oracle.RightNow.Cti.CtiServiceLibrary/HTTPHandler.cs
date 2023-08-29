using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;
using System.Net;
using Blueleap.Finesse.Constants;
using Blueleap.Finesse.XML;

namespace Blueleap.Finesse.HTTP
{
    class HTTPHandler
    {
        private HttpWebRequest request;
        private HttpWebResponse response;
        private StreamWriter writer;
        private StreamReader reader;

        private XMLHandler xmlHandler;
        private FinesseURLHandler urlHandler;

        private Logger logwrite;

        private string URL;
        //private string serverIP;

        public HTTPHandler(Logger logwrite)
        {
            this.logwrite = logwrite;
            this.xmlHandler = new XMLHandler();
            this.urlHandler = new FinesseURLHandler();

            //PK: To get past invlid certificates. Need to remove at a later stage after testing.
            ServicePointManager.ServerCertificateValidationCallback =
delegate (object s, X509Certificate certificate,
X509Chain chain, SslPolicyErrors sslPolicyErrors)
{
    return true;
};

        }


        public string requestGETAPI(string url, Agent agent, string methodType)
        {
            this.URL = url;

            try
            {

                request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = methodType;
                System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | (SecurityProtocolType)192;
                request.Timeout = 300000;
                string basicEncode = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(agent.getAgentID() + ":" + agent.getAgentPwd()));

                request.Headers.Add("Authorization", "Basic " + basicEncode);

                logwrite.write("requestRESTAPI", "============================= REQUEST REST API =================================");
                logwrite.write("requestRESTAPI", "  URL \t : " + URL);
                logwrite.write("requestRESTAPI", "  METHOD \t : " + methodType);
                logwrite.write("requestRESTAPI", "  basicEncode \t : " + basicEncode);

                response = (HttpWebResponse)request.GetResponse();
                int code = Convert.ToInt32(response.StatusCode);


                Stream webStream = response.GetResponseStream();
                reader = new StreamReader(webStream);

                string responseStr = reader.ReadToEnd();

                logwrite.write("requestRESTAPI", "============================= RESPONSE REST API =================================");
                logwrite.write("requestRESTAPI", "  code \t : " + code);
                logwrite.write("requestRESTAPI", "  DATA \t : " + responseStr);
                logwrite.write("requestRESTAPI", "=================================================================================");

                reader.Close();

                return responseStr;

            }
            catch (Exception e)
            {
                logwrite.write("requestRESTAPI", e.ToString());
                return null;
            }

        }


        public int requestRESTAPI(string url, Agent agent, string methodType, string requestData)
        {
            this.URL = url;

            try
            {

                request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = methodType;
                if (!methodType.Equals("GET"))
                {
                    request.ContentType = "application/xml";
                    request.ContentLength = requestData.Length;
                }
                string basicEncode = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(agent.getAgentID() + ":" + agent.getAgentPwd()));

                request.Headers.Add("Authorization", "Basic " + basicEncode);

                logwrite.write("requestRESTAPI", "============================= REQUEST REST API =================================");
                logwrite.write("requestRESTAPI", "  URL \t : " + URL);
                logwrite.write("requestRESTAPI", "  METHOD \t : " + methodType);
                logwrite.write("requestRESTAPI", "  DATA \t : " + requestData);
                logwrite.write("requestRESTAPI", "  basicEncode \t : " + basicEncode);

                if (!methodType.Equals("GET"))
                {
                    writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(requestData);
                    writer.Close();
                }

                response = (HttpWebResponse)request.GetResponse();
                int code = Convert.ToInt32(response.StatusCode);


                Stream webStream = response.GetResponseStream();
                reader = new StreamReader(webStream);

                string responseStr = reader.ReadToEnd();

                logwrite.write("requestRESTAPI", "============================= RESPONSE REST API =================================");
                logwrite.write("requestRESTAPI", "  code \t : " + code);
                logwrite.write("requestRESTAPI", "  DATA \t : " + responseStr);
                logwrite.write("requestRESTAPI", "=================================================================================");

                reader.Close();


            }
            catch (Exception e)
            {
                logwrite.write("requestRESTAPI", e.ToString());
                return ERRORCODE.FAIL;
            }

            return ERRORCODE.SUCCESS;
        }


        public string checkAgentState(string serverIP, Agent agent)
        {
            return requestGETAPI(urlHandler.getUserURL(serverIP, agent), agent, "GET");
        }

        public string reasonCodeRequest(string serverIP, Agent agent)
        {
            return requestGETAPI(urlHandler.getReasonCodeURL(serverIP, agent), agent, "GET");
        }

        public int agentStateChangeRequest(string serverIP, Agent agent, string state)
        {
            return requestRESTAPI(urlHandler.getUserURL(serverIP, agent), agent, "PUT", xmlHandler.getAgentState(state));
        }
        public int agentStateChangeRequest(string serverIP, Agent agent, string state, string reasonCode)
        {
            return requestRESTAPI(urlHandler.getUserURL(serverIP, agent), agent, "PUT", xmlHandler.getAgentState(state, reasonCode));
        }

        public int makeCallRequest(string serverIP, Agent agent, string dialNumber)
        {
            return requestRESTAPI(urlHandler.getDialogURL(serverIP, agent), agent, "POST", xmlHandler.getMakeCall(agent.getExtension(), dialNumber));
        }
    }
}
