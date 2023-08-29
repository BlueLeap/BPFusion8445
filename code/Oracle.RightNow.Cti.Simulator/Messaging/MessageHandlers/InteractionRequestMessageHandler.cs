// ===========================================================================================
//  Oracle RightNow Connect
//  CTI Sample Code
// ===========================================================================================
//  Copyright © Oracle Corporation.  All rights reserved.
// 
//  Sample code for training only. This sample code is provided "as is" with no warranties 
//  of any kind express or implied. Use of this sample code is pursuant to the applicable
//  non-disclosure agreement and or end user agreement and or partner agreement between
//  you and Oracle Corporation. You acknowledge Oracle Corporation is the exclusive
//  owner of the object code, source code, results, findings, ideas and any works developed
//  in using this sample code.
// ===========================================================================================
using System;
using System.ComponentModel.Composition;
using System.Linq;
using Oracle.RightNow.Cti.Providers.Simulator;
using Oracle.RightNow.Cti.Providers.Simulator.Messaging;
using Oracle.RightNow.Cti.Simulator.Messaging.Messages;

namespace Oracle.RightNow.Cti.Simulator.Messaging.MessageHandlers {
    [Export(typeof(IMessageHandler))]
    public class InteractionRequestMessageHandler : IMessageHandler {
        public void HandleMessage(SimulatedSwitch @switch, Message message) {
            var stateMessage = message as InteractionRequestMessage;
            if (stateMessage != null) {
                var interaction = @switch.Interactions.FirstOrDefault(i => i.Id == stateMessage.InteractionId);

                switch (stateMessage.Action) {
                    case InteractionRequestAction.Accept:
                        interaction.Agent.State = SwitchAgentState.HandlingInteraction;
                        notifyAgentState(interaction.Agent.Device, SwitchAgentState.HandlingInteraction, @switch);
                        
                        interaction.State = SwitchInteractionState.Active;
                        @switch.SendMessage(interaction.Agent.Device, new InteractionMessage {
                            Interaction = interaction,
                            Action = InteractionMessageAction.Assigned
                        });
                        break;
                    case InteractionRequestAction.Hold:
                        notifyInteractionState(@switch, interaction, SwitchInteractionState.Held);
                        break;
                    case InteractionRequestAction.Retrieve:
                        notifyInteractionState(@switch, interaction, SwitchInteractionState.Active);
                        break;
                    case InteractionRequestAction.Disconnect:
                        interaction.State = Providers.Simulator.SwitchInteractionState.Disconnected;
                        notifyInteractionState(@switch, interaction, SwitchInteractionState.Disconnected);
                        
                        interaction.Agent.State = SwitchAgentState.WrapUp;
                        notifyAgentState(interaction.Agent.Device, SwitchAgentState.WrapUp, @switch);
                        break;
                    case InteractionRequestAction.Complete:
                        if (interaction.State != SwitchInteractionState.Disconnected) {
                            interaction.Agent.State = SwitchAgentState.WrapUp;
                            notifyAgentState(interaction.Agent.Device, SwitchAgentState.WrapUp, @switch);
                        }
                        
                        interaction.Agent.Interactions.Remove(interaction);
                        @switch.Interactions.Remove(interaction);
                        notifyInteractionState(@switch, interaction, SwitchInteractionState.Completed);
                        break;
                    default:
                        break;
                }
            }
        }
  
        private void notifyAgentState(Device device, SwitchAgentState state, SimulatedSwitch @switch) {
            var agentStateMessage = new AgentStateMessage {
                AgentId = device.Agent.Id,
                DeviceId = device.Id,
                State = state,
            };

            @switch.SendMessage(device, agentStateMessage);
        }

        private void notifyInteractionState(SimulatedSwitch @switch, SwitchInteraction interaction, SwitchInteractionState state) {
            interaction.State = state;
            @switch.SendMessage(interaction.Agent.Device, new InteractionMessage { Interaction = interaction, Action = InteractionMessageAction.StateChanged });
        }

        public SwitchMessageType MessageType {
            get {
                return SwitchMessageType.InteractionRequest;
            }
        }
    }
}