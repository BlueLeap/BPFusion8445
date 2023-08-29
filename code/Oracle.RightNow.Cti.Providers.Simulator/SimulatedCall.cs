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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Oracle.RightNow.Cti.Model;
using Oracle.RightNow.Cti.Simulator;
using Oracle.RightNow.Cti.Simulator.Messaging.Messages;

namespace Oracle.RightNow.Cti.Providers.Simulator {
    public class SimulatedCall : SimulatedInteraction, Oracle.RightNow.Cti.Model.ICall {
        public SimulatedCall(SimulatedProvider provider) : base(provider) {
        }

        public CallType CallType { get; set; }
               
        public string Dnis { get; internal set; }

        public int Order { get; internal set; }

        public override MediaType Type {
            get {
                return MediaType.Voice;
            }
        }

        public override bool IsRealTime { get { return true; } }

        public override void Accept() {
            Provider.Client.Request(new InteractionRequestMessage(InteractionRequestAction.Accept, new Guid(Id)));
        }
  
        public void HangUp() {
            Provider.Client.Request(new InteractionRequestMessage(InteractionRequestAction.Disconnect, new Guid(Id)));
        }

        public void Hold() {
            Provider.Client.Request(new InteractionRequestMessage(InteractionRequestAction.Hold, new Guid(Id)));
        }

        public void Retrieve() {
            Provider.Client.Request(new InteractionRequestMessage(InteractionRequestAction.Retrieve, new Guid(Id)));
        }
    }
}