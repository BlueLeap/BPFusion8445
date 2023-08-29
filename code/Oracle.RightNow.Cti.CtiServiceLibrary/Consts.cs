using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blueleap.Finesse.Constants
{
    class ERRORCODE
    {
        public readonly static int SUCCESS = 0;
        public readonly static int SOCKET_CONNECTION_FAIL = -100;
        public readonly static int FAIL = -1;
        public readonly static int LOGIN_FAIL = -3;
    }

    class CONNECTION
    {
        public readonly static int CONNECTION_TIMEOUT = 3000;
    }

    class AGENTSTATE
    {
        public readonly static string NOT_READY = "NOT_READY";
        public readonly static string READY = "READY";
        public readonly static string LOGOUT = "LOGOUT";
    }

    public class SERVERINFO
    {
        public static string Finesse_PORT = "8445";
        public static string Finesse_PROTOCOL = "https://";
    }

    class CALL
    {
        public readonly static string MAKE_CALL = "MAKE_CALL";
    }

    class EVENT_TYPE
    {
        public const string ALERTING = "ALERTING";
        public const string ESTABLISHED = "ESTABLISHED";
        public const string DROPPED = "DROPPED";
        public const string WRAP_UP = "WRAP_UP";
        public const string ACTIVE = "ACTIVE";
        public const string FAILED = "FAILED";
        public const string ON_AGENTSTATE_CHANGE = "ON_AGENTSTATE_CHANGE";
        public const string ON_CONNECTION = "ON_CONNECTION";
        public const string ON_DISCONNECTION = "ON_DISCONNECTION";
        public const string INITIATING = "INITIATING";
        public const string INITIATED = "INITIATED";
        public const string ERROR = "ERROR";
        public const string HELD = "HELD";
        public const string CANNOT_CONNECT_FINESSE = "CANNOT_CONNECT_FINESSE";
    }

    class SECURITYCODE
    {
        public readonly static int Tls11 = 768;
        public readonly static int Tls12 = -3027;
    }
}
