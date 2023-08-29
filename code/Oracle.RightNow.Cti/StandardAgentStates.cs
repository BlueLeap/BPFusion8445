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
using Oracle.RightNow.Cti.Model;

namespace Oracle.RightNow.Cti {
    public static class StandardAgentStates {
        public static AgentState Available = new AgentState(-1, "Available to receive calls", AgentSwitchMode.Ready, "-1", true, "Ready");
        public static AgentState Default = new AgentState(-2, "Default", AgentSwitchMode.NotReady, "-2", false, "Not Ready");
        public static AgentState WrapUp = new AgentState(-3, "Wrap up work mode", AgentSwitchMode.WrapUp, "-3", false, "Wrap up");
        public static AgentState LoggedOut = new AgentState(-4, "Not Connected to Finesse", AgentSwitchMode.LoggedOut, "-4", false, "Logged out");
        public static AgentState LoggedIn = new AgentState(-5, "Logged into the phone", AgentSwitchMode.LoggedIn, "-5", false, "Logged in");
        public static AgentState Reserved = new AgentState(-6, "Currently handling a call", AgentSwitchMode.Reserved, "-6", false, "Reserved");
        public static AgentState Talking = new AgentState(-8, "On a call", AgentSwitchMode.Talking, "-7", false, "Talking");
        public static AgentState Unknown = new AgentState(-9, "Unknown agent state", AgentSwitchMode.NewReason, string.Empty, false, "Unknown");
        public static AgentState NotReady = new AgentState(-10, "Not Ready to receive calls", AgentSwitchMode.NotReady, "-8", true, "Not Ready");
        public static AgentState NewReason = new AgentState(-11, "New Reason for Not Ready", AgentSwitchMode.NewReason, "-9", false, "New Reason");
        public static AgentState Connecting = new AgentState(-12, "Connecting to Finesse. Please wait.", AgentSwitchMode.Connecting, "-10", false, "Connecting");
        public static AgentState Hold = new AgentState(-13, "Call on hold", AgentSwitchMode.Hold, "-11", false, "Hold");
    }
}