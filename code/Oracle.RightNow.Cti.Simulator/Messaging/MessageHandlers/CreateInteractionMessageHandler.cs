using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Oracle.RightNow.Cti.Providers.Simulator.Messaging;
using Oracle.RightNow.Cti.Simulator.Messaging.Messages;

namespace Oracle.RightNow.Cti.Simulator.Messaging.MessageHandlers {
    [Export(typeof(IMessageHandler))]
    public class CreateInteractionMessageHandler : IMessageHandler {
        public void HandleMessage(SimulatedSwitch @switch, Message message) {
            var createInteractionMessage = message as CreateInteractionMessage;
            if (createInteractionMessage != null) {
                @switch.QueueInteraction(createInteractionMessage.Interaction);
            }
        }

        public SwitchMessageType MessageType {
            get {
                return SwitchMessageType.CreateInteraction;
            }
        }
    }
}
