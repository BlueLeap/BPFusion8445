using Oracle.RightNow.Cti.Simulator;
using Oracle.RightNow.Cti.Simulator.Messaging;
using Oracle.RightNow.Cti.Simulator.Messaging.Messages;

namespace Oracle.RightNow.Cti.Providers.Simulator.Messaging {
    public interface IMessageHandler {
        void HandleMessage(SimulatedSwitch @switch, Message message);

        SwitchMessageType MessageType { get; }
    }
}