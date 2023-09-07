

define([],function() {
	return {
		prefix : "SVCMCA",
		attributes : {
			"communicationDirection": "SVCMCA_COMMUNICATION_DIRECTION",
			"wrapupSeconds":"SVCMCA_WRAPUP_TIMEOUT",
			"notificationSeconds":"SVCMCA_OFFER_TIMEOUT_SEC",
			"appClassification":"appClassification",
			"wrapupType":"wrapupType",
			"notificationType":"notificationType",
			"lookupObject" :"lookupObject",
			"ivrData":"ivrData",
			"autoAccept" : "AUTO_ACCEPT",
			"channelType" : "channelType",
			"comPaneleMsg1" : "comPanelMsg1",
			"comPaneleMsg2" : "comPanelMsg2",
			"comPanelQName1" : "comPanelQName1",
			"comPanelQName2" : "comPanelQName2",
			"comPanelQName3" : "comPanelQName3",
			"agentMsg2ComPanel" : "agentMsg2ComPanel",
			"averageWaitTime" : "averageWaitTime",
			"numberOfCalls" : "numberOfCalls",
			"phoneLineId":"phoneLineId",
			"eventId":"eventId",
			"engagementId" : "SVCMCA_ENGAGEMENT_ID",
			"chatTestMode" : "chatTestMode"
		},
		direction : {
			"inbound_call"  : "ORA_SVC_INBOUND",
			"outbound_call" : "ORA_SVC_OUTBOUND"
		},
		events : {
			"channelPhone" : "PHONE",
			"channelChat"  : "CHAT",
			"channelNRT"   : "NRT"
		},
		channelType : {
			"phoneChannelType"  : "ORA_SVC_PHONE",
			"socialChannelType" : "ORA_SVC_SOCIAL",
			"webChannelType"    : "ORA_SVC_WEB",
			"chatChannelType"   : "ORA_SVC_CHAT",
			"emailChannelType"  : "ORA_SVC_EMAIL",
			"noneChannelType"   : "ORA_SVC_NONE"
		},
		reason :{
			"wrapup" : "WRAPUP",
			"reject" : "REJECT",
			"missed" : "MISSED",
			"end" : "ENDCOMMUNICATION"
		},
		wrapupType : {
			"noWrapup" : "No Wrap Up",
			"unlimitedWrapup" : "Unlimited Wrap Up",
			"timedWrapup" : "Timed Wrap Up"
		},
		actions: {
			"LOGIN_ACTION" : "LOGIN",
			"REQUEST_STATUS_ACTION" : "REQUEST_STATUS",
			"LOGOUT_ACTION" : "LOGOUT",
			"RING_ACTION" : "RING",
			"ACCEPT_ACTION" : "ACCEPT",
			"REJECT_ACTION" : "REJECT",
			"END_ACTION" : "END",
			"REMOTE_END_ACTION" : "REMOTE_END",
			"ENDTRANSFER_ACTION" : "END_TRANSFER",
			"ADMIN_ACTION" : "ADMIN",
			"CONFERENCE_ACTION" : "INIT_CONFERENCE",  
			"ADD_TO_CONFERENCE_ACTION" : "ADD_TO_CONFERENCE",
			"JOIN_CONFERENCE_ACTION" : "JOIN_CONFERENCE",
			"INVITE_CONFERENCE_ACTION" : "INVITE_CONFERENCE",
			"END_CONFERENCE_ACTION" : "END_CONFERENCE",
			"CLOSE_CONFERENCE_ACTION" : "CLOSE_CONFERENCE",
			"TRANSFER_ACTION" : "INIT_TRANSFER",
			"COMPLETE_TRANSFER_ACTION" : "COMPLETE_TRANSFER",
			"RECEIVING_TRANSFER_ACTION" : "TRANSFERRED",
			"REQUEST_TRANSFER_AGENTS_ACTION" : "REQUEST_TRANSFER_AGENTS",
			"TRANSFER_TO_ACTION" : "TRANSFER_TO",
			"CONSULT_ACTION" : "INIT_CONSULT",
			"INVITE_CONSULT_ACTION" : "INVITE_CONSULT",
			"END_CONSULT_ACTION" : "END_CONSULT",
			"OUTBOUND_CALL_ACTION" : "OUTBOUND_CALL",
			"OUTBOUND_CONNECT_ACTION" : "OUTBOUND_CONNECT",
			"OUTBOUND_RING_ACTION" : "OUTBOUND_RING",
			"OUTBOUND_FAIL_ACTION" : "OUTBOUND_FAIL",
			"OUTBOUND_ACCEPT_ACTION" : "OUTBOUND_ACCEPT",
			"OUTBOUND_REJECT_ACTION" : "OUTBOUND_REJECT",
			"CONSULT_CONNECTED_ACTION" : "CONSULT_CONNECTED",
			"UPDATE_IVR_DATA_ACTION" : "UPDATE_IVR_DATA",
			"CLOSE_CONFERENCE_ACTION" : "CLOSE_CONFERENCE",
			"TEST_RING_SELF_ACTION" : "TEST_RING_SELF",
			"TEST_HANGUP_CALL_ACTION" : "TEST_HANGUP_CALL",
			"UPDATE_COM_PANEL_ACTION" : "UPDATE_COM_PANEL",
			"COM_PANEL_NEW_CALL_ACTION" : "COM_PANEL_NEW_CALL",
			"COM_PANEL_REFRESH_ACTION" : "COM_PANEL_REFRESH",			
			"NOTIFICATION_TIMER_EXP_ACTION" : "NOTIFICATION_TIMER_EXP",
			"GET_ACTIVE_ENGAGEMENTS_ACTION" : "getActiveEngagements",
			"CHAT_ACTION" : "CHAT",
			"CHAT_ACCEPT_ACTION" : "CHAT_ACCEPT",
			"CHAT_REJECT_ACTION" : "CHAT_REJECT",
			"CHAT_DISCONNECT_ACTION" : "CHAT_DISCONNECT"				
                },
		appClass : 'ORA_SERVICE',
		messageType : {
			"NOTIFICATION_READY" : "NOTIFICATION_READY",
			"END_CALL" : "END_CALL",
			"HOLD_CALL" :"HOLD_CALL"
		},
		features : {
			"INBOUND_CALL_FEATURE" : "INBOUND_CALL",
			"OUTBOUND_CALL_FEATURE" : "REQUEST_STATUS",
			"TRANSFER_CALL_FEATURE" : "TRANSFER_CALL",
			"CONFERENCE_CALL_FEATURE" : "CONFERENCE_CALL",
			"OUTBOUND_CALL_EXTENSIONS_FEATURE" : "OUTBOUND_CALL_EXTENSIONS",
			"INBOUND_CHAT_FEATURE" : "INBOUND_CHAT",
			"NRT_WORK_ASSIGN_FEATURE" : "NRT_WORK_ASSIGN"
        },
		strings : {
			"INVITE_CONSULT" : "Consulting with",
			"INVITE_CONFERENCE" : "Conference ",
			"TRANSFER_TO" : "Transferred from "
		},
		callDataAttributes : [
			"eventId",
			"callStatus",
			"callDrection",
			"appClassification",
			"wrapupType",
			"lookupObject",
			"callSource",
			"phoneLineId"
		]
	}
	
});