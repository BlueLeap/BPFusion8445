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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using Oracle.RightNow.Cti.Model;
using Oracle.RightNow.Cti.Simulator;
using Oracle.RightNow.Cti.Simulator.Messaging;
using Oracle.RightNow.Cti.Simulator.Messaging.Messages;

namespace Oracle.RightNow.Cti.Providers.Simulator {
    [Export(typeof(IAgent))]
    internal class SimulatedAgent : IAgent, ISwitchMessageConsumer {
        private AgentState _currentState;
        private IInteractionProvider _interactionProvider;
        
        public event EventHandler DataChanged;
        public event EventHandler<LoginErrorEventArgs> LoginError;
        
        public event EventHandler NameChanged;

        public event EventHandler<AgentStateChangedEventArgs> StateChanged;

        public SimulatedAgent() {
            CurrentState = StandardAgentStates.LoggedOut;
            Id = Guid.NewGuid();
        }

        public AgentState CurrentState {
            get {
                return _currentState;
            }
            set {
                if (_currentState != value) {
                    var oldState = value;
                    _currentState = value;
                    OnStateChanged(new AgentStateChangedEventArgs(oldState, _currentState));
                }
            }
        }

        public Guid Id { get; set; }

        [Import]
        public InteractionManager InteractionManager { get; set; }

        [Import]
        public IInteractionProvider InteractionProvider {
            get {
                return _interactionProvider;
            }
            set {
                if (_interactionProvider != value) {
                    if (_interactionProvider is ISwitchMessageProducer) {
                        ((ISwitchMessageProducer)_interactionProvider).Unsubscribe(this);
                    }

                    _interactionProvider = value;

                    if (_interactionProvider is ISwitchMessageProducer) {
                        ((ISwitchMessageProducer)_interactionProvider).Subscribe(this);
                    }
                }
            }
        }

        public string Name { get; set; }

        public SwitchClient SwitchClient {
            get {
                var provider = _interactionProvider as SimulatedProvider;

                if (provider != null) {
                    return provider.Client;
                }
                return null;
            }
        }

        public void HandleMessage(Message message) {
            switch (message.Type)
            {
                case SwitchMessageType.InteractionRequest:
                    break;
                case SwitchMessageType.AgentState:
                    //handleAgentStateMessage(message as AgentStateMessage);
                    break;
                case SwitchMessageType.Connect:
                    break;
                case SwitchMessageType.Connected:
                    break;
                case SwitchMessageType.Disconnected:
                    handleDisconnectedMessage();
                    break;
                default:
                    break;
            }
        }
  
        public void SetState(AgentState state) {
            //PK: 
            //SwitchClient.Request(new AgentStateMessage {
            //    AgentId = Id,
            //    DeviceId = SwitchClient.Id,
            //    Code = state.Code,
            //    State = getSwitchAgentState(state),
            //});
        }
  
        protected virtual void OnStateChanged(AgentStateChangedEventArgs args) {
            var temp = StateChanged;
            if (temp != null) {
                temp(this, args);
            }
        }

        private AgentState getAgentState(AgentStateMessage message) {
            AgentSwitchMode switchMode = AgentSwitchMode.NewReason;

            switch (message.State)
            {
                case SwitchAgentState.Available:
                    switchMode = AgentSwitchMode.Ready;
                    break;
                case SwitchAgentState.Busy:
                    switchMode = AgentSwitchMode.NotReady;
                    break;
                case SwitchAgentState.Reserved:
                    switchMode = AgentSwitchMode.Reserved;
                    break;
                case SwitchAgentState.LoggedIn:
                    switchMode = AgentSwitchMode.LoggedIn;
                    break;
                case SwitchAgentState.WrapUp:
                    switchMode = AgentSwitchMode.WrapUp;
                    break;
                case SwitchAgentState.Connecting:
                    switchMode = AgentSwitchMode.Connecting;
                    break;
                default:
                    break;
            }

            // TODO: Better match
            var agentState = InteractionManager.AgentStates.FirstOrDefault(s => s.SwitchMode == switchMode);
            if (agentState == null)
                agentState = StandardAgentStates.Unknown;
            
            return agentState;
        }

        private SwitchAgentState getSwitchAgentState(AgentState state) {
            SwitchAgentState result = SwitchAgentState.LoggedOut;
            switch (state.SwitchMode)
            {
                case AgentSwitchMode.LoggedIn:
                    result = SwitchAgentState.LoggedIn;
                    break;
                case AgentSwitchMode.Ready:
                    result = SwitchAgentState.Available;
                    break;
                case AgentSwitchMode.NotReady:
                    result = SwitchAgentState.Busy;
                    break;
                case AgentSwitchMode.LoggedOut:
                    result = SwitchAgentState.LoggedOut;
                    break;
                case AgentSwitchMode.WrapUp:
                    result = SwitchAgentState.WrapUp;
                    break;
                case AgentSwitchMode.Reserved:
                    result = SwitchAgentState.Reserved;
                    break;
                case AgentSwitchMode.NewReason:
                    result = SwitchAgentState.Unknown;
                    break;
                case AgentSwitchMode.Connecting:
                    result = SwitchAgentState.Connecting;
                    break;
                default:
                    break;
            }

            return result;
        }

        private void handleDisconnectedMessage() {
            CurrentState = StandardAgentStates.LoggedOut;
        }
    }
}