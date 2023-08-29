using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Oracle.RightNow.Cti.Simulator.Messaging.Messages {
    [DataContract]
    public class GetDeviceInfoMessage : SwitchMessage {
        [DataMember]
        public Guid TargetDeviceId { get; set; }

        public override SwitchMessageType Type {
            get {
                return SwitchMessageType.GetDeviceInfo;
            }
            set {
                //
            }
        }
    }
}
