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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Oracle.RightNow.Cti.Model;
using Oracle.RightNow.Cti.Providers.Simulator.Properties;
using Oracle.RightNow.Cti.Simulator;
using Oracle.RightNow.Cti.Simulator.Messaging;
using Oracle.RightNow.Cti.Simulator.Messaging.Messages;
using RightNow.AddIns.AddInViews;

namespace Oracle.RightNow.Cti.Providers.Simulator {
    [Export(typeof(IInteractionProvider))]
    public class SimulatedProvider : ITelephonyProvider, IRemoteMessagingClient, ISwitchMessageProducer {
        private readonly object _clientRootSync = new object();
        private readonly List<ISwitchMessageConsumer> _messageConsumers = new List<ISwitchMessageConsumer>();
        
        private readonly SynchronizationContext _synchronizationContext;

        private SwitchClient _client;
        private IInteraction _currentInteraction;
        private IList<IInteraction> _interactions = new ObservableCollection<IInteraction>();
        private Device _switchDevice;

        public event EventHandler<InteractionEventArgs> CurrentInteractionChanged;
        public event EventHandler<CustomEventArgs> CustomProviderEvent;
        public event EventHandler<InteractionEventArgs> InteractionCompleted;
        public event EventHandler<InteractionEventArgs> InteractionConnected;
        public event EventHandler<InteractionEventArgs> InteractionDisconnected;
        public event EventHandler<InteractionEventArgs> NewInteraction;

        public SimulatedProvider() {
            _synchronizationContext = SynchronizationContext.Current;
        }

        [Import]
        public IAgent Agent { get; set; }

        public SwitchCredentials Credentials { get; private set; }

        [Import]
        public ICredentialsProvider CredentialsProvider { get; set; }

        public IInteraction CurrentInteraction {
            get {
                return _currentInteraction;
            }
            set {
                if (_currentInteraction != value) {
                    _currentInteraction = value;
                    OnCurrentInteractionChanged();
                }
            }
        }

        [Import]
        public IDevice Device { get; set; }

        public IList<DnsEndPoint> Endpoints { get; set; }

        public IList<IInteraction> Interactions {
            get {
                return _interactions;
            }
            private set {
                _interactions = value;
            }
        }

        public string Name {
            get {
                return Resources.ProviderName;
            }
        }

        public string Platform {
            get {
                return Resources.ProviderPlatform;
            }
        }

        public Dictionary<string, string> Properties {
            get {
                return null;
            }
        }

        [Import]
        public IGlobalContext RightNowGlobalContext { get; set; }

        public TransferTypes SupportedTransferTypes {
            get {
                return TransferTypes.Warm | TransferTypes.Cold | TransferTypes.Conference | TransferTypes.OutboundDialing;
            }
        }

        internal SwitchClient Client {
            get {
                if (_client == null) {
                    lock (_clientRootSync) {
                        if (_client == null) {
                            initializeClient();
                        }
                    }
                }

                return _client;
            }
        }

        ICall IInteractionProvider<ICall>.CurrentInteraction {
            get {
                return CurrentInteraction as ICall;
            }
        }

        IList<ICall> IInteractionProvider<ICall>.Interactions {
            get {
                return _interactions.OfType<ICall>().ToList();
            }
        }

        public void CompleteInteraction() {
            var interaction = CurrentInteraction;

            if (interaction != null) {
                Client.Request(new InteractionRequestMessage {
                    Action = InteractionRequestAction.Complete,
                    InteractionId = new Guid(interaction.Id),
                });
            }
        }

        public void HandleMessage(Message message) {
            Parallel.ForEach(_messageConsumers, c => c.HandleMessage(message));

            switch (message.Type)
            {
                case SwitchMessageType.Interaction:
                    handleInteractionMessage((InteractionMessage)message);
                    break;
                case SwitchMessageType.Disconnected:
                    _client = null;
                    break;
            }
        }

        public void Initialize() {
            if (Credentials == null && CredentialsProvider != null) {
                //PK:
                //var authenticationResult = CredentialsProvider.GetCredentials();
                Credentials = new SwitchCredentials();
                Device.Address = "";
                ((SimulatedAgent)Agent).Name = "";
            }
        }

        public void Login() {
            _switchDevice = new Device {
                Id = Client.Id,
                Address = Device.Address,
                Agent = new Agent {
                    Id = ((SimulatedAgent)Agent).Id,
                    DisplayName = Credentials.Name
                }
            };
            Client.Connect(_switchDevice);
        }

