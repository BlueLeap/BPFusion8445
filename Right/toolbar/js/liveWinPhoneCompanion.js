
(function(){
    //============================= START constants.js
    // Constants definition
    var constants = {
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
			"comPaneleMsg1" : "comPanelMsg1",
			"comPaneleMsg2" : "comPanelMsg2",
			"comPanelQName1" : "comPanelQName1",
			"comPanelQName2" : "comPanelQName2",
			"comPanelQName3" : "comPanelQName3",
			"agentMsg2ComPanel" : "agentMsg2ComPanel",
			"averageWaitTime" : "averageWaitTime",
    		"numberOfCalls" : "numberOfCalls",
			"phoneLineId":"phoneLineId",
			"eventId":"eventId"
		},
		direction : {
			"inbound_call" : "ORA_SVC_INBOUND",
			"outbound_call" : "ORA_SVC_OUTBOUND"
		},
		events : {
			"channelPhone" : "PHONE",
			"channelChat" : "CHAT"
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
			"HOLD_CALL" :"HOLD_CALL",
			"UPDATE_COM_PANEL_MSG" : "UPDATE_COM_PANEL"
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
			"callSource"
		]
	};
    //============================= END constants.js
    //============================= START liveConn.js
    function liveConnFactory(atmosphere) {

        function connSocket() {
            var self = this;
            self.conn = null;
            self.socket = atmosphere;
            self.url = retrieveUrl();
            self.openCallback = null;
            self.msgCallback = null;
            self.onReconnectListener = null;
            self.onReopenListener = null;
            self.onCloseListener = null;
            self.onErrorListener = null;
            self.onTransportFailureListener = null;
            self.request = { 
                url: self.url + 'reftlb',
                contentType: "application/json",
                logLevel: 'debug',
                transport: 'websocket',
                reconnectInterval: 5000,
                fallbackTransport: 'long-polling',
                enableProtocol: true,
                trackMessageLength: true
            };

            function retrieveUrl(){
                var url = document.location.toString();
                var index = url.lastIndexOf("/")+1;
                url = url.substring(0,index);
                return url;
            }

            self.request.onOpen = function (response) {
                console.log('Atmosphere connected using '+response.transport);
                if (self.openCallback) {
                    self.openCallback();
                }
            };

            self.request.onReconnect = function (request, response) {
                console.log('Connection lost, trying to reconnect. Trying to reconnect '+request.reconnectInterval);
                if (self.onReconnectListener) {
                    self.onReconnectListener(request, response);
                }
            };

            self.request.onReopen = function (response) {
                console.log('Atmosphere re-connected using '+response.transport);
                if (self.onReopenListener) {
                    self.onReopenListener(response);
                }
            };

            self.request.onMessage = function (response) {
                var json;
                var message = response.responseBody;
                console.log("Atmosphere (toolbar) onMessage: "+message);
                try {
                    json = JSON.parse(message);
                } catch (e) {
                    console.log('This doesn\'t look like a valid JSON: ', message);
                    return;
                }
                if (self.msgCallback) {
                    self.msgCallback(json);
                }        
            };

            self.request.onClose = function (response) {
                console.log('Atmosphere (toolbar) received an message on close: '+response);
                if (self.onCloseListener) {
                    self.onCloseListener(response);
                }            
            };

            self.request.onError = function (response) {
                console.log('Atmosphere (toolbar) received an error : '+response);
                if (self.onErrorListener) {
                    self.onErrorListener(response);
                }            
            };

            self.request.onTransportFailure = function (errorMsg, request) {
                console.log('Atmosphere (toolbar) received an error on transport failure: '+errorMsg);
                if (self.onTransportFailureListener) {
                    self.onTransportFailureListener(errorMsg, request);
                }            
            };            
            return {
                getUrl: function() {
                    return self.url;
                },
                connect: function(userName, openCallback, messageCallback) {
                    self.openCallback = openCallback;
                    self.msgCallback = messageCallback;
                    self.request.url = self.url + 'reftlb/'+userName;
                    self.conn = self.socket.subscribe(self.request);
                },
                publish: function(message) {

                    if (self.conn) {
                        self.conn.push(message);
                    }
                },
                disconnect : function(){
                    self.socket.unsubscribe();
                }
            };
        }

        return new connSocket();
    };
    //============================= END liveConn.js
    //============================= START vToolbar.js
    function companionPanelFactory( liveConn, constants, svcMcaTlb) {

        var author, json, eventId;
        var self = this;
        self.slot = null;
        self.agentLogin = false;
        self.firstCallUpdate = true;
        
        function getParameterByName(name) {
            var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
            return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
        }

        function toolbarMessageCallback(response) {
            console.log('>>-- [Companion Panel] Inter toolbar communication message received: '+response.messagePayload);
            var msgPayload = JSON.parse(response.messagePayload);
            if (msgPayload.msgCommand && msgPayload.msgCommand === 'AGENT_AVAIL') {
                configureAgentInfo();
            } else if (msgPayload.msgCommand && msgPayload.msgCommand === constants.messageType.UPDATE_COM_PANEL_MSG) {            	
            	updateCompanionPanel(msgPayload);
            }
        }
        
/*        function openVToolbar() {
            console.log('>>-- [Companion Panel] Open Companion Panel');
            initTabs();
            dialpad.initDialpad();
            $('#VToolbar').show();
        }

        function closeVToolbar() {
            console.log('>>-- [Companion Panel] Close Companion Panel');
            clearAgents();
            $('#VToolbar').hide();
        }
*/        
        function newCall() {
        	
        	// Request to pop up outbound dialpad
        	liveConn.publish(JSON.stringify({ author: author, callData: {}, action: constants.actions.COM_PANEL_NEW_CALL_ACTION}));

            console.log("Companion Panel Request New Call message sent!");


        };

        function handleMessage(json) {
            switch(json.action){
                case constants.actions.LOGIN_ACTION:
                    if(json.author){
                        console.log("Companion Panel Logged in...");
                        author = json.author;
                        if (author != "") {
                        	console.log("Logged in Companion Panel User is set to " + author);
                        	self.agentLogin = true;
                        }
 
                    }
                    break;
                case constants.actions.UPDATE_COM_PANEL_ACTION:
                    updateCompanionPanel(json);
                    break;
                  default:
                    break;
            }
        }
        
        function updateCompanionPanel(json) {
        	console.log("Update Companion Panel with message: " + json.toString());
        	
        	var callData = json.callData;
        	var comMsg1 = callData.comPanelMsg1;
        	var comMsg2 = callData.comPanelMsg2;
        	var comQname1 = callData.comPanelQName1;
        	var comQname2 = callData.comPanelQName2;
        	var comQname3 = callData.comPanelQName3;
         	var agentMsg2Cp = callData.agentMsg2ComPanel;
        	var avgWaitTime = callData.averageWaitTime;
        	var numberOfCalls = callData.numberOfCalls;
        	
            var msgArea1 = $('#topEventText');
            var msgArea2 = $('#bottomEventText');
            var QName1 = $('#queueName1');
            var QName2 = $('#queueName2');
            var QName3 = $('#queueName3');
            var avgWTArea = $('#waitTimeNumber');
            var avgWTItem = $('#waitTimeItem');
            var numOfCallsArea = $('#callStatsNumber');

            var today = new Date();
            var month = today.getMonth() + 1;
            var day = today.getDate();
            var todayString = month + "/" + day + ": ";
            
            if (comMsg1) {
            	msgArea1.html(comMsg1);
            	//$('#topEventText').html(comMsg1);
            	$('#topDate').html(todayString);
            }
      	
            if (comMsg2) {
            	msgArea2.html(comMsg2);
            	//$('#bottomEventText').html(comMsg2);
            	$('#bottomDate').html(todayString);
            }

            if (comQname1) {
            	QName1.html(comQname1);
            }

            if (comQname2) {
            	QName2.html(comQname2);
            }
            
            if (comQname3) {
            	QName3.html(comQname3);
            }

            if (agentMsg2Cp) {
                alert("Companion Panel receive message from agent: "+agentMsg2Cp); 
            }
        	
 /*           if (avgWaitTime && (!isNaN(avgWaitTime))) {
            	
            	if (avgWTArea.hasClass('currentStatsNormal')) {
            		avgWTArea.removeClass('currentStatsNormal');
            	}
            	
            	if (avgWTArea.hasClass('currentStatsWarning')) {
            		avgWTArea.removeClass('currentStatsWarning');
            	}

            	if (avgWTArea.hasClass('currentStatsAlarm')) {
            		avgWTArea.removeClass('currentStatsAlarm');
            	}

            	if (avgWaitTime >= 100) {
            		avgWTArea.addClass('currentStatsAlarm');
            	} else if (avgWaitTime >= 60) {
            		avgWTArea.addClass('currentStatsWarning');
            	} else {
            		avgWTArea.addClass('currentStatsNomal');
            	}
            			
            	var minutes = 0;
            	var seconds = 0;
            	
            	if (avgWaitTime >= 60) {
            		minutes = Math.floor(avgWaitTime / 60);
            		
            		seconds = avgWaitTime - minutes * 60;
            	} else {
            		seconds = avgWaitTime;
            	}
            	
            	var showTime = null;
            	
            	if (seconds >= 10) {
            		showTime = minutes + ":" + seconds;
            	} else {
            		showTime = minutes + ":0" + seconds;
            	}

            	avgWTArea.html(showTime);
            	//$('#waitTimeNumber').html(avgWaitTime);
            }
*/
            if (numberOfCalls && (!isNaN(numberOfCalls))) {
            	numOfCallsArea.html(numberOfCalls);
            	
            	if (self.firstCallUpdate) {
            		updateAverageWaitTime();
            		setInterval(updateAverageWaitTime, 10000);
            		self.firstCallUpdate = false;
            	}
            	
            	//$('callStatsNumber').html(avgWaitTime);
            }

        }

        function updateAverageWaitTime() {
        	
        	// Start Update Average Wait time when agent logged in
        	//if (!self.agentLogin) {
        	//	return;
        	//}
        	
        	/* generate a random number between 10 and 200 */
        	var avgWaitTime = Math.floor(Math.random() * 190) + 10;
        	
            var avgWTArea = $('#waitTimeNumber');
            var avgWTItem = $('#waitTimeItem');
            
        	var avgWTItem = $('#waitTimeItem');

            if (avgWaitTime && (!isNaN(avgWaitTime))) {
            	
            	if (avgWTArea.hasClass('currentStatsNormal')) {
            		avgWTArea.removeClass('currentStatsNormal');
            	}
            	
            	if (avgWTArea.hasClass('currentStatsWarning')) {
            		avgWTArea.removeClass('currentStatsWarning');
            	}

            	if (avgWTArea.hasClass('currentStatsAlarm')) {
            		avgWTArea.removeClass('currentStatsAlarm');
            	}

            	if (avgWaitTime >= 100) {
            		avgWTArea.addClass('currentStatsAlarm');
            	} else if (avgWaitTime >= 60) {
            		avgWTArea.addClass('currentStatsWarning');
            	} else {
            		avgWTArea.addClass('currentStatsNomal');
            	}
            			
            	var minutes = 0;
            	var seconds = 0;
            	
            	if (avgWaitTime >= 60) {
            		minutes = Math.floor(avgWaitTime / 60);
            		
            		seconds = avgWaitTime - minutes * 60;
            	} else {
            		seconds = avgWaitTime;
            	}
            	
            	var showTime = null;
            	
            	if (seconds >= 10) {
            		showTime = minutes + ":" + seconds;
            	} else {
            		showTime = minutes + ":0" + seconds;
            	}

            	avgWTArea.html(showTime);
            	//$('#waitTimeNumber').html(avgWaitTime);
            }

        }

        function atmoSubscribeCallback(uniqueUserName) {
            liveConn.publish(JSON.stringify({ author: uniqueUserName, message: uniqueUserName, data: "{'source': 'companionToolbar'}", action: constants.actions.LOGIN_ACTION }));
        }

        function configureAgentInfo() {
            svcMcaTlb.api.getConfiguration("TOOLBAR", function(response) { 
                if (response.configuration) {
                    var agentId = response.configuration.agentId;
                    if (agentId) {
                        retrieveUniqueAgentId(agentId);
                    }
                 }
            });
        };

        function retrieveUniqueAgentId(username) {

        };


        function checkAgentLoggedin() {
        	
        	console.log("Companion Panel checkAgentLoggedin function invoked");

            var uuidReqData = {
                type: 'GET',
                dataType: 'json',
                async: true,
                url : liveConn.getUrl()+'services/agent/check',
                data: {},
                error: function( jqXHR, textStatus, errorThrown ) {
                    console.log("Companion Panel Check agent loggedin failed with status: "+jqXHR.status+" - message: "+textStatus);
                },
                success : function ( data, textStatus, jqXHR ) {
                	console.log("Companion Panel Retrieved data: "+data);
                    console.log("Companion Panel Retrieved: "+data.agentId);
                    if (data.agentId && data.agentId !== "") {

                        var unqUsername = data.agentId;
                        liveConn.connect(unqUsername, function(){
                                atmoSubscribeCallback(unqUsername);
                            }, handleMessage);
                    }
                }
            };
            $.ajax(uuidReqData);
        	console.log("Companion Panel checkAgentLoggedin function completed");

        };

        function removeLoggedinAgent() {
            var uuidReqData = {
                type: 'GET',
                dataType: 'json',
                async: true,
                url : liveConn.getUrl()+'services/agent/remove',
                data: {},
                error: function( jqXHR, textStatus, errorThrown ) {
                    console.log("Companion Panel remove agent loggedin failed with status: "+jqXHR.status+" - message: "+textStatus);
                },
                success : function ( data, textStatus, jqXHR ) {
                    console.log("Companion Panel Retrieved: "+data.agentId);
                 }
            };
            $.ajax(uuidReqData);
        };


        return{
            init: function() {
            	console.log("Companion Panel init function invoked");
                svcMcaTlb.api.onToolbarMessage(toolbarMessageCallback);

                $('#newCallLabelItem').click(function () {
                	newCall();
                });

                
/*                svcMcaTlb.api.getConfiguration("TOOLBAR", function(response) { 
                    if (response.configuration) {
                        var agentId = response.configuration.agentId;
                        
                        console.log("Companion Panel configureAgentInfo returned with agentId " + agentId);

                        if (agentId) {
                        	checkAgentLoggedin(agentId, loggedInCallback); 
                        }
                     }
                });
*/
                // HZH Set Init User status
                //$('.loggedInUser').html('(Not Signed in)');
                // HZH 101717 Temporary comment out
                //checkAgentLoggedin();
                //setInterval(updateAverageWaitTime, 10000);
            	console.log("Companion Panel init function completed");
            },
            
            updateAverageWaitTime : function() {
            	updateAverageWaitTime();
            },
            
            handleMessage: function (json) {
            	handleMessage(json);
            },
            
        }
    }    
    //============================= END vToolbar.js

    //============================= BEGIN agentInfo.js
    function agentInfoFactory(liveConn, constants, svcMcaTlb) { 
        var author, json;
 
        function agentInfo() {
            var self = this;
            self.jwt = null;
            self.loggedIn = false;
            self.subSocket = liveConn;
            //var dialpad = dialpadFactory();
            //self.dialpad = dialpad;
            self.companionPanel = companionPanelFactory(liveConn, constants, svcMcaTlb);
            //self.call = new callInfo();
            //self.call.setAtmosphereSocket(self.subSocket);
            //self.consultSlot = new consultSlot();
            //self.consultSlot.setAtmosphereSocket(self.subSocket);
            self.agentStatus = 'OFF';
 
            //self.Notification = window.Notification || window.mozNotification || window.webkitNotification;
            //if (self.Notification){
            //    self.Notification.requestPermission(function (permission){ 
            //        console.log(permission); 
            //    });
            //}

            function getAgentStatus() {
            	return self.agentStatus;
            	//return 'OFF';
            }
            
            function setAgentStatus(status) {
            	self.agentStatus = status;
            }
            
            function agentStateEventChangeCallBack(response) {
            	console.log("<RefToolbar> response for agentStateEvent received!!! Reponse " + JSON.stringify(response));
            }
            
            function logoutAgent() {

                self.subSocket.publish(JSON.stringify({ author: author, message: '', action: constants.actions.LOGOUT_ACTION }));
                self.agentStatus = 'OFF';
            }
            
            function prepareLogging(){
                //make rest call to get the configuration for logging
                var debugMode=true;
                if(!debugMode){
                    if(window.console){
                        window.console = {};
                        window.console.log = function(){};
                    }
                }
            }

            function userLoggedInInfo() {
                $('.userSlot').removeClass('agentOff').addClass('agentOn');
                $('.agentIcon').removeClass('agentIconOff').addClass('agentIconOn');
                $('#availableBtn').removeClass('unavailBtn').addClass('availBtn');
                $('#availableBtn').attr('title', 'Available');
                setAgentStatus('LOGGED_IN');
                //$('#availableBtn').off("click");
            };

            function agentLogin(){
                //$('#outbound').hide()
                window.onbeforeunload = (function(){
                    logoutAgent();
                });
                configureAgentInfo();

                svcMcaTlb.api.postToolbarMessage(JSON.stringify( {"msgCommand":"AGENT_AVAIL"} ), function(response) {
                    console.log("======== Response for POST from agentLogin - status: "+response.result);
                });

            };
/*
            function isCustomerDataPresent(data){
                if(data){
                    if(data.SVCMCA_SR_NUM || data.SVCMCA_SR_TITLE || data.SVCMCA_CONTACT_NAME || data.SVCMCA_ORG_NAME) {
                        return true;
                    }
                }
                return false;
            }
*/
            function setAgentOnBreak(){
                $('.userSlot').removeClass('agentOn').addClass('agentBreak');
                $('.agentIcon').removeClass('agentIconOn').addClass('agentIconTempBreak');
                $('#availableBtn').removeClass('availBtn').addClass('onBreakBtn');
                $('#availableBtn').attr('title', 'OnBreak');
                svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",false,true,"UNAVAILABLE","On Break. Lunch Break",null,"On Break",{}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);
                self.agentStatus = 'ON_BREAK';
            }

            function setAgentOffBreak(){
                $('.userSlot').removeClass('agentBreak').addClass('agentOn');
                $('.agentIcon').removeClass('agentIconTempBreak').addClass('agentIconOn');
                $('#availableBtn').removeClass('onBreakBtn').addClass('availBtn');
                $('#availableBtn').attr('title', 'Available');
                svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",true,true,"AVAILABLE","Idle",null,"Idle",{}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);
                self.agentStatus = 'LOGGED_IN';
            }

            self.toolbarMessageCallback = function(response) {
                console.log('>>--  Inter toolbar communication message received: '+response.messagePayload);
                // TODO
                try{
                    var sentData = JSON.parse(response.messagePayload);

                    if(sentData.messageType == constants.messageType.NOTIFICATION_READY){

                        var inboundData = self.call.getInboundData();
                        svcMcaTlb.api.postToolbarMessage(JSON.stringify(inboundData), function(response) {
                            console.log("======== ########## Message POST status: "+response.result);
                        });
                    }else{
                        var msgPayload = JSON.parse(response.messagePayload);
                        $('#interMsgRec').append('<p>Received: '+msgPayload.message+'</p>');
                    }
                }catch(err){
                    console.log(err);
                }
            };

            function handleMessage(json) {

                console.log("CP AgentInfo HandleMessage(), action type " + json.action);

                switch(json.action){
                    case constants.actions.LOGOUT_ACTION:
                        //window.location.reload();  
                    	self.agentStatus = 'OFF';
                        svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",false,false,"DISCONNECTED",null,null,"Logged Out",{}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);
                    	break;	
                    case constants.actions.LOGIN_ACTION:
                        if(json.author){
                            console.log("Login response action received! json value is " + json);
                            if (!self.loggedIn) {
                                console.log("Already logged in!");
                                self.loggedIn = true;
                                self.agentStatus = 'LOGGED_IN';
                                author = json.author;
                                console.log("User Author 1 value is " + author);
                                $('#input').val('');
                                if (json.author !== null && json.author != "") {
                                	console.log("Logged in User (1) is set to " + json.author);
                                	$('.loggedInUser').html(''+json.author);
                                    $('#outboundVT').show();
                                
                                    self.subSocket.publish(JSON.stringify({ author: author, message: author, action: constants.actions.GET_STATUS_ACTION }));
                                }
                            }

                            svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",true,true,"AVAILABLE","Idle",null,"Idle",{}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);

                        }
                        break;
                    default:
                        break;
                }

                // Handle vtoolbarActions:
                self.companionPanel.handleMessage(json);

            }
/*
            function outboundCallCallback(response) {
                self.call.setInboundData(response.outData);                        
                self.call.setEventId(response.uuid);
                self.call.createAndShowCallSlot(constants.direction.outbound_call);
                self.subSocket.publish(JSON.stringify({ author: author, 
                                                        message: '{"eventId":"'+response.uuid+'"}', 
                                                        action: constants.actions.OUTBOUND_CALL_ACTION,
                                                        data: JSON.stringify(response.outData)}));
            }

            function setupAboutBox(buildVersion, adminUrl) {
                $('#buildVersion').text(buildVersion);
                $('#adminUrl').text(adminUrl);
                $('#aboutBtn').click(function() {
                    $('#aboutBox').fadeIn();
                }); 
                $('#closeAboutBox').click(function() {
                    $('#aboutBox').fadeOut();
                });

            }
            function interactionCommandExecutor(command) {
                console.log("<RefToolbar> interactionCommandExecutor Invoked for command: "+command.command);
                console.log(command);
                //sjmD
                var cmd = command.command.toUpperCase();
                switch(cmd) {
                case constants.actions.ACCEPT_ACTION:
                       self.call.acceptAcknowledged();
                       break;
                case constants.actions.REJECT_ACTION:
                       self.call.rejectAckknowledged();
                       break;
                }
                command.result = 'success';
                command.sendResponse(command);    
              }
              
              function agentCommandExecutor(command) {
                console.log("<RefToolbar> agentCommandExecutor Invoked for command: "+command.command);
                console.log(command);
                var cmd = command.command;
                switch(cmd) {
                case "getCurrentAgentState":  // TODO return actual state
                       var outData = {'channel':command.channel,
                                                  'channelType':command.channelType,
                                                  'isAvailable':true,
                                                  'isLoggedIn':true,
                                                  'state':"AVAILABLE",
                                                  'stateDisplayString':"Available On RefToolbar",
                                                  'reason':null,
                                                  'reasonDisplayString':null};
                       command.outData = outData;
                       break;
                case "getActiveInteractionCount":
                       var outData = {'activeCount':1}; // TODO return actual count
                       break;
                case "makeAvailable":
                       alert("makeAvailable command invoked");
                       break;
                case "makeUnavailable":
                       alert("makeUnavailable command invoked");
                }
                command.result = 'success';
                command.sendResponse(command);
            }
*/
            function configureAgentInfo() {
                retrieveAboutBoxInfo();

                svcMcaTlb.api.onOutgoingEvent(constants.events.channelPhone, constants.appClass, outboundCallCallback, constants.channelType.phoneChannelType);
                svcMcaTlb.api.onToolbarInteractionCommand(interactionCommandExecutor);
                console.log("INVOKING new agentCommand Listener!!!");
                svcMcaTlb.api.onToolbarAgentCommand(constants.events.channelPhone, constants.channelType.phoneChannelType, agentCommandExecutor);

                svcMcaTlb.api.getConfiguration("TOOLBAR", function(response) { 
                    if (response.configuration) {
                        var agentId = response.configuration.agentId;
                        
                        console.log("configureAgentInfo returned with agentId " + agentId);

                        if (agentId) {
                            retrieveUniqueAgentId(agentId);
                        }
                     }
                });
            };

            function retrieveUniqueAgentId(agentId) {
                prepareLogging();


                var uuidReqData = {
                    type: 'GET',
                    dataType: 'json',
                    async: true,
                    url : self.subSocket.getUrl()+'services/agent/id?userAgent='+agentId,
                    data: {},
                    error: function( jqXHR, textStatus, errorThrown ) {
                        console.log("Get Agent ID failed with status: "+jqXHR.status+" - message: "+textStatus);
                        console.log("Fallback too locally generated ");

                        var unqUsername = agentId+"_"+(new Date()).getTime();
                        self.subSocket.connect(unqUsername, function(){
                            atmoSubscribeCallback(unqUsername);
                        }, handleMessage);
                    },
                    success : function ( data, textStatus, jqXHR ) {
                        console.log("agentInfo Retrieved: "+data.agentId);
                        var unqUsername = data.agentId;
                        self.subSocket.connect(unqUsername, function(){
                            atmoSubscribeCallback(unqUsername);
                            self.call.setAuthor(unqUsername);
                        }, handleMessage);
                    }
                };
                $.ajax(uuidReqData);
            };

            function checkAgentLoggedin(inAgentId, loggedInCallback) {
                prepareLogging();

                var uuidReqData = {
                    type: 'GET',
                    dataType: 'json',
                    async: true,
                    url : self.subSocket.getUrl()+'services/agent/check',
                    data: {},
                    error: function( jqXHR, textStatus, errorThrown ) {
                        console.log("Check agent loggedin failed with status: "+jqXHR.status+" - message: "+textStatus);
                    },
                    success : function ( data, textStatus, jqXHR ) {
                        console.log("CheckAgentLoggedin Retrieved: agentId "+ data.agentId + " inAgentId = " + inAgentId);
                        if (data.agentId && data.agentId !== "") {

                        	//if (data.agentId.includes(inAgentId)) {
                        	if (data.agentId.indexOf(inAgentId) >= 0) {
                        		
                        		console.log("CheckAgentLoggedin Retrieved: "+data.agentId + " matches username");
                        		
	                            retrieveAboutBoxInfo();
	                            //svcMcaTlb.api.onOutgoingEvent(constants.events.channelPhone, constants.appClass, outboundCallCallback, constants.channelType.phoneChannelType);
	                            //svcMcaTlb.api.onToolbarInteractionCommand(interactionCommandExecutor);
	                            //svcMcaTlb.api.onToolbarAgentCommand(constants.events.channelPhone, constants.channelType.phoneChannelType, agentCommandExecutor);
	                            
	                            var unqUsername = data.agentId;
	                            self.subSocket.connect(unqUsername, function(){
	                                    atmoSubscribeCallback(unqUsername);
	                                    //self.call.setAuthor(unqUsername);
	                                }, handleMessage);
	                                
	                            svcMcaTlb.api.postToolbarMessage(JSON.stringify( {"msgCommand":"AGENT_AVAIL"} ), function(response) {
	                                console.log("======== Response for POST from agentLogin - status: "+response.result);
	                            });
	                            
	                            // HZH Init virtical Toolbar
	                            self.companionPanel.init();
	                            
	                            self.subSocket.publish(JSON.stringify({ author: author, callData: {}, action: constants.actions.COM_PANEL_REFRESH_ACTION}));
	                            console.log("Companion Panel Request Refresh message sent!");
	                            
	                            if (loggedInCallback) {
	                                loggedInCallback();
	                            }
                        	} else {

                        		console.log("CheckAgentLoggedin Retrieved: "+data.agentId + " does not match username, to be removed");

                        		removeLoggedinAgent();
                        	}
                        }
                    }
                };
                $.ajax(uuidReqData);
            };

            function removeLoggedinAgent() {
                var uuidReqData = {
                    type: 'GET',
                    dataType: 'json',
                    async: true,
                    url : liveConn.getUrl()+'services/agent/remove',
                    data: {},
                    error: function( jqXHR, textStatus, errorThrown ) {
                        console.log("Companion Panel remove agent loggedin failed with status: "+jqXHR.status+" - message: "+textStatus);
                    },
                    success : function ( data, textStatus, jqXHR ) {
                        console.log("Companion Panel Retrieved: "+data.agentId);
                        //if (data.agentId && data.agentId !== "") {
                        //	console.log("vToolbar Connect for: " + data.agentId);
                        //    var unqUsername = data.agentId;
                        //    liveConn.connect(unqUsername, function(){
                        //            atmoSubscribeCallback(unqUsername);
                        //        }, handleMessage);
                            //initTabs();
                            //dialpad.initDialpad();
                        //}
                    }
                };
                $.ajax(uuidReqData);
            };

            function atmoSubscribeCallback(uniqueUserName) {
                self.subSocket.publish(JSON.stringify({ author: uniqueUserName, message: uniqueUserName, action: constants.actions.LOGIN_ACTION }));
                //getLastState(uniqueUserName);
            };

/*            function getLastState(uniqueUserName){
                var uuidReqData = {
                    type: 'GET',
                    dataType: 'json',
                    async: true,
                    url : self.subSocket.getUrl()+'services/calls/restore?agentId='+uniqueUserName,
                    data: {},
                    error: function( jqXHR, textStatus, errorThrown ) {
                        console.log("Restore Error: "+errorThrown);
                    },
                    success : function ( data, textStatus, jqXHR ) {
                        console.log("Restore Retrieved: "+JSON.stringify(data));
                        
                        if (data['agentId'] != null) { 
                        	data['author']=data['agentId'];

                        	self.call.setRestoreState(true);
                        	restoreCallSlot(data);
                        	self.call.setRestoreState(false);
                        }

                    }
                };
                $.ajax(uuidReqData);
            }
            function restoreCallSlot(json){
                var action = json.action;

                switch(action){
                    case constants.actions.RING_ACTION:
                        switchHandleMessageAction(json, [constants.actions.RING_ACTION]);

                        break;

                    case constants.actions.ACCEPT_ACTION:
                        switchHandleMessageAction(json, [constants.actions.RING_ACTION, 
                                                   constants.actions.ACCEPT_ACTION]);
                        break;
                    case constants.actions.TRANSFER_ACTION:
                        switchHandleMessageAction(json, [constants.actions.RING_ACTION, 
                                                   constants.actions.ACCEPT_ACTION]);
                        self.call.transferCall();
                        break;
                    case constants.actions.INVITE_CONSULT_ACTION:
                        switchHandleMessageAction(json, [constants.actions.RING_ACTION, 
                                                   constants.actions.ACCEPT_ACTION, 
                                                   constants.actions.INVITE_CONSULT_ACTION]);

                        break;
                    case constants.actions.CONSULT_CONNECTED_ACTION:
                        switchHandleMessageAction(json, [constants.actions.RING_ACTION, 
                                                   constants.actions.ACCEPT_ACTION, 
                                                   constants.actions.INVITE_CONSULT_ACTION, 
                                                   constants.actions.CONSULT_CONNECTED_ACTION]);

                        break;
                    case constants.actions.INVITE_CONFERENCE_ACTION:
                        switchHandleMessageAction(json, [constants.actions.RING_ACTION, 
                                                           constants.actions.ACCEPT_ACTION,
                                                           constants.actions.INVITE_CONSULT_ACTION,
                                                           constants.actions.INVITE_CONFERENCE_ACTION]);
                        break;
                }
            }

            function switchHandleMessageAction(json, actions){

                for(var i=0; i< actions.length; i++){
                    var initialAction = json.action;
                    json.action = actions[i];
                    handleMessage(json);
                    json.action = initialAction;
                }
            }
*/
            // HHUANG 20161110
            // Send Test Message
            function testRing(agentId, callData) {

                console.log("Start Testing Ring to "+agentId+" - callData: "+callData);


                self.subSocket.publish(JSON.stringify({ author: author, callData: callData, action: constants.actions.TEST_RING_SELF_ACTION}));

                //getLastState(author);

                console.log("Test Ring action message sent!");


            };

 /*           function newCall() {
            	
            	if (getAgentStatus() != 'LOGGED_IN') {

            		console.log("Agent is not Available to make new call. ");
            		
            		return;
            	}

            	// Request to pop up outbound dialpad
                self.subSocket.publish(JSON.stringify({ author: author, callData: {}, action: constants.actions.COM_PANEL_NEW_CALL_ACTION}));

                //getLastState(author);

                console.log("Companion Panel Request New Call message sent!");


            };
*/
            function companionPanelInit() {

                console.log("Start Companion Panel init");

                self.companionPanel.init();
                //window.searchAgent = self.vToolbar.searchAgent;

                console.log("finish Companion Panel init");


            };

            function retrieveAboutBoxInfo() {
                var uuidReqData = {
                    type: 'GET',
                    dataType: 'json',
                    async: true,
                    url : self.subSocket.getUrl()+'services/about/version',
                    data: {},
                    error: function( jqXHR, textStatus, errorThrown ) {
                        console.log("Get buildVersion failed with status: "+jqXHR.status+" - message: "+textStatus);
                        //setupAboutBox('ERROR_READING.UNKNOWN_BUILD', self.subSocket.getUrl()+'admin.jsp' );
                    },
                    success : function ( data, textStatus, jqXHR ) {
                        console.log("Retrieved: "+data.buildString);
                        //setupAboutBox(data.buildString, self.subSocket.getUrl()+'admin.jsp' );
                    }
                };
                $.ajax(uuidReqData);
            };

            return {

                //callInfo : self.call,
                init: function(loggedInCallback) {

                	console.log("Companion Panel Init invoked.");

                	svcMcaTlb.api.onToolbarMessage(self.toolbarMessageCallback);
                    
                    svcMcaTlb.api.getConfiguration("TOOLBAR", function(response) { 

                    	console.log("Companion Panel getConfig response " + response);                        	

                    	if (response.configuration) {
                        	
                            var agentId = response.configuration.agentId;
                            
                            console.log("Companion Panel configureAgentInfo returned with agentId " + agentId);

                            if (agentId) {
                            	checkAgentLoggedin(agentId, loggedInCallback); 
                            }
                         }
                    });

                    //checkAgentLoggedin(loggedInCallback);
                    companionPanelInit();

                },
                agentLogin: function(){
                    //$('#outbound').hide()
                    window.onbeforeunload = (function(){
                        logoutAgent();
                    });
                    configureAgentInfo();

                    svcMcaTlb.api.postToolbarMessage(JSON.stringify( {"msgCommand":"AGENT_AVAIL"} ), function(response) {
                        console.log("======== Response for POST from agentLogin - status: "+response.result);
                    });

                },
                
                getAgentStatus : function() {
                	return getAgentStatus();
                },
                setAgentStatus : function(status) {
                	return setAgentStatus(status);
                },
                
/*                setAgentOnBreak : function() {
                	setAgentOnBreak();
                },
                setAgentOffBreak : function() {
                	setAgentOffBreak();
                },
*/                outboundCallCallback: function(response) {
                    outboundCallCallback(response);
                },

                testRing: function(agentId, callData) {

                    testRing(agentId,callData);

                },

                newCall : function() {
                	newCall();
                },
                
/*                handleAgentStatusChange: function() {
                	handleAgentStatusChange();
                },
*/               
                handleMessage: function(json){
                    handleMessage(json);
                },
/*
                sendIntercom: function(payload) {
    // TODO                        
    //	                    svcMcaTlb.api.postToolbarMessage(JSON.stringify(m), function(response) {
    //	                            console.log("======== ########## Message POST status: "+response.result);
    //	                    } );                		
                },
                queryCustomerData : function(){

                    var inboundData = {"SVCMCA_COMMUNICATION_DIRECTION":"ORA_SVC_INBOUND","appClassification" : "ORA_SERVICE", "wrapupSeconds" : "", "wrapupType" : "Unlimited Wrap Up"};
                    var token = $("#token").val();
                    var value = $("#tokenValue").val();
                    inboundData[token]=value;

                    svcMcaTlb.api.newCommEvent(constants.events.channelPhone, constants.appClass, "", self.inboundData, null, function(response){}, constants.channelType.phoneChannelType);
                },
                showFloatingBar: function() {
                        svcMcaTlb.api.openFloatingToolbar("FLOAT_VERTICAL", null, null, null, {"showDialpad":"true"}, function(response){
                                console.log(response);
                        }, constants.channelType.phoneChannelType);                	
                },

                showFloatingBarTest: function() {

                    var url = $("#vUrl").val();
                    var width = $("#vWidth").val();
                    var heigth = $("#vHeight").val();

                    svcMcaTlb.api.openFloatingToolbar("FLOAT_VERTICAL",url, heigth, width, {"showDialpad":"true"}, function(response){
                            console.log(response);
                    }, constants.channelType.phoneChannelType);                	
                },
                closeFloatingBarTest: function() {
                    svcMcaTlb.api.closeFloatingToolbar(function(response){
                            console.log(response);
                    });                	
                },

                logoutAction: function(){                	
                    logoutAgent();
                    removeLoggedinAgent();
                },
*/                disconnect : function(){
                    self.subSocket.disconnect();
                }
            };
        }

        return new agentInfo();
    }    
    //============================= END agentInfo.js
    
    window.initCompanionPanel = function() {
        var liveConn = liveConnFactory(atmosphere);
        var agentInfo = agentInfoFactory(liveConn, constants, svcMca.tlb);
        
        window.tlbAgent = agentInfo;
        var agentId = "";

        window.showQueryFields = function(){
            var status = $('#queryData').css('display');
            if(status === 'none'){
                $('#queryData').show();
            }else{
                $('#queryData').hide();
            }
        };
        
        function userLoggedInInfo() {
            console.log("###########  Companion Panel User Registration Returned #########");
        };
        console.log("###########  Companion Panel Init Started #########");

        agentInfo.init(userLoggedInInfo);
        console.log("###########  Companion Panel Init CALLED #########");
    };
})();
