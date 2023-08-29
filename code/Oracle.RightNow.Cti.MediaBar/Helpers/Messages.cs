using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oracle.RightNow.Cti.MediaBar.Helpers
{
    public class Messages
    {
        public static string MessageBoxTitle = "BlueLeap Media Bar";
        public static string MessageCannotSetAgentState = "Cannot set Agent's State. Please contact your administrator for help.";
        public static string MessageMediaBarDisconnected = "Media Bar will be disconnected from Finesse. Do you want to continue?";
        public static string MessageCannotSubscribeToFinesse = "Cannot subscribe to Finesse. Please verify your username and password.";
        public static string MessageCannotConnectToFinesse = "Cannot connect to Finesse with the URL {0} .Please contact your administrator.";
        public static string MessageContactReplace = "This will replace the contact {0} to {1}. Continue?";
        public static string MessageContactReplaceFailure = "Cannot associate current contact record. Please try again.";
        public static string MessageUnassociateContact = "This will remove the current contact {0} number {1}. Continue?";
        public static string MessageUnassociateContactFailure = "Cannot Un-associate the current contact record. Please try again.";
        public static string MessageWebRTCConnectFailure = "Cannot connect the number via WebRTC.\nPlease contact your administrator";
        public static string MessageWebRTCDialing = "Dialling the destination number {0} via WebRTC";
        public static string MessageFinesseLoginMessage = "Please login to Finesse Agent Desktop with the same AgentID/username before subscribing.";
        public static string MessageFinesseSignedOut = "You probably signed out from Finesse. Media Bar disconnected.";
        public static string MessageFinesseUnhandledCommunicationError = "Unhandled Exception with communicating with Finesse.\n";
        public static string MessageFinesseCommunicationError = "Cannot connect to Finesse.";
        public static string MessageSMSSendSuccessful = "SMS Send.";
        public static string MessageSMSSendFailure = "Cannot Send SMS.";
        public static string MessageSMSSendFailureInteraction = "Cannot Save to BLOutboundMessage object. SMS not sent.";
        public static string MessageCannotUpdatePhone = "Cannot save the phone number to service cloud.";
        public static string MessagePerformOutboundDial = "Cannot perform the outbound dialing operation. Please contact your administrator.";
        public static string MessageCannotFormatToE164 = "Cannot format the number {0} to E164 format required for WebRTC. The outbound call may not work. Continue with the call?";
        public static string MessageNotAValidNumber = "{0} is not a valid AU number.";
        public static string MessageNoPermissionToUseCountryCode = "Sorry you do not have permission to use the country code {0}.";
        public static string MessageNotAValidMobileNumber = "This is not a valid Mobile number {0}.";
        public static string MessageEmailNotAvailableInAccount = "No Email Address provided in Account Object. Cannot associate phone number to contact.";
        public static string MessageMoreThen1ContactAvailable = "More than 1 contact is available maching the Account Email {0}. Cannot associate Phone Number to Contact.";
        public static string MessageNoContactAvailable = "No contact is available maching the Account Email {0}. Cannot associate Phone Number to Contact.";
        public static string MessageCannotCreateCampaign = "Cannot Create SMS Campaign. Error while saving the data.";
        public static string MessageCannotFormatNumber = "Cannot convert the number {0} to the desired format.";
        public static string MessageCampaignCreatedSuccess = "SMS Campaign created successfully.";
        public static string MessageCampaignCreatedError = "Cannot create SMS Campaign.";
        public static string MessageContactSelectedOptOut = "Contact person has chosen to opt out.\nDo you want to still send an SMS?";
        public static string MessageCannotChangeOptOutFlag = "Cannot change conatact's opt out flag.";
        public static string MessageCannotSendSMSToNumber = "Cannot send a SMS to the number {0}.\nLandline number detected";
        public static string MessageCannotFetchContactListDetails = "Cannot fetch contact list details for {0}.";
        public static string MessageDisplayCampaignClickFetch = "Use Fetch Button to get the list details before clicking Save.";
        public static string MessageDisplayCampaignFetching = "Fetching details. Please wait.";
        public static string MessageEnterContactListValue = "Please enter Contact List ID/Name to fetch the details.";
        public static string MessageCampaignEditedSuccess = "SMS Campaign updated successfully.";
        public static string MessageCampaignUpdateError = "Cannot update SMS Campaign.";
        public static string CancelCampaignMessage = "Are you sure you want to cancel Campaign {0}:{1}?.";
        public static string StopCampaignMessage = "Are you sure you want to stop Campaign {0}:{1}?.";
        public static string SuccessfulUpdateCancelCampaignMessage = "Campaign Cancelled successfully.";
        public static string CannotCreateMessageTemplate = "Error while creating message template.";
        public static string CannotUpdateMessageTemplate = "Error while updating message template";
        public static string WebRTCURLNotAvailable = "WebRTC URL is not available to make an outgoing WebRTC call.";
    }

    public class WebRTCToken
    {
        public string token { get; set; }
    }

    public class BlueLeapUserAccount
    {
        public string LocalCountryCode { get; set; }
        public string InternationalPrefix { get; set; }
        public string LocalAreaCode { get; set; }
        public string MobilePrefix { get; set; }
        public string TrunkAccessCode { get; set; }
        public string ShowAlphaNumericSendingOptions { get; set; }
        public string WebRTCURL { get; set; }
        public string MasterSMSEnable { get; set; }
        public string MasterCTIEnable { get; set; }
        public string MasterCampaignEnable { get; set; }
        public string MasterWebRTCEnable { get; set; }
        public string IconURL { get; set; }
        public string ShowOptOutSendingOptions { get; set; }
        public string ShowSendingOptionsDefault { get; set; }
        public string SMSCreateIncident { get; set; }
        public string CTICreateIncident { get; set; }
    }
}
