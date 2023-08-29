using Oracle.RightNow.Cti.Simulator.Messaging.Messages;

namespace Oracle.RightNow.Cti.Simulator {
    internal interface INotificationClient {
        void HandleMessage(Message message);
    }
}