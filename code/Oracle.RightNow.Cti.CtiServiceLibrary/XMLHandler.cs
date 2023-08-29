using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blueleap.Finesse.Constants;

namespace Blueleap.Finesse.XML
{
    class XMLHandler
    {
        public string getMakeCall(string extension , string dialNumber)
        {
            return "<Dialog><requestedAction>"+CALL.MAKE_CALL+"</requestedAction><fromAddress>" + extension + "</fromAddress><toAddress>" + dialNumber + "</toAddress></Dialog>";
        }
        public string getAgentState(string state)
        {
            return "<User><state>"+state+"</state></User>";
        }
        public string getAgentState(string state, string reasonCode)
        {
            return "<User><state>" + state + "</state><reasonCodeId>" + reasonCode + "</reasonCodeId></User>";
        }
    }
}
