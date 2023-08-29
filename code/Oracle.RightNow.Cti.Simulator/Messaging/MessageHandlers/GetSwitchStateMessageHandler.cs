using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Oracle.RightNow.Cti.Providers.Simulator;
using Oracle.RightNow.Cti.Providers.Simulator.Messaging;
using Oracle.RightNow.Cti.Simulator.Messaging.Messages;

namespace Oracle.RightNow.Cti.Simulator.Messaging.MessageHandlers {
    [Export(typeof(IMessageHandler))]
    public class GetSwitchStateMessageHandler : IMessageHandler {
        public void HandleMessage(SimulatedSwitch @switch, Message message) {
            var switchMessage = message as SwitchMessage;
            if (switchMessage != null) {
                var device = @switch.Devices.FirstOrDefault(d => d.Id == switchMessage.DeviceId);

                var response = new SwitchStateMessage { Devices = new List<Device>(@switch.Devices) };

                @switch.SendMessage(device, response);
            }
        }

        public SwitchMessageType MessageType {
            get {
                return SwitchMessageType.GetSwitchState;
            }
        }
    }
}