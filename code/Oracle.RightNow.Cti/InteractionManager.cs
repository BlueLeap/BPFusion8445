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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.RightNow.Cti.Configuration;
using Oracle.RightNow.Cti.Model;

namespace Oracle.RightNow.Cti {
    [Export]
    public class InteractionManager : IPartImportsSatisfiedNotification {
        [ImportingConstructor]
        public InteractionManager(IConfigurationProvider configurationProvider) {
            initialize(configurationProvider);
            ScreenPopHandlers = new List<IScreenPopHandler>();
        }

        public IEnumerable<AgentState> AgentStates { get; private set; }

        public IList<Contact> Contacts { get; set; }

        public SwitchCredentials Credentials { get; set; }

        [Import]
        public ICredentialsProvider CredentialsProvider { get; private set; }

        [Import]
        public IInteractionProvider InteractionProvider { get; private set; }

        [ImportMany(AllowRecomposition = true)]
        public ICollection<IScreenPopHandler> ScreenPopHandlers { get; set; }

        public void OnImportsSatisfied() {
            if (InteractionProvider != null) {
                InteractionProvider.Initialize();
            }

            InteractionProvider.NewInteraction += newInteractionHandler;
        }

        private void initialize(IConfigurationProvider configurationProvider) {
            AgentStates = configurationProvider.GetAgentStates();
        }

        public SynchronizationContext SynchronizationContext { get; set; }

        private void newInteractionHandler(object sender, InteractionEventArgs e) {
            foreach (var handler in ScreenPopHandlers) {
                if (SynchronizationContext != null)
                    SynchronizationContext.Post(o => ((InvocationTarget)o).HandleInteraction(), new InvocationTarget {
                        Interaction = e.Interaction,
                        Handler = handler
                    });
                else
                    handler.HandleInteraction(e.Interaction);
            }
        }

        private class InvocationTarget {
            public IInteraction Interaction { get; set; }

            public IScreenPopHandler Handler { get; set; }

            public void HandleInteraction() {
                Handler.HandleInteraction(Interaction);
            }
        }
    }
}