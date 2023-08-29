using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
  using Oracle.RightNow.Cti.Model;
using RightNow.AddIns.AddInViews;

namespace Oracle.RightNow.Cti.Configuration {
    [Export(typeof(IConfigurationProvider))]
    public class RightNowConnectConfigurationProvider : IConfigurationProvider {
        private RightNowObjectProvider _objectProvider;

        [ImportingConstructor]
        public RightNowConnectConfigurationProvider(IGlobalContext globalContext) {
            _objectProvider = new RightNowObjectProvider(globalContext);
        }

        public IEnumerable<AgentState> GetAgentStates() {
            // Get configured states
            //Pratik: Do not get from RN use Finesse one defined in standard agent state.
            //var states = new List<AgentState>(_objectProvider.GetObjects<AgentState>());

            var states = new List<AgentState>();

            // Add standard states. These are Finesse's Agents State

            states.Add(StandardAgentStates.Available);
            states.Add(StandardAgentStates.WrapUp);
            states.Add(StandardAgentStates.NotReady);
            states.Add(StandardAgentStates.NewReason);
            states.Add(StandardAgentStates.LoggedIn);
            states.Add(StandardAgentStates.LoggedOut);
            states.Add(StandardAgentStates.Reserved);
            states.Add(StandardAgentStates.Talking);
            states.Add(StandardAgentStates.Unknown);
            states.Add(StandardAgentStates.Connecting);
            states.Add(StandardAgentStates.Hold);
            return states;
        }
    }
}
