using Oracle.RightNow.Cti.Simulator.Messaging.Messages;

namespace Oracle.RightNow.Cti.Providers.Simulator {
    internal interface ISwitchMessageConsumer {
        void HandleMessage(Message message);
    }
}