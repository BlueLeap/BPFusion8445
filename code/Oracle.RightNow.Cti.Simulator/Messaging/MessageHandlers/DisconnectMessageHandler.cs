using System.ComponentModel.Composition;
using System.Linq;
using Oracle.RightNow.Cti.Providers.Simulator.Messaging;
using Oracle.RightNow.Cti.Simulator.Messaging.Messages;

namespace Oracle.RightNow.Cti.Simulator.Messaging.MessageHandlers {
    [Export(typeof(IMessageHandler))]
    class DisconnectMessageHandler : IMessageHandler {
        public void HandleMessage(SimulatedSwitch @switch, Message message) {
            var disconnectMessage = message as SwitchMessage;
            if (disconnectMessage != null) {
                @switch.SendMessage(null, disconnectMessage);

                var device = @switch.Devices.FirstOrDefault(d=>d.Id == disconnectMessage.DeviceId);
                if (device != null)
                    @switch.Devices.Remove(device);
            }
        }

        public SwitchMessageType MessageType {
            get {
                return SwitchMessageType.Disconnected;
            }
        }
    }
}