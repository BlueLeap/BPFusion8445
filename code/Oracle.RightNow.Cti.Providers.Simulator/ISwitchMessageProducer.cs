using System;
using System.Linq;

namespace Oracle.RightNow.Cti.Providers.Simulator {
    internal interface ISwitchMessageProducer {
        void Subscribe(ISwitchMessageConsumer consumer);
        void Unsubscribe(ISwitchMessageConsumer consumer);
    }
}
