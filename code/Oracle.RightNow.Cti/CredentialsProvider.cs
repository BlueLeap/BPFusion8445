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
using Oracle.RightNow.Cti.Model;
using RightNow.AddIns.AddInViews;

namespace Oracle.RightNow.Cti {
    [Export(typeof(ICredentialsProvider))]
    public class CredentialsProvider : ICredentialsProvider {
        private readonly IGlobalContext _context;

        [ImportingConstructor]
        public CredentialsProvider(IGlobalContext context) {
            _context = context;
        }

        public InteractionCredentials GetCredentials() {
            // This is a simple implementation of the credentials provider.
            // Here, we're using the current user credentials but credentials could
            // come from any source, custom fields, objects or a UI prompt.
            var objectProvider = new RightNowObjectProvider(_context);
            StaffAccountInfo staffAccount = objectProvider.GetStaffAccountInformation(_context.AccountId);

            return new InteractionCredentials {
                AgentCredentials = new SwitchCredentials {
                    Id = _context.AccountId.ToString(),
                    Name = staffAccount.Name,
                    Password = staffAccount.AcdPassword
                },
                LocationInfo = new LocationInfo {
                    ComputerName = Environment.MachineName,
                    DeviceAddress = string.Format("{0}{1}{2}", _context.InterfaceId, _context.ProfileId, _context.AccountId)
                }
            };
        }
    }
}