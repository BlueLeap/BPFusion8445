using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using Oracle.RightNow.Cti.Simulator.Messaging;

namespace Oracle.RightNow.Cti.Simulator {
    public class SwitchClient : System.ServiceModel.DuplexClientBase<IRemoteMessagingProvider>, IRemoteMessagingProvider {
        private Timer _keepAliveTimer;

        public SwitchClient(InstanceContext callbackInstance) : base(callbackInstance, GetBinding(), new EndpointAddress("net.pipe://localhost/oraclerightnow/simulatedswitch")) {
            Id = Guid.NewGuid();
        }
               
        public SwitchClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : base(callbackInstance, endpointConfigurationName) {
        }

        public SwitchClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }

        public SwitchClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }

        public SwitchClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : base(callbackInstance, binding, remoteAddress) {
        }

        private static Binding GetBinding() {
            var binding = new CustomBinding(new NetNamedPipeBinding() { ReceiveTimeout = TimeSpan.FromDays(1) });
            return binding;
        }

        public void Connect(Oracle.RightNow.Cti.Providers.Simulator.Device device, bool enableGlobalSubscription = false) {
            Channel.Connect(device, enableGlobalSubscription);

            _keepAliveTimer = new Timer(ping, null,30000, 30000);
        }
   
        private void ping(object state) {
            Request(new Oracle.RightNow.Cti.Simulator.Messaging.Messages.SwitchMessage(SwitchMessageType.KeepAlive));
        }
     
        public void Disconnect(Oracle.RightNow.Cti.Providers.Simulator.Device device) {
            _keepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _keepAliveTimer.Dispose();
            _keepAliveTimer = null;

            Channel.Disconnect(device);
        }

        public void Request(Oracle.RightNow.Cti.Simulator.Messaging.Messages.Message message) {
            Channel.Request(message);
        }

        public Guid Id { get; set; }
    }
}