        public void Logout() {
            Client.Disconnect(_switchDevice);
        }

        public void SendNotification(CtiNotification notification) {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        protected virtual void OnCurrentInteractionChanged() {
            var temp = CurrentInteractionChanged;
            if (temp != null) {
                temp(this, new InteractionEventArgs(CurrentInteraction));
            }
        }

        protected virtual void OnInteractionCompleted(InteractionEventArgs interactionEventArgs) {
            var temp = InteractionCompleted;
            if (temp != null) {
                temp(this, interactionEventArgs);
            }
        }

        protected virtual void OnInteractionConnected(InteractionEventArgs interactionEventArgs) {
            var temp = InteractionConnected;
            if (temp != null) {
                temp(this, interactionEventArgs);
            }
        }

        protected virtual void OnInteractionDisconnected(InteractionEventArgs interactionEventArgs) {
            var temp = InteractionDisconnected;
            if (temp != null) {
                temp(this, interactionEventArgs);
            }
        }

        private void handleInteractionMessage(InteractionMessage message) {
            switch (message.Action)
            {
                case InteractionMessageAction.Created:
                    {
                        SimulatedInteraction interaction = null;
                        
                        if (message.Interaction.Type == InteractionType.Call) {
                            interaction = new SimulatedCall(this) {
                                Dnis = message.Interaction.Queue,
                                CallType = CallType.Inbound,
                                Order = Interactions.Count,
                            };
                        }
                        else if (message.Interaction.Type == InteractionType.Email) {
                            interaction = new SimulatedEmail(this);
                        }
                        else if (message.Interaction.Type == InteractionType.Web) {
                            interaction = new SimulatedWebIncident(this);
                        }

                        if (interaction != null) {
                            interaction.Id = message.Interaction.Id.ToString();
                            interaction.Address = message.Interaction.SourceAddress;
                            interaction.StartTime = DateTime.Now;
                            interaction.State = interaction.IsRealTime ? InteractionState.Ringing : InteractionState.Active;
                            interaction.Queue = message.Interaction.Queue;
                            interaction.InteractionData = message.Interaction.Properties;

                            if (message.Interaction.ReferenceId != null) {
                                interaction.AdditionalIdentifiers.Add("ReferenceId", message.Interaction.ReferenceId);
                            }

                            Interactions.Add(interaction);
                            CurrentInteraction = interaction;

                            OnNewInteraction(new InteractionEventArgs(interaction));
                        }
                    }
                    break;
                case InteractionMessageAction.Assigned:
                    {
                        IInteraction interaction = Interactions.FirstOrDefault(i => string.Compare(i.Id, message.Interaction.Id.ToString()) == 0);

                        if (interaction != null) {
                            ((SimulatedInteraction)interaction).State = InteractionState.Active;
                            OnInteractionConnected(new InteractionEventArgs(interaction));
                        }
                    }
                    break;
                case InteractionMessageAction.StateChanged:
                    {
                        IInteraction interaction = Interactions.FirstOrDefault(i => string.Compare(i.Id, message.Interaction.Id.ToString()) == 0);

                        switch (message.Interaction.State)
                        {
                            case SwitchInteractionState.Active:
                                break;
                            case SwitchInteractionState.Held:
                                break;
                            case SwitchInteractionState.Disconnected:
                                if (interaction != null) {
                                    OnInteractionDisconnected(new InteractionEventArgs(interaction));
                                }
                                break;
                            case SwitchInteractionState.Completed:
                                Interactions.Remove(interaction);
                                OnInteractionCompleted(new InteractionEventArgs(interaction));
                                CurrentInteraction = Interactions.FirstOrDefault();
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void initializeClient() {
            _client = new SwitchClient(new InstanceContext(this));
            try {
                _client.Open();
            }
            catch (EndpointNotFoundException) {
                SimulatedSwitch.Run(RightNowGlobalContext);
                initializeClient();
            }
        }

        void ISwitchMessageProducer.Subscribe(ISwitchMessageConsumer consumer) {
            _messageConsumers.Add(consumer);
        }

        void ISwitchMessageProducer.Unsubscribe(ISwitchMessageConsumer consumer) {
            _messageConsumers.Remove(consumer);
        }

        private void OnNewInteraction(InteractionEventArgs newInteractionEventArgs) {
            var temp = NewInteraction;
            if (temp != null) {
                temp(this, newInteractionEventArgs);
            }
        }
    }
}