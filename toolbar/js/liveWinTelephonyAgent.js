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
			"agentMsgComPanel" : "agentMsg2ComPanel",
			"averageWaitTime" : "averageWaitTime",
    		"numberOfCalls" : "numberOfCalls",
    		"phoneLineId":"phoneLineId",
    		"eventId":"eventId",
    		"engagementId" : "SVCMCA_ENGAGEMENT_ID",
    		"chatTestMode" : "chatTestMode"
		},
		direction : {
			"inbound_call" : "ORA_SVC_INBOUND",
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
			"end" : "ENDCOMMUNICATION",
			"abandoned" : "ABANDONED"
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
			"TEST_RING_SELF_ACTION" : "TEST_RING_SELF",
			"TEST_CHAT_SELF_ACTION" : "TEST_CHAT_SELF",
			"TEST_HANGUP_CALL_ACTION" : "TEST_HANGUP_CALL",
			"UPDATE_COM_PANEL_ACTION" : "UPDATE_COM_PANEL",
			"COM_PANEL_NEW_CALL_ACTION" : "COM_PANEL_NEW_CALL",
			"COM_PANEL_REFRESH_ACTION" : "COM_PANEL_REFRESH",
			"NOTIFICATION_TIMER_EXP_ACTION" : "NOTIFICATION_TIMER_EXP",
			"GET_ACTIVE_ENGAGEMENTS_ACTION" : "getActiveEngagements",
			"CHAT_ACTION" : "CHAT",
			"DISCONNECT_ACTION" : "DISCONNECT",
			"PING_ADMIN_ACTION" : "PING_ADMIN",
			"PING_REPLY_ACTION" : "PPING_REPLY"
                },
		appClass : 'ORA_SERVICE',
		messageType : {
			"NOTIFICATION_READY" : "NOTIFICATION_READY",
			"END_CALL" : "END_CALL",
			"HOLD_CALL" :"HOLD_CALL",
			"UPDATE_COM_PANEL_MSG" : "UPDATE_COM_PANEL"
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
    //============================= START callInfo.js
    // Call info constructor method
    function callInfoFactory(constants, svcMcaTlb) { 
    	


        function callTimer(timerSlot) {
            var self = this;
            self.seconds = 0;
            self.minutes = 0;
            self.hours = 0;
            self.stopTime = true;
            self.timerSlot = timerSlot;

            function checkTime(inTime) {
                var retTime = ''+inTime;
                if (inTime < 10) {
                    retTime = '0'+inTime;                
                }
                return retTime;
            }
            function buildDisplayString(h, m, s) {
                var hh = checkTime(h);
                var mm = checkTime(m);
                var ss = checkTime(s);
                return hh+':'+mm+':'+ss;
            }
            function updateTimer() {
                if(self.stopTime) return;

                self.seconds++;
                if (self.seconds >= 60) {
                    self.seconds = 0;
                    self.minutes++;
                }
                if (self.minutes >= 60) {
                    self.minutes = 0;
                    self.hours++;
                }
                if (self.hours > 24) {
                    self.hours = 0;
                }
                self.timerSlot.text(buildDisplayString(self.hours, self.minutes, self.seconds));
                nextTick();
            }
            function nextTick() {
                var t = setTimeout(updateTimer, 1000);
            }

            return {
                startTimer: function(){                    
                    self.stopTime=false;
                    self.seconds = 0; 
                    self.minutes = 0;
                    self.hours = 0;

                    console.log("startTimer()."); 

                    self.timerSlot.show().text(buildDisplayString(self.hours, self.minutes, self.seconds));
                    nextTick();
                },
                startAdjustedTimer: function(time){

                   	var currentTime = Date.now();
                	var diff = 0;
                	
                	if (time) {
                		diff = Math.floor((currentTime - time)/1000);
                	}
                    self.stopTime=false;
                    
                    console.log("startAdjustedTimer() with diff: " + diff + " seconds action: Event Time is " + time + "; currentTime is " + currentTime+ "."); 

                	if (diff < 1) {
                      self.seconds = 0; 
                      self.minutes = 0;
                      self.hours = 0;
                    } else {
                    	self.hours =  Math.floor(diff/3600);
                    	self.minutes = Math.floor((diff - self.hours * 3600)/60);
                    	self.seconds = diff - self.hours * 3600 - self.minutes*60;
                    }
                
                    self.timerSlot.show().text(buildDisplayString(self.hours, self.minutes, self.seconds));
                    nextTick();
                },
                stopTimer: function(){
                    self.timerSlot.hide();
                    self.stopTime=true;
                },
                holdTimer: function(){
                    if(self.stopTime){
                        nextTick();
                    } 
                    self.stopTime=!self.stopTime;
                },
                pauseTimer: function(){
                    self.stopTime=true;

                },
                resumeTimer: function(){
                    if(self.stopTime){
                        nextTick();
                    } 
                    self.stopTime=false;
                },
                resetTimer: function() {
                	
                    console.log("resetTimer()."); 

                    self.seconds = 0;
                    self.minutes = 0;
                    self.hours = 0;                            
                },
                resetAdjustedTimer: function(time) {
                   	var currentTime = Date.now();
                	var diff = 0;
                	
                	if (time) {
                		diff = Math.floor((currentTime - time)/1000);
                	}

                    console.log("resetAdjustedTimer() with diff: " + diff + " seconds action: Event Time is " + time + "; currentTime is " + currentTime+ "."); 

                	if (diff < 1) {
                      self.seconds = 0; 
                      self.minutes = 0;
                      self.hours = 0;
                    } else {
                    	self.hours =  Math.floor(diff/3600);
                    	self.minutes = Math.floor((diff - self.hours * 3600)/60);
                    	self.seconds = diff - self.hours * 3600 - self.minutes*60;
                    }
                }

            };
        }

        function callInfo(phonelineId) {
            var self = this;

            self.eventUuid = null;
            self.callStates = { none: 'none', connecting: 'conn', ringing: 'ring', ongoing: 'on' };
            self.state = self.callStates.none;
            self.onHold = false;
            self.inboundData = {};
            self.direction = constants.direction.inbound_call;
            self.timer = null;
            self.phoneLineId = phonelineId;
            self.slot = null;
            self.lookupObj = null;
            self.restoreState = false;
            // HZH 20180503
            self.ringTime = null;
            self.acceptTime = null;
            self.appClassfication = 'ORA_SERVICE';
            self.wrapupType="";        
            self.displayOutgoingButtons = function (mode) {
                if (!self.slot) return;
                self.slot.removeClass('holdState');
                self.slot.find('.userIcon').removeClass('holdState');
                self.slot.find('.phoneIcon').show();
                self.slot.find('.phoneHold').hide();
                self.slot.find('.chatIcon').hide();
                self.slot.find('.timerBox').removeClass('timerPaused').removeClass('timerOngoing');
                self.slot.find('#accept').hide();
                self.slot.find('#btnSep1').hide();
                self.slot.find('#reject').hide();  

                if (mode === 'conn') {
                    self.slot.find("#hold").hide();
                    self.slot.find("#end").show();
                    self.slot.find("#transfer").hide();
                    self.slot.find("#compTransfer").hide();
                    self.slot.find('#btnSepOngoing').hide();
                } else if (mode === 'on') {
                    self.slot.find("#hold").show();
                    self.slot.find("#end").show();
                    self.slot.find("#transfer").show();
                    self.slot.find("#compTransfer").hide();
                    self.slot.find('#btnSepOngoing').show();
                } else {
                    self.slot.find("#hold").hide();
                    self.slot.find("#end").hide();
                    self.slot.find("#transfer").hide();
                    self.slot.find("#compTransfer").hide();
                    self.slot.find('#btnSepOngoing').hide();
                }           
            };
            self.setIncomingRingMode = function() {
                if (!self.slot) return;
                self.slot.removeClass('holdState');
                self.slot.find('.userIcon').removeClass('holdState');
                self.slot.find('.phoneIcon').show();
                self.slot.find('.phoneHold').hide();
                self.slot.find('.chatIcon').hide();

                self.slot.find('#accept').show();
                self.slot.find('#btnSep1').show();
                self.slot.find('#reject').show();

                self.slot.find('#hold').hide();
                self.slot.find('#end').hide();
                self.slot.find('.timerBox').removeClass('timerPaused').removeClass('timerOngoing');

                self.slot.find('#transfer').hide();
                self.slot.find('#compTransfer').hide();
                self.slot.find('#conference').hide();
                // HZH 180814
                self.slot.find('#btnSepOngoing').hide();            
            };
            self.setOngoingMode = function() {
                //self.slot.removeClass('holdState');
                //self.slot.find('.userIcon').removeClass('holdState');
                self.slot.find('.phoneIcon').show();
                self.slot.find('.phoneHold').hide();
                self.slot.find('.chatIcon').hide();

                self.slot.find('#accept').hide();
                self.slot.find('#btnSep1').hide();
                self.slot.find('#reject').hide();
                self.slot.find('#compTransfer').hide();

                self.slot.find('#hold').show();
                self.slot.find('#end').show();
                self.slot.find('#hold').removeClass('holdBtnPaused');
                self.slot.find('.timerBox').removeClass('timerPaused').addClass('timerOngoing');

                self.slot.find('#transfer').show();
                self.slot.find('#btnSepOngoing').show();
                self.timer.resumeTimer();
            };
            self.setAddToConferenceMode = function(dest) {
                self.slot.removeClass('holdState');
                self.slot.find('.userIcon').removeClass('holdState');
                self.slot.find('.phoneIcon').show();
                self.slot.find('.phoneHold').hide();
                self.slot.find('.chatIcon').hide();

                self.slot.find('#accept').hide();
                self.slot.find('#btnSep1').hide();
                self.slot.find('#reject').hide();            
                self.slot.find('#compTransfer').hide();

                self.slot.find('#hold').show();
                self.slot.find('#end').show();
                self.slot.find('#hold').removeClass('holdBtnPaused');
                self.slot.find('.timerBox').removeClass('timerPaused').addClass('timerOngoing');

                self.slot.find('#transfer').show();
                self.slot.find('#btnSepOngoing').show();
                self.inboundData.SVCMCA_2NDARY_CALL_TYPE = constants.actions.ADD_TO_CONFERENCE_ACTION;
                self.inboundData.SVCMCA_DESTINATION_AGENT_NAME = dest;
                self.displayCustomerData();
                self.timer.resumeTimer();
            };
            self.setTransferredMode = function() {
                //self.slot.removeClass('holdState');
                //self.slot.find('.userIcon').removeClass('holdState');
                self.slot.find('.phoneIcon').show();
                self.slot.find('.phoneHold').hide();
                self.slot.find('.chatIcon').hide();

                self.slot.find('#accept').hide();
                self.slot.find('#btnSep1').hide();
                self.slot.find('#reject').hide();            

                self.slot.find('#hold').show();
                self.slot.find('#end').show();
                self.slot.find('#hold').removeClass('holdBtnPaused');
                self.slot.find('.timerBox').removeClass('timerPaused').addClass('timerOngoing');

                self.slot.find('#transfer').show();
                self.slot.find("#compTransfer").hide();
                self.slot.find('#btnSepOngoing').show();
                self.timer.resumeTimer();
                self.inboundData.SVCMCA_2NDARY_CALL_TYPE = constants.actions.RECEIVING_TRANSFER_ACTION;
                self.displayCustomerData();
            };
            self.setConferenceJoinMode = function() {
                //self.slot.removeClass('holdState');
                //self.slot.find('.userIcon').removeClass('holdState');
                self.slot.find('.phoneIcon').show();
                self.slot.find('.phoneHold').hide();
                self.slot.find('.chatIcon').hide();

                self.slot.find('#accept').hide();
                self.slot.find('#btnSep1').hide();
                self.slot.find('#reject').hide();            

                self.slot.find('#hold').show();
                self.slot.find('#end').show();
                self.slot.find('#hold').removeClass('holdBtnPaused');
                self.slot.find('.timerBox').removeClass('timerPaused').addClass('timerOngoing');

                self.slot.find('#transfer').show();
                self.slot.find("#compTransfer").hide();
                self.slot.find('#btnSepOngoing').show();
                self.timer.resumeTimer();
                self.inboundData.SVCMCA_2NDARY_CALL_TYPE = constants.actions.JOIN_CONFERENCE_ACTION;
                self.displayCustomerData();
            };
            self.setConsultAcknowledgeMode = function() {
                //self.slot.removeClass('holdState');
                //self.slot.find('.userIcon').removeClass('holdState');
                self.slot.find('.phoneIcon').show();
                self.slot.find('.phoneHold').hide();
                self.slot.find('.chatIcon').hide();

                self.slot.find('#accept').hide();
                self.slot.find('#btnSep1').hide();
                self.slot.find('#reject').hide();            

                self.slot.find('#hold').show();
                self.slot.find('#end').show();
                self.slot.find('#hold').removeClass('holdBtnPaused');
                self.slot.find('.timerBox').removeClass('timerPaused').addClass('timerOngoing');

                self.slot.find('#transfer').hide();
                self.slot.find("#conference").show();
                self.slot.find("#compTransfer").show();
                self.slot.find('#btnSepOngoing').show();
                self.timer.resumeTimer();
            };
            self.setHoldMode = function() {
                self.slot.addClass('holdState');
                self.slot.find('.userIcon').addClass('holdState');
                self.slot.find('.phoneIcon').hide();
                self.slot.find('.phoneHold').show();
                self.slot.find('.chatIcon').hide();

                self.slot.find('#accept').hide();
                self.slot.find('#btnSep1').hide();
                self.slot.find('#reject').hide();            

                self.slot.find('#hold').show();            
                self.slot.find('#end').show();
                self.slot.find('#hold').addClass('holdBtnPaused');
                self.slot.find('.timerBox').removeClass('timerOngoing').addClass('timerPaused');

                self.slot.find('#transfer').show();
                self.slot.find("#compTransfer").hide();
                self.slot.find('#btnSepOngoing').show();
                self.timer.pauseTimer();

            };
            
            // HZH 20180919 *******
            
            self.parseBarOrigin = function(barOrigin) {
            	 var parser = document.createElement('a'),
            	 			searchObject = {},
            	 			queries, split, i;
            	 parser.href = barOrigin;
            	 queries = parser.search.replace(/^\?/, '').split('&');
                 for( i = 0; i < queries.length; i++ ) {
                     split = queries[i].split('=');
                     searchObject[split[0]] = split[1];
                 }
                 return {
                     protocol: parser.protocol,
                     host: parser.host,
                     hostname: parser.hostname,
                     port: parser.port,
                     pathname: parser.pathname,
                     search: parser.search,
                     searchObject: searchObject,
                     hash: parser.hash
                 };
            };
            
            // HZH 20180919 *******
            self.showParseBarOrigin = function(barOrigin) {
            	
            	var a = self.parseBarOrigin(barOrigin);
            	console.log("Bar Origin protocol: " + a.protocol);
            	console.log("Bar Origin host: " + a.host);	
            	console.log("Bar Origin hostname: " + a.hostname);	
            	console.log("Bar Origin pathname: " + a.pathname);	
            	console.log("Bar Origin search: " + a.search);
            	console.log("Bar Origin searchObject: " + a.searchObject);	
            	console.log("Bar Origin hash: " + a.hash);
            	
            	var validationRegex = "^(http|https):\/\/|[\*\.]|[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,6}(:[0-9]{1,5})";
            	var myArray;
            	while ((myArray = validationRegex.exec(barOrigin)) !== null) {
            	  var msg = 'Found ' + myArray[0] + '. ';
            	  msg += 'Next match starts at ' + validationRegex.lastIndex;
            	  console.log(msg);
            	}
            	
            };

            // HZH 20180919 ******
            self.invalidDomainName = function(validDomainName, eventOrigin) {

            	var msg = 'Verify Valid Domain Name: "' +validDomainName + '" with event Orgin: "' + eventOrigin + '"';

                var checkDomainName = validDomainName.toUpperCase();
                var originParser = document.createElement('a');        
                    originParser.href =  eventOrigin;
                
                var eventOriginHostName = originParser.hostname.toUpperCase();        
                var checkDomainArray = checkDomainName.split('.');
                var eventOrginNameArray = eventOriginHostName.split('.');
                
                var checkValidDomainNameSize = checkDomainArray.length;
                var eventOriginNameSize = eventOrginNameArray.length;

            	if (eventOriginNameSize < checkValidDomainNameSize) {
                    /* Check domain name failed, event Origin domain name is shorter than valid domain name */
                    return true;
                }

                for (i = 1; i <= checkValidDomainNameSize; i++) {

                	/* Check from Array top to bottom, i.e. from tld, then right to left in domain name */ 
                	var domainItem = checkDomainArray[checkValidDomainNameSize-i];
                	var eventItem = eventOrginNameArray[eventOriginNameSize-i];
                    if (domainItem !== eventItem) {
                    	console.log(msg + ' result: invalid');
                        return true;
                    }
                }

            	console.log(msg + ' result: valid');
                return false;
            };

            self.firstLetterUpperCase = function(string){
                return string.substr(0, 1).toUpperCase()+string.substr(1).toLowerCase();
            };
            self.displayCustomerData = function () {
                if (self.slot) {
                    console.log("Toolbar displayCustomerData: "+JSON.stringify(self.inboundData));

                    if(self.inboundData){
                        var primaryInfo = self.slot.find('.primaryInfo');
                        var secondaryInfo = self.slot.find('.secondaryInfo');
                        primaryInfo.html('');
                        secondaryInfo.html('');
                        
                        if (self.inboundData.SVCMCA_DISPLAY_NAME) {
                            primaryInfo.html(self.inboundData.SVCMCA_DISPLAY_NAME);
                            if (self.inboundData.SVCMCA_ANI) {
                                secondaryInfo.html(self.inboundData.SVCMCA_ANI);
                            }
                        } else if (self.inboundData.SVCMCA_CONTACT_NAME) {
                            primaryInfo.html(self.inboundData.SVCMCA_CONTACT_NAME);
                            if (self.inboundData.SVCMCA_ANI) {
                                secondaryInfo.html(self.inboundData.SVCMCA_ANI);
                            }
                        } else if (self.inboundData.SVCMCA_ORG_NAME) {
                            primaryInfo.html(self.inboundData.SVCMCA_ORG_NAME);
                            if (self.inboundData.SVCMCA_ANI) {
                                secondaryInfo.html(self.inboundData.SVCMCA_ANI);
                            }
                        } else if (self.inboundData.SVCMCA_SR_NUM) {
                            primaryInfo.html(self.inboundData.SVCMCA_SR_NUM);
                        } else if (self.inboundData.SVCMCA_SR_TITLE) {
                            primaryInfo.html(self.inboundData.SVCMCA_SR_TITLE);
                            if (self.inboundData.SVCMCA_SR_NUM) {
                                secondaryInfo.html(self.inboundData.SVCMCA_SR_NUM);
                            }
                        } else if (self.inboundData.SVCMCA_OPPORTUNITY_NAME) {
                            primaryInfo.html(self.inboundData.SVCMCA_OPPORTUNITY_NAME);
                            if (self.inboundData.SVCMCA_OPPORTUNITY_NUMBER) {
                                secondaryInfo.html(self.inboundData.SVCMCA_OPPORTUNITY_NUMBER);
                            }
                        } else if (self.inboundData.SVCMCA_LEAD_NAME) {
                            primaryInfo.html(self.inboundData.SVCMCA_LEAD_NAME);
                            if (self.inboundData.SVCMCA_LEAD_NUMBER) {
                                secondaryInfo.html(self.inboundData.SVCMCA_LEAD_NUMBER);
                            }
                        } else if (self.inboundData.SVCMCA_ANI) {
                            secondaryInfo.html(self.inboundData.SVCMCA_ANI);
                        }
                        if ((self.inboundData.SVCMCA_2NDARY_CALL_TYPE == constants.actions.CONFERENCE_ACTION || 
                                self.inboundData.SVCMCA_2NDARY_CALL_TYPE	== constants.actions.CONSULT_ACTION) &&
                                self.inboundData.SVCMCA_INITIATING_AGENT_NAME != self.author){
                                primaryInfo.html(self.firstLetterUpperCase(self.inboundData.SVCMCA_2NDARY_CALL_TYPE) +" from ");
                                secondaryInfo.html(self.inboundData.SVCMCA_INITIATING_AGENT_NAME);
                                //primaryInfo.html(self.firstLetterUpperCase(self.inboundData.SVCMCA_2NDARY_CALL_TYPE) +": " + self.inboundData.SVCMCA_INITIATING_AGENT_NAME);
                                //secondaryInfo.html(self.inboundData.SVCMCA_CONTACT_NAME);
                        } else if (self.inboundData.SVCMCA_2NDARY_CALL_TYPE == constants.actions.RECEIVING_TRANSFER_ACTION &&
                                        self.inboundData.SVCMCA_INITIATING_AGENT_NAME != self.author){
                                        primaryInfo.html(self.firstLetterUpperCase(self.inboundData.SVCMCA_2NDARY_CALL_TYPE));
                                        //secondaryInfo.html(self.inboundData.SVCMCA_INITIATING_AGENT_NAME);
                                        //primaryInfo.html(self.firstLetterUpperCase(self.inboundData.SVCMCA_2NDARY_CALL_TYPE) +": " + self.inboundData.SVCMCA_INITIATING_AGENT_NAME);
                                        if (self.inboundData.SVCMCA_DISPLAY_NAME) {
                                        	secondaryInfo.html(self.inboundData.SVCMCA_DISPLAY_NAME);
                                        } else {
                                        	secondaryInfo.html(self.inboundData.SVCMCA_CONTACT_NAME);
                                        }
                        } else if ((self.inboundData.SVCMCA_2NDARY_CALL_TYPE == constants.actions.JOIN_CONFERENCE_ACTION) &&
                                self.inboundData.SVCMCA_INITIATING_AGENT_NAME != self.author){
                                //primaryInfo.html("Conference: " + self.inboundData.SVCMCA_INITIATING_AGENT_NAME);
                                primaryInfo.html("Conference");
                                if (self.inboundData.SVCMCA_DISPLAY_NAME) {
                                	secondaryInfo.html(self.inboundData.SVCMCA_DISPLAY_NAME);
                                } else {
                                	secondaryInfo.html(self.inboundData.SVCMCA_CONTACT_NAME);
                                }
                       } else if ((self.inboundData.SVCMCA_2NDARY_CALL_TYPE == constants.actions.ADD_TO_CONFERENCE_ACTION) &&
                                   self.inboundData.SVCMCA_DESTINATION_AGENT_NAME != self.author){
                                console.log("Toolbar displayCustomerData for : ADD_TO_CONFERENCE_ACTION "+JSON.stringify(self.inboundData));
                                console.log("Conference with " + self.inboundData.SVCMCA_DESTINATION_AGENT_NAME);
                                console.log("Conference Contact " + self.inboundData.SVCMCA_CONTACT_NAME);
                                //primaryInfo.html("Conference: " + self.inboundData.SVCMCA_DESTINATION_AGENT_NAME);
                                primaryInfo.html("Conference");
                                if (self.inboundData.SVCMCA_DISPLAY_NAME) {
                                	secondaryInfo.html(self.inboundData.SVCMCA_DISPLAY_NAME);
                                } else {
                                	secondaryInfo.html(self.inboundData.SVCMCA_CONTACT_NAME);
                                }
                       }
                    }            
                }
            };
            self.updateCallSlotData = function(primaryInfo, secondaryInfo){
                self.slot.find('.primaryInfo').html(primaryInfo);
                self.slot.find('.secondaryInfo').html(secondaryInfo);
            }
            self.updateSecondSlotData = function(actionType, agent){
                var primaryInfo = self.slot.find('.primaryInfo');
                var secondaryInfo = self.slot.find('.secondaryInfo');


                if(constants.actions.INVITE_CONFERENCE_ACTION == actionType){
                    primaryInfo.html(constants.strings[actionType]);
                    secondaryInfo.html('');
                }else{
                    primaryInfo.html(constants.strings[actionType]);
                    secondaryInfo.html(agent);
                }

            },
            self.acceptCall = function () {
 
                if(self.slot === null){
                    self.direction = constants.direction.inbound_call;
                    self.slot = $('#toolbar').clone();
                    self.slot.attr("id", self.eventUuid);
                    self.slot.find('#accept').click(self.acceptCall);
                    self.slot.find('#reject').click(self.rejectCall);
                    self.slot.find('#hold').click(self.holdCall);
                    self.slot.find('#end').click(self.endCall);
                    self.slot.find('#conference').click(self.conferenceCall);
                    self.slot.find('#transfer').click(self.transferCall);
                    self.slot.appendTo($('#calls'));
                    self.slot.show();
                    self.timer = new callTimer(self.slot.find('#timer'));
                    self.slot.find('#timer').removeClass('timerOngoing').addClass('timerRing');
                    //HZH 20180503
                    //self.timer.startTimer();
                    self.timer.startAdjustedTimer(self.acceptTime);
                    //event.preventDefault();
                }
                self.state = self.callStates.ongoing;
                self.slot.find('.timerBox').show();

                if (self.direction === constants.direction.inbound_call) {
                    //HZH 20180503
                    //self.timer.resetTimer();
                    self.timer.resetAdjustedTimer(self.acceptTime);

                    self.setOngoingMode();
                } else {
                    //HZH 20180503
                    //self.timer.startTimer();
                    self.timer.startAdjustedTimer(self.acceptTime);

                    self.displayOutgoingButtons('on');
                }
                svcMcaTlb.api.startCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, function(response){                    
                //svcMcaTlb.api.startCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, function(response){                    
                //svcMcaTlb.api.startCommEvent(constants.events.channelPhone, constants.appClass, myUuid, self.inboundData, function(response){                    
                    console.log("Toolbar startCommEvent reposnse: "+JSON.stringify(response));

                    for (var token in response.outData) {
                        self.inboundData[token] = response.outData[token];
                    }
                    self.displayCustomerData();
                    self.subSocket.publish(JSON.stringify({ author: self.author, callData: self.getCallData(), action: constants.actions.UPDATE_IVR_DATA_ACTION }));


                }, constants.channelType.phoneChannelType);
                if (self.direction === constants.direction.inbound_call) {
                    self.subSocket.publish(JSON.stringify({ author: self.author, callData: self.getCallData(), action: constants.actions.ACCEPT_ACTION }));
                }
            };
            self.rejectCall = function () {
                $('#reverseLookupFinished').attr('value', 'false');
                self.state = self.callStates.none;
                self.timer.stopTimer();
                //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, constants.reason.reject, function(response){
                svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, constants.reason.reject, function(response){
                    console.log("reposnse: "+JSON.stringify(response));
                }, constants.channelType.phoneChannelType);
                self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':self.eventUuid, 'phoneLineId':self.phoneLineId}, action: constants.actions.REJECT_ACTION }));     
                //event.preventDefault();
            };
            self.rejectCallByTimeExp = function () {
                $('#reverseLookupFinished').attr('value', 'false');
                self.state = self.callStates.none;
                self.timer.stopTimer();
                
      
                //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, constants.reason.end, function(response){
                //    console.log("reposnse: "+JSON.stringify(response));
                //}, constants.channelType.phoneChannelType);
                //self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':self.eventUuid}, action: constants.actions.REJECT_ACTION }));            

                // HZH 20171107 have to use close comm by reject right now
                //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, constants.reason.reject, function(response){
                svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, constants.reason.reject, function(response){
                    console.log("reposnse: "+JSON.stringify(response));
                }, constants.channelType.phoneChannelType);
                self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':self.eventUuid, 'phoneLineId':self.phoneLineId}, action: constants.actions.REJECT_ACTION }));            
            };
            self.endCall = function () {
            	            	
                $('#reverseLookupFinished').attr('value', 'false');
                if (self.direction === constants.direction.outbound_call && self.state !== self.callStates.ongoing) {
                    self.state = self.callStates.none;
                    svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, constants.reason.missed, function(response){
                    //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, constants.reason.missed, function(response){
                    //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, myUuid, self.inboundData, constants.reason.missed, function(response){
                        console.log("reposnse: "+JSON.stringify(response));
                    }, constants.channelType.phoneChannelType);
                } else {      
                	
                	var abandoned = self.state == self.callStates.ringing ? true : false;
                    self.state = self.callStates.none;
                    self.timer.stopTimer();

                    var timeout = self.inboundData[constants.attributes.wrapupSeconds] == "" ? 0 : self.inboundData[constants.attributes.wrapupSeconds];
                
                    var endReason = constants.reason.wrapup;
                    
                    if(self.wrapupType == constants.wrapupType.unlimitedWrapup || self.wrapupType == constants.wrapupType.timedWrapup){
                    	if (abandoned) {
                    		endReason = constants.reason.abandoned;
                    	} else {
                    		endReason = constants.reason.wrapup;
                    	}

                        svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, endReason, function(response){
                        //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, constants.reason.wrapup, function(response){
                        //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, myUuid, self.inboundData, constants.reason.wrapup, function(response){
                            console.log("reposnse: "+JSON.stringify(response));
                        }, constants.channelType.phoneChannelType);
                    }

                    if(self.wrapupType == constants.wrapupType.noWrapup || (self.wrapupType == constants.wrapupType.timedWrapup && timeout > 0)){

                    	if (abandoned) {
                    		endReason = constants.reason.abandoned;
                    	} else {
                    		endReason = constants.reason.end;
                    	}

                        setTimeout( 
                            function(){
                                svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, endReason, function(response){
                                //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, constants.reason.end, function(response){
                                //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, myUuid, self.inboundData, constants.reason.end, function(response){
                                    console.log("reposnse: "+JSON.stringify(response));
                                }, constants.channelType.phoneChannelType)
                            }
                            ,timeout * 1000
                        );
                    }

                }		

                self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':self.eventUuid,'phoneLineId':self.phoneLineId}, action: constants.actions.END_ACTION }));
                //self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':myUuid}, action: constants.actions.END_ACTION }));
                
                //event.preventDefault();
            };
            self.endTransferCall = function () {
                $('#reverseLookupFinished').attr('value', 'false');
                if (self.direction === constants.direction.outbound_call && self.state !== self.callStates.ongoing) {
                    self.state = self.callStates.none;
                    self.inboundData['SVCMCA_2NDARY_CALL_TYPE'] = 'COMPLETE_TRANSFER';
                    //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, constants.reason.missed, function(response){
                    svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, constants.reason.missed, function(response){
                        console.log("reposnse: "+JSON.stringify(response));
                    }, constants.channelType.phoneChannelType);
                } else {            
                    self.state = self.callStates.none;
                    self.timer.stopTimer();

                    var timeout = self.inboundData[constants.attributes.wrapupSeconds] == "" ? 0 : self.inboundData[constants.attributes.wrapupSeconds];


                    if(self.wrapupType == constants.wrapupType.unlimitedWrapup || self.wrapupType == constants.wrapupType.timedWrapup){
                    	self.inboundData['SVCMCA_2NDARY_CALL_TYPE'] = 'COMPLETE_TRANSFER';
                        //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, constants.reason.wrapup, function(response){
                        svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, constants.reason.wrapup, function(response){
                            console.log("reposnse: "+JSON.stringify(response));
                        }, constants.channelType.phoneChannelType);
                    }

                    if(self.wrapupType == constants.wrapupType.noWrapup || (self.wrapupType == constants.wrapupType.timedWrapup && timeout > 0)){
                        setTimeout( 
                            function(){
                            	self.inboundData['SVCMCA_2NDARY_CALL_TYPE'] = 'COMPLETE_TRANSFER';
                                //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, constants.reason.end, function(response){
                                svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, constants.reason.end, function(response){
                                     console.log("reposnse: "+JSON.stringify(response));
                                }, constants.channelType.phoneChannelType)
                            }
                            ,timeout * 1000
                        );
                    }
                }		

                self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':self.eventUuid,'phoneLineId':self.phoneLineId}, action: constants.actions.END_TRANSFER_ACTION }));
                
                //event.preventDefault();
            };
            self.endConference= function () {
                self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':self.eventUuid,'phoneLineId':self.phoneLineId}, action: constants.actions.END_CONFERENCE_ACTION }));
            }
            self.holdCall = function () {

                self.onHold = !self.onHold;
                if (self.onHold) {
                    self.setHoldMode();
                } else {
                    self.setOngoingMode();
                }
            };
            self.transferCall = function () {

                //var vToolbarUrl = window.location.href.replace("liveWinTelephony.html","vToolbar.html"); 
                //var vToolbarUrl = window.location.href; 
                //svcMcaTlb.api.openFloatingToolbar("FLOAT_VERTICAL", vToolbarUrl, null, null, {"showAgents":"true", "eventId":self.eventUuid}, function(response){
                //    console.log(response);
                //});

            	//self.vToolbar.openVToolbar();
            	//$('#VToolbar').show();
                self.subSocket.publish(JSON.stringify({ author: self.author, callData: {"eventId":self.eventUuid,'phoneLineId':self.phoneLineId}, action: constants.actions.TRANSFER_ACTION }));   
                
                //event.preventDefault();

            };
            self.conferenceCall = function () {
                 //var vToolbarUrl = window.location.href.replace("liveWinTelephony.html","vToolbar.html"); 
                //var vToolbarUrl = window.location.href.replace("index.html","vToolbar.html"); 
                 //var vToolbarUrl = window.location.href; 
                //svcMcaTlb.api.openFloatingToolbar("FLOAT_VERTICAL", vToolbarUrl, null, null, {"showAgents":"true", "eventId":self.eventUuid, "conference":'true'}, function(response){
                //    console.log(response);
                //});
            	//self.vToolbar.openVToolbar();
                //$('#VToolbar').show();
                self.subSocket.publish(JSON.stringify({ author: self.author, callData: {"eventId":self.eventUuid,'phoneLineId':self.phoneLineId}, action: constants.actions.CONFERENCE_ACTION }));        
                //event.preventDefault();
            };

            //self.transferConsultCall = function(){
            //	
            //	var agentId = self.slot.find('.secondaryInfo').html();
            //	var transferData = {};

            //	transferData['agentId']=agentId;

            //	self.subSocket.publish(JSON.stringify({ author: self.author, data: JSON.stringify(transferData), action: constants.actions.COMPLETE_TRANSFER_ACTION }));
            //}
            self.completeTransferCall = function(){

                console.log("Insider completeTransferCall.");

                var agentId = self.slot.find('.secondaryInfo').html();
                var transferData = {};

                transferData['agentId']=agentId;
                transferData['phoneLineId']=self.phoneLineId;
                transferData['SVCMCA_2NDARY_CALL_TYPE']='COMPLETE_TRANSFER';

                self.inboundData.SVCMCA_DESTINATION_AGENT_NAME = agentId;
                self.subSocket.publish(JSON.stringify({ author: self.author, data: JSON.stringify(transferData), action: constants.actions.COMPLETE_TRANSFER_ACTION }));
                
                //event.preventDefault();
            }
            self.addToConferenceCall = function(){

                console.log("Inside callinfo addToConferenceCall.");

                var agentId = self.slot.find('.secondaryInfo').html();
                var conferenceData = {};

                conferenceData['agentId']=agentId;
                conferenceData['phoneLineId']=self.phoneLineId;

                self.subSocket.publish(JSON.stringify({ author: self.author, data: JSON.stringify(conferenceData), action: constants.actions.ADD_TO_CONFERENCE_ACTION }));
            }
            self.consultAcknowledged = function(){
                self.setConsultAcknowledgeMode();
            }
            self.getCallData = function(){

                var callData = {};
                var ivrData = {};
                callData['eventId']=self.eventUuid;
                callData['phoneLineId']=self.phoneLineId;

                for (var token in self.inboundData) {
                    if(constants.callDataAttributes[token]){
                    	if (token != 'phoneLineId') {
                    		callData[token] = self.inboundData[token];
                    	}
                    }else{
                        ivrData[token] = self.inboundData[token];
                    }
                }

                callData[constants.attributes.ivrData] = JSON.stringify(ivrData);

                return callData;
            };
            return {
                slot : self.slot,
                setAtmosphereSocket : function(atmosphereSocket){
                    self.subSocket =  atmosphereSocket;
                    var originalPublish = self.subSocket.publish;

                    self.subSocket.publish = function(message){
                        if(!self.restoreState){
                            originalPublish(message);
                        }
                    }
                },
                setAuthor : function(author){
                    self.author = author;
                },
                createAndShowCallSlot: function(direction) {
                	
                	// HZH 20180919 *******
                	//self.showParseBarOrigin('https://slc10gma.us.oracle.com:8447');
                	//self.showParseBarOrigin('slc10gma.us.oracle.com:8447');
                	//self.showParseBarOrigin('https://slc10gma.us.oracle.com');
                	//self.showParseBarOrigin('slc10gma.us.oracle.com');
                	//self.showParseBarOrigin('https://*.oracle.com');
                	//self.showParseBarOrigin('*.oracle.com');
                	//self.invalidDomainName('oracle.com', 'https://slc10gma.us.oracle.com:8447');
                	//self.invalidDomainName('oracle.com', 'https://slc10gma.us.oracle.com');
                	//self.invalidDomainName('oracle.com', 'https://slc10gma.us.ooracle.com:8447');
                	//self.invalidDomainName('oracle.com', 'https://slc12pfx.us.ooracle.com');
                	//self.invalidDomainName('oracle.com', 'https://oracle.com');
                	//self.invalidDomainName('slc12pfx.us.oracle.com', 'https://oracle.com');
                	
                    $('#reverseLookupFinished').attr('value', 'false');
                    self.direction = direction;
                    self.wrapupType = self.inboundData.wrapupType;
                    delete self.inboundData.wrapupType;

                    //var myUuid = self.eventUuid;
                    self.inboundData[constants.attributes.communicationDirection] = direction;
                    self.slot = $('#toolbar').clone();
                    self.slot.attr("id", self.eventUuid);
                    
                    $('#toolbarControl').attr("id", self.eventUuid);
                    
                    //var pp = self.slot.find('#callerBtnReg2');
                    //pp.attr("id", self.eventUuid);
                    
                    self.slot.find('#accept').click(self.acceptCall);
                    self.slot.find('#reject').click(self.rejectCall);
                    self.slot.find('#hold').click(self.holdCall);
                    self.slot.find('#end').click(self.endCall);
                    self.slot.find('#conference').click(self.conferenceCall);
                    self.slot.find('#transfer').click(self.transferCall);
                    self.slot.appendTo($('#calls'));
                    self.slot.show();
                    self.timer = new callTimer(self.slot.find('#timer'));
                    self.slot.find('#timer').removeClass('timerOngoing').addClass('timerRing');
                    if (self.direction === constants.direction.inbound_call) {
                    	//HZH 20180503
                        //self.timer.startTimer();
                    	self.timer.startAdjustedTimer(self.ringTime);
                        self.slot.find('.timerBox').show();
                        self.state = self.callStates.ringing;
                        self.setIncomingRingMode();
                        self.slot.find('#phoneEvent').text("");
                        self.displayCustomerData();
                        function processNewCommEventResponse(response) {
                            console.log("-------- Toolbar newCommEvent response: "+JSON.stringify(response));
                            $('#reverseLookupFinished').attr('value', 'true');
                            for (var token in response.outData) {
                                self.inboundData[token] = response.outData[token];
                            }
                            self.displayCustomerData();
                            svcMcaTlb.api.postToolbarMessage(JSON.stringify(self.inboundData), function(response) {
                                console.log("======== Response for POST from newCommEvent response after RL - status: "+response.result);
                            });
                        }
                        if (self.lookupObj) {
                            //svcMcaTlb.api.newCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, self.lookupObj, processNewCommEventResponse, constants.channelType.phoneChannelType);
                            svcMcaTlb.api.newCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, self.lookupObj, processNewCommEventResponse, constants.channelType.phoneChannelType);
                        } else {
                            //svcMcaTlb.api.newCommEvent(constants.events.channelPhone, constants.appClass, self.eventUuid, self.inboundData, null, processNewCommEventResponse, constants.channelType.phoneChannelType);
                            svcMcaTlb.api.newCommEvent(constants.events.channelPhone, self.appClassfication, self.eventUuid, self.inboundData, null, processNewCommEventResponse, constants.channelType.phoneChannelType);
                        }
                    } else {
                        self.slot.find('.timerBox').hide();
                        self.state = self.callStates.connecting;
                        self.displayOutgoingButtons('conn');					
                        self.slot.find('#phoneEvent').text("Dialing ...");
                        self.displayCustomerData();
                    }

                },
                createAndShowConsultSlot: function() {
                    $('#reverseLookupFinished').attr('value', 'false');

                    self.slot = $('#toolbar').clone();
                    self.slot.attr("id", self.eventUuid);
                    self.slot.find('#end').click(self.endConference);
                    self.slot.find("#compTransfer").click(self.completeTransferCall);
                    //self.slot.find('#transfer').click(self.transferConsultCall);
                    self.slot.find('#conference').click(self.addToConferenceCall);
                    //self.slot.find('#conference').click(self.conferenceCall);

                    //self.slot.find('#conference').show();
                    //self.slot.find('#compTransfer').show();
                    self.slot.find('#conference').hide();
                    self.slot.find('#compTransfer').hide();
                    self.slot.find('#transfer').hide();


                    self.slot.appendTo($('#calls'));
                    self.slot.show();
                    self.timer = new callTimer(self.slot.find('#timer'));
                    self.slot.find('#timer').removeClass('timerOngoing').addClass('timerRing');

                    self.timer.startTimer();
                    self.slot.find('.timerBox').show();
                    self.state = self.callStates.ringing;
                    self.setIncomingRingMode();
                    self.slot.find('#phoneEvent').text("");

                    self.slot.find('#accept').hide();
                    self.slot.find('#reject').hide();
                    self.slot.find('#end').show();

                },
                hideAndCloseCallSlot: function() {
                    if (self.slot !== null) {
                        self.slot.hide();
                        self.slot.remove();
                    }
                    self.slot = null;
                },
                acceptAcknowledged : function () {
                    if (self.state != self.callStates.ongoing) {
                        self.acceptCall();
                    }
                },
                rejectAcknowledged: function () {
                    if (self.state != self.callStates.none) {
                        self.rejectCall();
                    }
                },
                notificationTimerExpAcknowledged: function () {
                    if (self.state != self.callStates.none) {
                        self.rejectCallByTimeExp();
                    }
                },
                updateCallData: function() {
                    self.displayCustomerData();
                },
                updatePhoneEventTitle : function(title){
                     	self.slot.find('#phoneEvent').text(title);
                },
                getInboundData: function() {
                    return self.inboundData;
                },
                setInboundData: function(data) {
                    self.inboundData = data;
                },
                getAppClassification: function() {
                    return self.appClassfication;
                },
                setAppClassification: function(appCode) {
                	self.appClassfication = appCode;
                },
                getEventId: function() {
                    return self.eventUuid;
                },
                setEventId: function(data) {
                    self.eventUuid = data;
                },
                setCallState: function(state) {
                	self.state = state;
                },
                getDirection: function() {
                    return self.direction;
                },
                setDirection: function(dir) {
                    if (dir === constants.direction.inbound_call || dir === constants.direction.outbound_call) {
                        self.direction = dir;
                    }
                },
                getTimer: function() {
                    return self.timer;
                },
                getLookupObject: function() {
                    return self.lookupObj;
                },
                setLookupObject: function(lo) {
                    self.lookupObj = lo;
                },
                getWrapupType: function() {
                    return self.wrapupType;
                },
                setWrapupType: function(wt) {
                    self.wrapupType = wt;
                },
                setRestoreState: function(state){
                    self.restoreState=state;
                },
                setCallRingTime:function(time) {
                	self.ringTime = time;
                },
                setCallAcceptTime:function(time) {
                	self.acceptTime = time;
                },
                endCall : self.endCall,
                endTransferCall : self.endTransferCall,
                setHoldMode : self.setHoldMode,
                setOngoingMode : self.setOngoingMode,
                updateSecondSlotData : self.updateSecondSlotData,
                updateCallSlotData  : self.updateCallSlotData ,
                consultAcknowledged : self.consultAcknowledged,
                transferCall : self.transferCall,
                setTransferredMode : self.setTransferredMode,
                addToConferenceCall : self.addToConferenceCall,
                setConferenceJoinMode : self.setConferenceJoinMode,
                setAddToConferenceMode : self.setAddToConferenceMode
            };
        }

        return callInfo;
    };
    
    //============================= END callInfo.js
    function chatInfoFactory(constants, svcMcaTlb) { 

        function chatTimer(timerSlot) {
            var self = this;
            self.seconds = 0;
            self.minutes = 0;
            self.hours = 0;
            self.stopTime = true;
            self.timerSlot = timerSlot;

            function checkTime(inTime) {
                var retTime = ''+inTime;
                if (inTime < 10) {
                    retTime = '0'+inTime;                
                }
                return retTime;
            }
            function buildDisplayString(h, m, s) {
                var hh = checkTime(h);
                var mm = checkTime(m);
                var ss = checkTime(s);
                return hh+':'+mm+':'+ss;
            }
            function updateTimer() {
                if(self.stopTime) return;

                self.seconds++;
                if (self.seconds >= 60) {
                    self.seconds = 0;
                    self.minutes++;
                }
                if (self.minutes >= 60) {
                    self.minutes = 0;
                    self.hours++;
                }
                if (self.hours > 24) {
                    self.hours = 0;
                }
                self.timerSlot.text(buildDisplayString(self.hours, self.minutes, self.seconds));
                nextTick();
            }
            function nextTick() {
                var t = setTimeout(updateTimer, 1000);
            }

            return {
                startTimer: function(){                    
                    self.stopTime=false;
                    self.seconds = 0; 
                    self.minutes = 0;
                    self.hours = 0;
                    self.timerSlot.show().text(buildDisplayString(self.hours, self.minutes, self.seconds));
                    nextTick();
                },
                stopTimer: function(){
                    self.timerSlot.hide();
                    self.stopTime=true;
                },
                holdTimer: function(){
                    if(self.stopTime){
                        nextTick();
                    } 
                    self.stopTime=!self.stopTime;
                },
                pauseTimer: function(){
                    self.stopTime=true;

                },
                resumeTimer: function(){
                    if(self.stopTime){
                        nextTick();
                    } 
                    self.stopTime=false;
                },
                resetTimer: function() {
                    self.seconds = 0;
                    self.minutes = 0;
                    self.hours = 0;                            
                }
            };
        }

        function chatInfo(phonelineId) {
	        var self = this;
	
	        self.eventUuid = null;
	        self.engagementId = null;
	        self.uwoId = null;
	        self.interactionId = null;
	        self.chatStates = { none: 'none', connecting: 'conn', ringing: 'ring', ongoing: 'on' };
	        self.state = self.chatStates.none;
	        self.inboundData = {};
	        self.phoneLineId = phonelineId;
	
	        self.setIncomingChatMode = function() {
	            if (!self.slot) return;
	            self.slot.removeClass('holdState');
	            self.slot.find('.userIcon').removeClass('holdState');
	            self.slot.find('.phoneIcon').hide();
	            self.slot.find('.phoneHold').hide();
	            self.slot.find('.chatIcon').show();
	
	            self.slot.find('#accept').show();
	            self.slot.find('#btnSep1').hide();
	            self.slot.find('#reject').show();
	
	            self.slot.find('#hold').hide();
	            self.slot.find('#end').hide();
	            self.slot.find('.timerBox').removeClass('timerPaused').removeClass('timerOngoing');
	
	            self.slot.find('#transfer').hide();
	            self.slot.find('#compTransfer').hide();
	            self.slot.find('#btnSepOngoing').hide();            
	        };

        self.setChatOngoingMode = function() {
            //self.slot.removeClass('holdState');
            //self.slot.find('.userIcon').removeClass('holdState');
            self.slot.find('.phoneIcon').hide();
            self.slot.find('.phoneHold').hide();
            self.slot.find('.chatIcon').show();

            self.slot.find('#accept').hide();
            self.slot.find('#btnSep1').hide();
            self.slot.find('#reject').hide();
            self.slot.find('#compTransfer').hide();

            self.slot.find('#hold').hide();
            self.slot.find('#end').show();
            self.slot.find('#hold').removeClass('holdBtnPaused');
            self.slot.find('.timerBox').removeClass('timerPaused').addClass('timerOngoing');

            self.slot.find('#transfer').hide();
            self.slot.find('#btnSepOngoing').hide();
            self.timer.resumeTimer();
        };

        self.displayChatData = function () {
            if (self.slot) {
                console.log("Toolbar displayChatData: "+JSON.stringify(self.inboundData));

                if(self.inboundData){
                    var primaryInfo = self.slot.find('.primaryInfo');
                    var secondaryInfo = self.slot.find('.secondaryInfo');
                    primaryInfo.html('');
                    secondaryInfo.html('');
                    
                    if (self.inboundData.SVCMCA_ENGAGEMENT_ID) {
                        primaryInfo.html(self.inboundData.SVCMCA_ENGAGEMENT_ID);
                        if (self.inboundData.SVCMCA_UWO_ID) {
                            secondaryInfo.html(self.inboundData.SVCMCA_UWO_ID);
                        } else {
                        	secondaryInfo.html(" Missing SVCMCA_UWO_ID");
                        }
                    } else {
                        primaryInfo.html("Missing SVCMCA_ENGAGEMENT_ID");
                        if (self.inboundData.SVCMCA_UWO_ID) {
                            secondaryInfo.html(self.inboundData.SVCMCA_UWO_ID);
                        } else {
                        	secondaryInfo.html(" Missing SVCMCA_UWO_ID");
                        }
                    }           
                }
            }
        }

        self.acceptChat = function (chatTestMode) {
        	
        	if (chatTestMode == true) {
	            if(self.slot === null){
	                self.direction = constants.direction.inbound_call;
	                self.slot = $('#toolbar').clone();
	                self.slot.attr("id", self.eventUuid);
	                self.slot.find('#accept').click(self.acceptCall);
	                self.slot.find('#reject').click(self.rejectCall);
	                self.slot.appendTo($('#calls'));
	                self.slot.find('.phoneIcon').hide();
	                self.slot.find('.phoneHold').hide();
	                self.slot.find('.chatIcon').show();
	                self.slot.show();
	                self.timer = new callTimer(self.slot.find('#timer'));
	                self.slot.find('#timer').removeClass('timerOngoing').addClass('timerRing');
	                self.timer.startTimer();
	                //event.preventDefault();
	            
		            self.state = self.chatStates.ongoing;
		            self.slot.find('.timerBox').show();
		
		            self.timer.resetTimer();
		            self.setChatOngoingMode();

	            } else {            
	            	self.state = self.chatStates.ongoing;
	            	self.setChatOngoingMode();
	            }
        	} else {
        		self.state = self.chatStates.ongoing;
        	}

            //svcMcaTlb.api.startCommEvent(constants.events.channelChat, constants.appClass, self.eventUuid, self.inboundData, function(response){                    
            svcMcaTlb.api.startCommEvent(constants.events.channelChat, self.appClassfication, self.eventUuid, self.inboundData, function(response){                    
                console.log("Toolbar Chat startCommEvent reposnse: "+JSON.stringify(response));

            }, constants.channelType.chatChannelType);
            
            //svcMcaTlb.api.agentStateEvent(constants.events.channelChat,"1",true,true,"AVAILABLE","Busy. On Chat",null,"Busy",{"phoneLineId":self.phoneLineId}, function(response) {
         	//   console.log("<RefToolbar> response for Chat agentStateEvent received!!! Reponse " + JSON.stringify(response));
            //}, constants.channelType.chatChannelType);
            
            if (chatTestMode == true) {
               self.subSocket.publish(JSON.stringify({ author: self.author, callData: self.getCallData(), action: constants.actions.ACCEPT_ACTION }));
            };
        }
        self.acceptChatTestMode = function () {
        	
               self.subSocket.publish(JSON.stringify({ author: self.author, callData: self.getCallData(), action: constants.actions.ACCEPT_ACTION }));
        }
        self.rejectChat = function (chatTestMode) {
            self.state = self.chatStates.none;
            
            if (chatTestMode == true) {
	            self.timer.stopTimer();
            }
            
            //svcMcaTlb.api.closeCommEvent(constants.events.channelChat, constants.appClass, self.eventUuid, self.inboundData, constants.reason.reject, function(response){
            svcMcaTlb.api.closeCommEvent(constants.events.channelChat, self.appClassfication, self.eventUuid, self.inboundData, constants.reason.reject, function(response){
                console.log("reposnse: "+JSON.stringify(response));
            }, constants.channelType.chatChannelType);

            //svcMcaTlb.api.agentStateEvent(constants.events.channelChat,"1",true,true,"AVAILABLE","Idle",null,"Idle",{"phoneLineId":self.phoneLineId}, function(response) {
            //	   console.log("<RefToolbar> response for Chat agentStateEvent received!!! Reponse " + JSON.stringify(response));
           	//}, constants.channelType.chatChannelType);

            if (self.slot !== null) {
                self.slot.hide();
                self.slot.remove();
            }
            self.slot = null;

            self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':self.eventUuid, 'phoneLineId':self.phoneLineId}, action: constants.actions.REJECT_ACTION })); 
        };
        self.rejectChatByTimeExp = function (chatTestMode) {
            $('#reverseLookupFinished').attr('value', 'false');
            self.state = self.chatStates.none;
            
            if (chatTestMode == true) {
	            self.timer.stopTimer();
            }
            
            //svcMcaTlb.api.closeCommEvent(constants.events.channelChat, constants.appClass, self.eventUuid, self.inboundData, constants.reason.reject, function(response){
            svcMcaTlb.api.closeCommEvent(constants.events.channelChat, self.appClassfication, self.eventUuid, self.inboundData, constants.reason.reject, function(response){
                console.log("reposnse: "+JSON.stringify(response));
            }, constants.channelType.chatChannelType);
            self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':self.eventUuid, 'phoneLineId':self.phoneLineId}, action: constants.actions.REJECT_ACTION }));            

            if (self.slot !== null) {
                self.slot.hide();
                self.slot.remove();
            }

            self.slot = null;

        };
        self.disconnectChat = function (chatTestMode) {
            self.state = self.chatStates.none;
            
            if (chatTestMode == true) {
	            self.timer.stopTimer();
            }

            //svcMcaTlb.api.closeCommEvent(constants.events.channelChat, constants.appClass, self.eventUuid, self.inboundData, constants.reason.wrapup, function(response){
            svcMcaTlb.api.closeCommEvent(constants.events.channelChat, self.appClassfication, self.eventUuid, self.inboundData, constants.reason.wrapup, function(response){
                console.log("reposnse: "+JSON.stringify(response));
            }, constants.channelType.chatChannelType);

            //svcMcaTlb.api.agentStateEvent(constants.events.channelChat,"1",true,true,"AVAILABLE","Idle",null,"Idle",{"phoneLineId":self.phoneLineId}, function(response) {
         	//   console.log("<RefToolbar> response for Chat agentStateEvent received!!! Reponse " + JSON.stringify(response));
        	//}, constants.channelType.chatChannelType);

            if (self.slot !== null) {
                self.slot.hide();
                self.slot.remove();
            }
            self.slot = null;
            self.subSocket.publish(JSON.stringify({ author: self.author, callData: {'eventId':self.eventUuid, 'phoneLineId':self.phoneLineId}, action: constants.actions.DISCONNECT_ACTION })); 
        };
        
        self.getCallData = function(){

            var callData = {};
            var ivrData = {};
            callData['eventId']=self.eventUuid;
            callData['phoneLineId']=self.phoneLineId;

            for (var token in self.inboundData) {
                if(constants.callDataAttributes[token]){
                	if (token != 'phoneLineId') {
                		callData[token] = self.inboundData[token];
                	}
                }else{
                    ivrData[token] = self.inboundData[token];
                }
            }

            callData[constants.attributes.ivrData] = JSON.stringify(ivrData);

            return callData;
        };
        return {

            setAtmosphereSocket : function(atmosphereSocket){
                self.subSocket =  atmosphereSocket;
                var originalPublish = self.subSocket.publish;

                self.subSocket.publish = function(message){
                    if(!self.restoreState){
                        originalPublish(message);
                    }
                }
            },
            setAuthor : function(author){
                self.author = author;
            },
            createAndShowChatSlot: function(direction, chatTestMode) {

                self.direction = direction;
                self.wrapupType = self.inboundData.wrapupType;
                self.state = self.chatStates.connecting;
                delete self.inboundData.wrapupType;

                //var myUuid = self.eventUuid;
                self.inboundData[constants.attributes.communicationDirection] = direction;
                
                if (chatTestMode == true) {
	                self.slot = $('#toolbar').clone();
	                self.slot.attr("id", self.eventUuid);
	                
	                $('#toolbarControl').attr("id", self.eventUuid);
	                
	                //var pp = self.slot.find('#callerBtnReg2');
	                //pp.attr("id", self.eventUuid);
	                
	                self.slot.find('#accept').click(self.acceptChat);
	                self.slot.find('#reject').click(self.rejectChat);
	                self.slot.find('#end').click(self.disconnectChat);
	                self.slot.appendTo($('#calls'));
	                self.slot.show();
	                self.timer = new chatTimer(self.slot.find('#timer'));
	                self.slot.find('#timer').removeClass('timerOngoing').addClass('timerRing');
	                
	                    self.timer.startTimer();
	                    self.slot.find('.timerBox').show();
	                    self.displayChatData();
	                    self.setIncomingChatMode();
                } else {
                	self.slot = null;
                }
                
                function processChatNewCommEventResponse(response) {
                    console.log("-------- Toolbar Chat newCommEvent response: "+JSON.stringify(response));
                    $('#reverseLookupFinished').attr('value', 'true');
                    for (var token in response.outData) {
                        self.inboundData[token] = response.outData[token];
                    }
                    self.displayChatData();
                    svcMcaTlb.api.postToolbarMessage(JSON.stringify(self.inboundData), function(response) {
                        console.log("======== Response for POST from Chat newCommEvent response after RL - status: "+response.result);
                    });
                }

				if (self.inboundData[constants.attributes.engagementId] != null) {
					self.eventUuid = self.inboundData[constants.attributes.engagementId];
				}                

                //svcMcaTlb.api.newCommEvent(constants.events.channelChat, constants.appClass, self.eventUuid, self.inboundData, null, processChatNewCommEventResponse, constants.channelType.chatChannelType);
                svcMcaTlb.api.newCommEvent(constants.events.channelChat, self.appClassfication, self.eventUuid, self.inboundData, null, processChatNewCommEventResponse, constants.channelType.chatChannelType);

                  
            },
            getInboundData: function() {
                return self.inboundData;
            },
            setInboundData: function(data) {
                self.inboundData = data;
            },
            getAppClassification: function() {
                return self.appClassfication;
            },
            setAppClassification: function(appCode) {
            	self.appClassfication = appCode;
            },            
            getEventId: function() {
                return self.eventUuid;
            },
            setEventId: function(data) {
                self.eventUuid = data;
            },
            setCallState : function(state) {
            	self.state = state;
            },
            getDirection: function() {
                return self.direction;
            },
            setDirection: function(dir) {
                if (dir === constants.direction.inbound_call || dir === constants.direction.outbound_call) {
                    self.direction = dir;
                }
            },
            hideAndCloseChatSlot: function() {
                if (self.slot !== null) {
                    self.slot.hide();
                    self.slot.remove();
                }
                self.slot = null;
            },
            acceptChatAcknowledged : function (chatTestMode) {
                if (self.state != self.chatStates.ongoing) {
                    self.acceptChat(chatTestMode);
                }
            },
            acceptChatTestMode : function() {
            	self.acceptChatTestMode();
            },
            rejectChatAcknowledged: function (chatTestMode) {
                if (self.state != self.chatStates.none) {
                    self.rejectChat(chatTestMode);
                }
            },
            disconnectChatAcknowledged: function (chatTestMode) {
                if (self.state != self.chatStates.none) {
                    self.disconnectChat(chatTestMode);
                }
            },
            notificationTimerExpAcknowledged: function (chatTestMode) {
                if (self.state != self.chatStates.none) {
                	self.rejectChatByTimeExp(chatTestMode);
                }
            },
        };
    }

    return chatInfo;
};

    //============================= START dialpad.js
    function dialpadFactory() {


        function callNumber(){
            var phoneNumber = $('.number').first().text().trim();
            $('.number').text('');
            $('#dialpad').hide();
            var callData = {
                phoneNumber: phoneNumber
            }
            // TODO
            startOutboundCall(callData);
        }
        
        function startOutboundCall(callData) {
        	
        	console.log('>>-- [VERTICAL TOOLBAL] Start New Outbound Phoen Call to : '+ callData.phoneNumber);
        	
        }

        function clearNumbers(){
            $('.number').contents().filter(function(){
                return (this.nodeType == 3);
            }).remove();
        }

        function callNewNumber(){
            var phoneNumber = $('.newCallNumber').first().text().trim();
            $('.newCallNumber').text('');
            $('#newCallContainer').hide();
            var callData = {
                phoneNumber: phoneNumber
            }
            // TODO
            startOutboundCall(callData);
        }

        function getNewCallNumber(){
            var phoneNumber = $('.newCallNumber').first().text().trim();
            return phoneNumber;
        }

        function clearNewCallNumbers(){
            $('.newCallNumber').contents().filter(function(){
                return (this.nodeType == 3);
            }).remove();
        }
        
        function cancelNewCall() {
        	clearNewCallNumbers();
        	$('#newCallContainer').hide();
        }

        return {
            initDialpad: function(){

                var dials = $(".dials ol li");
                var index;
                var number = $(".number");
                var total;

                $('#clearNumbers').click(clearNumbers);
                $('#callButton').click(callNumber);

                dials.click(function(){
                    index = dials.index(this);
                    if(index == 9){
                        number.append("*");
                    }else if(index == 10){
                        number.append("0");
                    }else if(index == 11){
                        number.append("#");
                    }else if(index == 12){
                        clearNumbers();
                    }else if(index == 13){
                        total = number.text();
                        total = total.slice(0,-1);
                        clearNumbers();
                        number.append(total);
                    }else if(index == 14){
                        callNumber();
                    }else{ number.append(index+1); }
                });

                $(document).bind('keydown', function(e){
                    switch(e.keyCode){
                        case 96:
                            number.append("0");
                            break;
                        case 97:
                            number.append("1");
                            break;
                        case 98:
                            number.append("2");
                            break;
                        case 99:
                            number.append("3");
                            break;
                        case 100:
                            number.append("4");
                            break;
                        case 101:
                            number.append("5");
                            break;
                        case 102:
                            number.append("6");
                            break;
                        case 103:
                            number.append("7");
                            break;
                        case 104:
                            number.append("8");
                            break;
                        case 105:
                            number.append("9");
                            break;
                        case 8:
                            total = number.text();
                            total = total.slice(0,-1);
                            clearNumbers();
                            number.append(total);
                            break;
                        case 27:
                            clearNumbers()
                            break;
                        case 106:
                            number.append("*");
                            break;
                        case 35:
                            number.append("#");
                            break;
                        case 13:
                            $('.pad-action').click();
                            break;
                        default: return;
                    }
                    e.preventDefault();
                });
            },
        
        initNewCallDialpad: function(){

            var dials = $(".newCallDials ol li");
            var index;
            var number = $(".newCallNumber");
            var total;

            $('#clearNewCallNumbers').click(clearNewCallNumbers);
            //$('#newCallButton').click(callNewNumber);
            $('#cancelNewCall').click(cancelNewCall);

            dials.click(function(){
                index = dials.index(this);
                if(index == 9){
                    number.append("*");
                }else if(index == 10){
                    number.append("0");
                }else if(index == 11){
                    number.append("#");
                }else if(index == 12){
                	clearNewCallNumbers();
                }else if(index == 13){
                    total = number.text();
                    total = total.slice(0,-1);
                    clearNewCallNumbers();
                    number.append(total);
                }else if(index == 14){
                    callNumber();
                }else{ number.append(index+1); }
            });

            $(document).bind('keydown', function(e){
                switch(e.keyCode){
                    case 96:
                        number.append("0");
                        break;
                    case 97:
                        number.append("1");
                        break;
                    case 98:
                        number.append("2");
                        break;
                    case 99:
                        number.append("3");
                        break;
                    case 100:
                        number.append("4");
                        break;
                    case 101:
                        number.append("5");
                        break;
                    case 102:
                        number.append("6");
                        break;
                    case 103:
                        number.append("7");
                        break;
                    case 104:
                        number.append("8");
                        break;
                    case 105:
                        number.append("9");
                        break;
                    case 8:
                        total = number.text();
                        total = total.slice(0,-1);
                        clearNumbers();
                        number.append(total);
                        break;
                    case 27:
                        clearNumbers()
                        break;
                    case 106:
                        number.append("*");
                        break;
                    case 35:
                        number.append("#");
                        break;
                    case 13:
                        $('.pad-action').click();
                        break;
                    default: return;
                }
                e.preventDefault();
            });
        },
        
        getNewCallNumber: function() {
        	var callNum = getNewCallNumber();
        	return callNum;
        },
        
        clearNewCallNumbers: function() {
        	clearNewCallNumbers();
        }

        };
    }
    //============================= END dialpad.js
    //============================= START vToolbar.js
    function vToolbarFactory( liveConn, constants, dialpad, svcMcaTlb, lineId) {

        var author, json, eventId;

        var PhoneLineId = lineId;
        	
        var newCallDialpadeInited = false;
        
        function getParameterByName(name) {
            var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
            return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
        }

        function toolbarMessageCallback(response) {
            console.log('>>-- [VERTICAL TOOLBAR] Inter toolbar communication message received: '+response.messagePayload);
            var msgPayload = JSON.parse(response.messagePayload);
            if (msgPayload.msgCommand && msgPayload.msgCommand === 'AGENT_AVAIL') {
            	// HZH 20171020 Remove Vtoolbar config
                //configureAgentInfo();
                initTabs();
                dialpad.initDialpad();
                //dialpad.initNewCallDialpad();
            }
        }
        
        function openVToolbar(thisEventId) {
        	
        	eventId = thisEventId;
        	
            console.log('>>-- [VERTICAL TOOLBAR] Open Vertical Toolbar');
            initTabs();
            dialpad.initDialpad();
            $('#VToolbar').show();
        }

        function closeVToolbar() {
            console.log('>>-- [VERTICAL TOOLBAR] Close Vertical Toolbar');
            clearAgents();
            $('#VToolbar').hide();
        }

        function inviteToConference(){
            var transferData ={};
            var callData = {};

            var agentId = $(this).parent().attr('id');
            callData['eventId']=eventId;
            callData['phoneLineId']=PhoneLineId;
            transferData['agentId']=agentId;  	

            console.log("AgentID: "+agentId);

            liveConn.publish(JSON.stringify({ author: author, data: JSON.stringify(transferData), callData: callData, action: constants.actions.INVITE_CONFERENCE_ACTION }));

            $(this).val('End Conference');
            $(this).off().click(closeConference);

            console.log("Closing Floating");
            closeVToolbar();
            //svcMcaTlb.api.closeFloatingToolbar("FLOAT_VERTICAL", function(response){
            //    console.log(response);
            //});  
        }

        function closeConference(){
            var agentId = $(this).parent().attr('id');
            console.log("AgentID: "+agentId);
            var transferData ={};
            transferData['agentId']=agentId; 

            liveConn.publish(JSON.stringify({ author: author, data: JSON.stringify(transferData), action: constants.actions.CLOSE_CONFERENCE_ACTION }));

            $(this).val('Conference');
            $(this).off().click(inviteToConference);
            console.log("Closing Floating");
            closeVToolbar();
            //svcMcaTlb.api.closeFloatingToolbar("FLOAT_VERTICAL", function(response){
            //    console.log(response);
            //});  
        }

        function consultWith(){
            var agentId = $(this).parent().attr('id');
            var transferData ={};
            var callData = {};

            callData['eventId']=eventId;
            callData['phoneLineId']=PhoneLineId;
            transferData['agentId']=agentId;

            liveConn.publish(JSON.stringify({ author: author, data: JSON.stringify(transferData), callData: callData, action: constants.actions.INVITE_CONSULT_ACTION }));
            console.log("Closing Floating");
            closeVToolbar();
            //svcMcaTlb.api.closeFloatingToolbar("FLOAT_VERTICAL", function(response){
            //    console.log(response);
            //});  

        }

        function endConsult(){
            var agentId = $(this).parent().attr('id');
            var callData = {};
            var transferData ={};

            callData['eventId']=eventId;
            callData['phoneLineId']=PhoneLineId;
            transferData['agentId']=agentId;   	    	

            liveConn.publish(JSON.stringify({ author: author, data: JSON.stringify(transferData), action: constants.actions.END_CONSULT_ACTION }));

            $(this).val('Consult');
            $(this).off().click(consultWith);

            console.log("Closing Floating");
            closeVToolbar();
            //svcMcaTlb.api.closeFloatingToolbar("FLOAT_VERTICAL", function(response){
            //    console.log(response);
            //});  

        }


        function transferTo(){

            var agentId = $(this).parent().attr('id');

            var transferData ={};
            var callData = {};
            callData['eventId']=eventId;
            callData['phoneLineId']=PhoneLineId;
            transferData['agentId']=agentId;
            console.log("transferTo: "+agentId)
            if(agentId){

                liveConn.publish(JSON.stringify({ author: author, data: JSON.stringify(transferData), callData: callData, action: constants.actions.TRANSFER_TO_ACTION }));
                clearAgents();
                console.log("Closing Floating");
                closeVToolbar();
                //svcMcaTlb.api.closeFloatingToolbar(function(response){
                //    console.log(response);
                //});  
            }
        }

        function clearAgents(){
            $('#agentsTabVT').children().each(function(){

                try{
                    var cssClasses = $(this).attr('class');

                    if(cssClasses.indexOf('callSlotContainer') != -1){
                        $(this).remove();
                    }
                }catch(err){}
            });

            $('#agentActionButton').hide();
        }

        function transferAction(json){
            if(json.data){

            	console.log("transferAction: "+eventId);
                clearAgents();

                var agentData = JSON.parse(json.data);
                var availableAgents = JSON.parse(agentData.availableAgents);
                
                for(var key in availableAgents){
                    var agentName = availableAgents[key];
                    var avaialbleAgent = $('#transferAgent').clone();
                    $(avaialbleAgent).attr('id',agentName);
                    $(avaialbleAgent).find('#primaryInfo').text(agentName);
                    $(avaialbleAgent).find('#transferButton').click(transferTo);
                    $(avaialbleAgent).find('#consultButton').click(consultWith);
                    $(avaialbleAgent).find('#conferenceButton').hide();


                    /*$(avaialbleAgent).hover(
                          function() {
                              $(this).css('background-color','#f2f2f2');
                              $(this).find('#consultButton').show();
                              $(this).find('#transferButton').show();

                          }, function() {
                              $(this).removeAttr('style');
                              $(this).find('#consultButton').hide();
                              $(this).find('#transferButton').hide();


                          }
                    );*/
                    
                    $(avaialbleAgent).css('background-color','#f2f2f2');
                    $(avaialbleAgent).find('#consultButton').show();
                    $(avaialbleAgent).find('#transferButton').show();

                    avaialbleAgent.insertAfter('#startAgentList');
                }

                $('#tabs-container').show();
            }
        }

        function conferenceAction(json){
            if(json.data){

                clearAgents();

                var agentData = JSON.parse(json.data);

                var availableAgents = JSON.parse(agentData.availableAgents);
                var conferenceAgents = JSON.parse(agentData.conferenceAgents);

                for(var key in availableAgents){
                    var agentName = availableAgents[key];
                                    console.log("available agent: "+agentName)
                    var avaialbleAgent = $('#transferAgent').clone();
                    $(avaialbleAgent).attr('id',agentName);
                    $(avaialbleAgent).find('#primaryInfo').text(agentName);


                    $(avaialbleAgent).find('#consultButton').hide();
                    $(avaialbleAgent).find('#conferenceButton').click(inviteToConference);
                    $(avaialbleAgent).find('#transferButton').click(transferTo);

                    /*$(avaialbleAgent).hover(
                              function() {
                                  $(this).css('background-color','#f2f2f2');
                                  $(this).find('#conferenceButton').show();
                                  $(this).find('#transferButton').show();
                              }, function() {
                                  $(this).removeAttr('style');
                                  $(this).find('#conferenceButton').hide();
                                  $(this).find('#transferButton').hide();
                              }
                        ); */

                    $(avaialbleAgent).css('background-color','#f2f2f2');
                    $(avaialbleAgent).find('#consultButton').show();
                    $(avaialbleAgent).find('#transferButton').show();

                    avaialbleAgent.insertAfter('#startAgentList');
                }


                for(var key in conferenceAgents){
                    var agentName = conferenceAgents[key];
                    console.log("conference agent: "+agentName)
                    var conferenceAgent = $('#'+agentName);

                    conferenceAgent.off();
                    conferenceAgent.remove();
                    conferenceAgent.insertAfter('#startAgentList');

                    var conferenceButton = $('#'+agentName).find('#conferenceButton');
                    conferenceButton.val("End Conference");
                    conferenceButton.off().click(closeConference);
                    conferenceButton.show();
                    $('#'+agentName).find('#transferButton').show();
                    $('#'+agentName).find('#transferButton').click(transferTo);
                }

                $('#tabs-container').show();


            }
        }


        function handleMessage(json) {
        	
        	if (!author) {
        		author = json.author;	
        	}
        	
            switch(json.action){
                case constants.actions.LOGIN_ACTION:
                    if(json.author){
                        console.log("Logged in...");
                        author = json.author;
                        if (author != "") {
                        	console.log("Logged in User (2) is set to " + author);
                        	$('.loggedInUser').html(json.author);
                        	$('#outboundVT').show();
                        	//$('#CompanionControl').show();
                        }
                        
                        requestAgentsList();
                        
                        // HZH
                        //initTabs();
                        //dialpad.initDialpad();
 
                    }
                    break;
                case constants.actions.END_ACTION:
                    clearAgents();
                    closeVToolbar();
                    break;
                case constants.actions.END_TRANSFER_ACTION:
                    //clearAgents();
                    break;

                case constants.actions.TRANSFER_ACTION:
                	var thisEventId = json.callData.eventId;
                	openVToolbar(thisEventId);
                    transferAction(json);
                    break;
                case constants.actions.TRANSFER_TO_ACTION:
                    svcMcaTlb.api.postToolbarMessage(JSON.stringify({"messageType" : constants.messageType.END_CALL}), function(response) {
                        console.log("======== ########## vToolbar Message POST status: "+response.result);
                    });
                break;

                case constants.actions.CONFERENCE_ACTION:
                	closeVToolbar();
                	conferenceAction(json);
                break;

                case constants.actions.END_CONSULT_ACTION:
                case constants.actions.END_CONFERENCE_ACTION:
                    clearAgents();
                    break;
                default:
                    break;
            }
        }


        function atmoSubscribeCallback(uniqueUserName) {
            liveConn.publish(JSON.stringify({ author: "alan.hooper_204", message: "alan.hooper_204", data: "{'source': 'vToolbar'}", action: constants.actions.LOGIN_ACTION }));
        }

        function configureAgentInfo() {
            svcMcaTlb.api.getConfiguration("TOOLBAR", function(response) { 
                if (response.configuration) {
                    var agentId = response.configuration.agentId;
                    
                    var comPanelUrl = response.configuration.companionPanelUrl;
                    var comPanelTitle = response.configuration.companionPanelTitle;
                    //var features = response.configuration.features;
                    var features = null;
                    if (response.configuration.features) {
                    	features = JSON.parse(response.configuration.features);
                    }

                    console.log("getConfiguration agentId: " + agentId + " Companion Panel URL: " + comPanelUrl + " Title: "+ comPanelTitle);
                    console.log("getConfiguration features: " + features);

                    setCompanionPanelUrl(comPanelUrl);
                    setCompanionPanelTitle(comPanelTitle); 
                    
                    if (features) { 
                    	for (var i=0; i<features.length; i++) {
                    	    var feature = features[i]; 
                    		console.log("getConfiguration feature: ["+i+"]:" + feature +";");
                    	}
                    }
                    
                   
                 }
            });
        };

        function retrieveUniqueAgentId(username) {

            var uuidReqData = {
                    type: 'GET',
                    dataType: 'json',
                    async: true,
                    url : liveConn.getUrl()+'services/agent/id?userAgent='+username,
                    data: {},
                    error: function( jqXHR, textStatus, errorThrown ) {
                        console.log("vToolbar Get Agent ID failed with status: "+jqXHR.status+" - message: "+textStatus);
                        console.log("Fallback too locally generated ");

                        var unqUsername = username+"_"+(new Date()).getTime();
                        liveConn.connect(unqUsername, function(){
                            atmoSubscribeCallback(unqUsername);
                        }, handleMessage);
                    },
                    success : function ( data, textStatus, jqXHR ) {
                        console.log("------------------------------------------vToolbar Retrieved: "+data.agentId);
                        var unqUsername = data.agentId;
                        liveConn.connect(unqUsername, function(){
                            atmoSubscribeCallback(unqUsername);
                        }, handleMessage);
                    }
            };
            $.ajax(uuidReqData);
        };


        function checkAgentLoggedin() {
            var uuidReqData = {
                type: 'GET',
                dataType: 'json',
                async: true,
                url : liveConn.getUrl()+'services/agent/check',
                data: {},
                error: function( jqXHR, textStatus, errorThrown ) {
                    console.log("vToolbar Check agent loggedin failed with status: "+jqXHR.status+" - message: "+textStatus);
                },
                success : function ( data, textStatus, jqXHR ) {
                    console.log("vToolbar Retrieved: "+data.agentId);
                    if (data.agentId && data.agentId !== "") {

                        var unqUsername = data.agentId;
                        liveConn.connect(unqUsername, function(){
                                atmoSubscribeCallback(unqUsername);
                            }, handleMessage);
                        initTabs();
                        dialpad.initDialpad();
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
                    console.log("vToolbar remove agent loggedin failed with status: "+jqXHR.status+" - message: "+textStatus);
                },
                success : function ( data, textStatus, jqXHR ) {
                    console.log("vToolbar Retrieved: "+data.agentId);
                    //if (data.agentId && data.agentId !== "") {

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

        function initTabs(){

            $(".tabs-menuVT a").click(function(event) {
                event.preventDefault();
                $(this).parent().addClass("current");
                $(this).parent().siblings().removeClass("current");
                var tab = $(this).attr("href");
                $(".tab-contentVT").not(tab).css("display", "none");
                $(tab).fadeIn();
            });

            $('#activateDialpad').click(function(){
                var dialpadDiv = $('#dialpad').clone();
                $('#dialpad').remove();
                dialpadDiv.insertAfter('#dialpadTab');
                dialpad.initDialpad();
                $('#dialpad').show();

            });

            $('#activateAgents').click(function(){
                $('#dialpad').hide();
            });

            $('#closeVToolbar').click(function(){
            	closeVToolbar();
            });

        }

        function requestAgentsList(){
            var showAgents = getParameterByName('showAgents');
            eventId = getParameterByName('eventId');
            var conference = getParameterByName('conference');
            var data = conference == 'true' ? { 'action' : constants.actions.CONFERENCE_ACTION} : { 'action' : constants.actions.TRANSFER_ACTION};

            if(showAgents == 'true' && eventId){
                liveConn.publish(JSON.stringify({ author: author, message: "", data: JSON.stringify(data), action: constants.actions.REQUEST_TRANSFER_AGENTS_ACTION }));
            }

        }
        
        function getNewCallNumber() {
        	var phoneNumber = dialpad.getNewCallNumber();
        	return phoneNumber;
        }
        
        function startNewCall() {
        	//$('#newCallContainer').show();
        	//$('#newCallButton').click(makeNewCall);
        	
        	if (!newCallDialpadeInited) {
        		dialpad.initNewCallDialpad();
        		newCallDialpadeInited = true;
        	}
        }
        
        function cancelNewCall() {
        	dialpad.clearNewCallNumbers();
         	//$('#newCallContainer').hide();
        }
        
        return{
            init: function() {
                svcMcaTlb.api.onToolbarMessage(toolbarMessageCallback);
                // HZH Set Init User status
                $('.loggedInUser').html('(Not Signed in)');
                //checkAgentLoggedin();
                
            },
//            initVToolbar : function(){
//                configureAgentInfo();
//                initTabs();
//                dialpad.initDialpad();
//            },
            
            handleMessage: function (json) {
            	handleMessage(json);
            },
 
            startNewCall: function() {
            	startNewCall();
            },

            cancelNewCall: function() {
            	cancelNewCall();
            },

            getNewCallNumber: function () {
            	var phoneNumber = getNewCallNumber();
            	return phoneNumber;
            },

            searchAgent : function(sourceElem, containerIdToSearch){
            	
            	//var evt = event | window.event;
            	//var KeyID = event.which | event.keyCode | event.charCode; 
            	            	
            	//console.log("******Keyup*** KeyID: " +  KeyID);
            	
            	//if (KeyID == 8) {
            	//	return true;
            	//} else if (KeyID == 46) {
            	//	return true;
            	//}

                var stringToSearch = $(sourceElem).val();
                var ownId = $(sourceElem).attr('id');
                $('#'+containerIdToSearch).children().show();

                $('#'+containerIdToSearch).children().each(function( index ) {

                    var elementId = $(this).attr('id');
                    //if(!elementId.includes(stringToSearch) && elementId != ownId){
                    if((elementId.indexOf(stringToSearch) < 0) && elementId != ownId){
                        $(this).hide();
                    }
                });
                
                //return true;
            }
        };

    }    
    //============================= END vToolbar.js

    //============================= START agentInfo.js
    function agentInfoFactory(liveConn, callInfo, chatInfo, consultSlot, constants, svcMcaTlb) { 
        var author, json;
 
        function agentInfo() {
            var self = this;
            self.jwt = null;
            self.loggedIn = false;
            self.subSocket = liveConn;
            var dialpad = dialpadFactory();
            self.dialpad = dialpad;
            self.vToolbar = new vToolbarFactory(liveConn, constants, dialpad, svcMcaTlb, "1");
            self.vToolbar2 = new vToolbarFactory(liveConn, constants, dialpad, svcMcaTlb, "2");
            self.call = new callInfo("1");
            self.call.setAtmosphereSocket(self.subSocket);
            self.call2 = new callInfo("2");
            self.call2.setAtmosphereSocket(self.subSocket);
            
            //var chatInfo = chatInfoFactory(constants, svcMcaTlb);
            
            self.chat = new chatInfo("1");
            self.chat.setAtmosphereSocket(self.subSocket);
            self.chat2 = new chatInfo("2");
            self.chat2.setAtmosphereSocket(self.subSocket);
            
            self.consultSlot = new consultSlot("1");
            self.consultSlot.setAtmosphereSocket(self.subSocket);
            self.consultSlot2 = new consultSlot("2");
            self.consultSlot2.setAtmosphereSocket(self.subSocket);
            self.agentStatus = 'OFF';
            self.agentId = null;
            self.agentUniqueId = null;
            self.agentBaseCallCount = -1;
            self.agentCallCount = 0;
            self.agentCurrentCallCount = 0;
            self.agentWaitTimeMedium = 60;
            self.agentWaitTimeMargin = 60;
            self.companionPanelShow = false;
            self.companionPanelUrl = null;
            self.companionPanelTitle = null;
            
            self.inbound_chat_feature = false;
            self.nrt_work_assign_feature = false;
 
            self.Notification = window.Notification || window.mozNotification || window.webkitNotification;
            if (self.Notification){
                self.Notification.requestPermission(function (permission){ 
                    console.log(permission); 
                });
            }

            function pingAdmin() {
            	if (self.loggedIn) {
            		self.subSocket.publish(JSON.stringify({ author: author, message: '', action: constants.actions.PING_ADMIN_ACTION }));
            		nextPing();
            	}
            }
            
            function nextPing() {
            	var t = setTimeout(pingAdmin, 180000);
            }
            
            function enableAgentInboundChatFeature() {
            	self.inbound_chat_feature = true;
            }
            
            function disableAgentInboundChatFeature() {
            	self.inbound_chat_feature = false;
            }

            function enableAgentNRTFeature() {
            	self.nrt_work_assign_feature = true;
            }
            
            function disableAgentNRTFeature() {
            	self.nrt_work_assign_feature = false;
            }

            function getAgentStatus() {
            	return self.agentStatus;
            	//return 'OFF';
            }
            
            function setAgentStatus(status) {
            	self.agentStatus = status;
            }
 
            function getCompanionPanelUrl() {
              return self.companionPanelUrl;	
            }
            
            function setCompanionPanelUrl(url) {
            	self.companionPanelUrl = url;
            }
 
            function getCompanionPanelTitle() {
                return self.companionPanelTitle;	
              }
              
            function setCompanionPanelTitle(title) {
              	self.companionPanelTitle = title;
             }

            function getAgentCurrentCallCount() {
            	self.agentCurrentCallCount = getAgentBaseCallCount() + self.agentCallCount;
            	return self.agentCurrentCallCount;

            }

            function getAgentBaseCallCount() {
            	if (self.agentBaseCallCount <= 0) {
            		// if agentBaseCallCount is not set yet, generate a random number between 8 an 14
            		self.agentBaseCallCount = Math.floor(Math.random() * 6) + 8;
            	}
            	
            	return self.agentBaseCallCount;
            }

            function setAgentBaseCallCount(baseCallCount) {
            	self.agentBaseCallCount = baseCallCount;
            }

            function getAgentCallCount() {
            	return self.agentCallCount;
            }

            function setAgentCallCount(agentCallCount) {
            	self.agentCallCount = agentCallCount;
            }
            
            function increaseAgentCallCount() {
            	self.agentCallCount++;
            }


            function getAgentWaitTimeMedium() {
            	return self.agentWaitTimeMedium;
            }

            function setAgentWaitTimeMedium(agentWaitTimeMedium) {
            	self.agentWaitTimeMedium = agentWaitTimeMedium;
            }

            function getAgentWaitTimeMargin() {
            	return self.agentWaitTimeMargin;
            }

            function setAgentWaitTimeMargin(agentWaitTimeMargin) {
            	self.agentWaitTimeMargin = agentWaitTimeMargin;
            }

            
            function agentStateEventChangeCallBack(response) {
            	console.log("<RefToolbar> response for agentStateEvent received!!! Reponse " + JSON.stringify(response));
            }
            
            function logoutAgent() {

                self.subSocket.publish(JSON.stringify({ author: author, message: '', action: constants.actions.LOGOUT_ACTION }));
                self.agentStatus = 'OFF';
            }
            
            function openCompanionPanel1() {
            	//var url = location.origin + "/toolbar/liveWinPhoneCompanion.html"
            	//var title = $("#companionPanelTitle").val();
            	
            	var url = getCompanionPanelUrl();
            	var title = getCompanionPanelTitle();
            	
            	console.log("Default Companion Panel Url is:  " + url);
            	
            	if (url == null) {
            		url = location.origin + "/toolbar/liveWinPhoneCompanion.html";
            		title = $("#companionPanelTitle").val();
            		
            		if (title == null) {
            			title = "Companion Panel"
            		} 
            	}
            	
            	console.log("Actual Companion Panel Url is:  " + url);

            	if (url) {
	            	svcMcaTlb.api.openCompanionPanel("RHS", url, title, {}, function(response) {
	            		console.log("Open Companion Panel 1, Reponse " + JSON.stringify(response));
	            	});
            	}            
            	//svcMcaTlb.api.openCompanionPanel("RHS", "http://10.146.111.55:8080/toolbar/liveWinPhoneCompanion.html", "Companion Panel 1", {}, function(response) {
            	//	console.log("Open Companion Panel 1, Reponse " + JSON.stringify(response));
            	//});
            	//autoReconnect();
            }
            
            function openCompanionPanel2() {

            	var url = location.origin + "/toolbar/liveWinCompanionPanel2.html"
            	var title = $("#companionPanelTitle").val();
  
            	
            	svcMcaTlb.api.openCompanionPanel("RHS", url, title, {}, function(response) {
            		console.log("Open Companion Panel 2, Reponse " + JSON.stringify(response));
            	});
            	
            	//svcMcaTlb.api.openCompanionPanel("RHS", "http://10.146.111.55:8080/toolbar/liveWinCompanionPanel2.html", "Companion Panel 2", {}, function(response) {
            	//	console.log("Open Companion Panel 2, Reponse " + JSON.stringify(response));
            	//});  
            	//autoReconnect();
            }

            function sendMessageToCompanionPanel(msg) {

                var callData={};
            	callData[constants.attributes.agentMsg2ComPanel] = msg;
            	var companionMsg = {};
            	companionMsg['callData'] = callData;
            	companionMsg['msgCommand'] = constants.messageType.UPDATE_COM_PANEL_MSG;
            	
            	//self.subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "UPDATE_COM_PANEL" }));

            	//svcMcaTlb.api.postToolbarMessage(JSON.stringify("messageType" : constants.messageType.UPDATE_COM_PANEL_MSG; callData : callData), function(response) {
                //      console.log("======== Response from Post Companion Panel Message - status: "+response.result);
                //});

            	svcMcaTlb.api.postToolbarMessage(JSON.stringify(companionMsg), function(response) {
                    console.log("======== Response from Post Companion Panel Message - status: "+response.result);
              });

            }

            function updateCompanionPanel(callData) {

            	var companionMsg = {};
            	companionMsg['callData'] = callData;
            	companionMsg['msgCommand'] = constants.messageType.UPDATE_COM_PANEL_MSG;
            	
            	//self.subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "UPDATE_COM_PANEL" }));

            	//svcMcaTlb.api.postToolbarMessage(JSON.stringify("messageType" : constants.messageType.UPDATE_COM_PANEL_MSG; callData : callData), function(response) {
                //      console.log("======== Response from Post Companion Panel Message - status: "+response.result);
                //});

            	svcMcaTlb.api.postToolbarMessage(JSON.stringify(companionMsg), function(response) {
                    console.log("======== Response from Post Companion Panel Message - status: "+response.result);
              });

            }

            
            function openCompanionPanelUrl(url) {
            	
            	var title = $("#companionPanelTitle").val();
            	
            	svcMcaTlb.api.openCompanionPanel("RHS", url, title, {}, function(response) {
            		console.log("Open Companion Panel URL, Reponse " + JSON.stringify(response));
            	});   
            	//autoReconnect();
            }

            function closeCompanionPanel() {
            	svcMcaTlb.api.closeCompanionPanel("RHS", function(response) {
            		console.log("Close Companion Panel, Reponse " + JSON.stringify(response));
            	});
            	//autoReconnect();            	
            }

            function S4() {
            	return (((1+Math.random())*0x10000)|0).toString(16).substring(1); 
            }
            
            function getGUID() {
            	var guid = (S4() + S4() + "-" + S4() + "-4" + S4().substr(0,3) + "-" + S4() + "-" + S4() + S4() + S4()).toLowerCase();
            	return guid;
            }
            
            function makeNewCall() {
            	
            	var phoneNumber = self.vToolbar.getNewCallNumber();
            	
            	var outData = {};
            	outData["SVCMCA_ANI"] = phoneNumber;
            	//outData["method"] = "onOutboundEvent";
            	//outData["channel"] = "PHONE";
            	//outData["channelType"] = "ORA_SVC_PHONE";
            	outData["SVCMCA_COMMUNICATION_DIRECTION"] = "ORA_SVC_OUTBOUND";
            	outData['phoneLineId']="1";
 
            	var callData = {};
            	callData["SVCMCA_ANI"] = phoneNumber;
            	callData["SVCMCA_COMMUNICATION_DIRECTION"] = "ORA_SVC_OUTBOUND";
            	callData['phoneLineId']="1";

            	var uuid = getGUID(); 
            	
            	//svcMcaTlb.api.postOutboundEvent(channel, "ORA_SERVICE", outData, function(response) {
            	//	console.log("======== Making Outbound Call returns "+response.result);
              //
            	//}, constants.channelType.phoneChannelType);

            	// Outbound Call always on line 1
            	self.call.setInboundData(outData);
            	self.call.setAppClassification('ORA_SERVICE');
            	self.call.setEventId(uuid);
                self.call.createAndShowCallSlot(constants.direction.outbound_call);
                self.subSocket.publish(JSON.stringify({ author: author, 
                                                        message: '{"eventId":"'+uuid+'"}', 
                                                        action: constants.actions.OUTBOUND_CALL_ACTION,
                                                        data: JSON.stringify(outData)}));

                
/*                svcMcaTlb.api.onOutgoingEvent(JSON.stringify({channel : channel, channelType: channelType, outData:JSON.stringify(outData)}), function(response) {
                    console.log("======== Making Outbound Call returns "+response.result);
                    
                    self.call.setInboundData(response.outData);                        
                    self.call.setEventId(response.uuid);
                    self.call.createAndShowCallSlot(constants.direction.outbound_call);
                    self.subSocket.publish(JSON.stringify({ author: author, 
                                                            message: '{"eventId":"'+response.uuid+'"}', 
                                                            action: constants.actions.OUTBOUND_CALL_ACTION,
                                                            data: JSON.stringify(response.outData)}));
                  
                });
*/
                cancelNewCall();
            	
            }
            
            function startNewCall() {
            	self.vToolbar.startNewCall();
            	//createAndShowCallSlot(); 
            	$('#newCallContainer').show();
            	//$('#newCallButton').click(makeNewCall);
            }

            function cancelNewCall() {
            	self.vToolbar.cancelNewCall(); 
            	$('#newCallContainer').hide();
            }

            function clearCompanionControlInputs() {
            	$('#companionPanelMessage').html("");
            	$('#companionPanelUrl').html("");
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

            function showToastNotification(title, body){
                if (self.Notification) {
                    var instance = new self.Notification(title, { body: body });
                }
            }

            function handleAgentStatusChange() {
            	
            	if (getAgentStatus() == 'OFF') {
    	            agentLogin();
    	            userLoggedInInfo();
            	} else if (getAgentStatus() == 'LOGGED_IN') {
            	    setAgentOnBreak();
            	} else if (getAgentStatus() == 'ON_BREAK') {
            		setAgentOffBreak();	
            	}
    	     };

            function userLoggedInInfo() {
                $('.userSlot').removeClass('agentOff').addClass('agentOn');
                $('.agentIcon').removeClass('agentIconOff').addClass('agentIconOn');
                $('#availableBtn').removeClass('unavailBtn').addClass('availBtn');
                $('#availableBtn').attr('title', 'Available');
                setAgentStatus('LOGGED_IN');
                //$('#').off("click");
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

            function isCustomerDataPresent(data){
                if(data){
                    if(data.SVCMCA_SR_NUM || data.SVCMCA_SR_TITLE || data.SVCMCA_DISPLAY_NAME || data.SVCMCA_CONTACT_NAME || data.SVCMCA_ORG_NAME) {
                        return true;
                    }
                }
                return false;
            }

            function setAgentOnBreak(){
                $('.userSlot').removeClass('agentOn').addClass('agentBreak');
                $('.agentIcon').removeClass('agentIconOn').addClass('agentIconTempBreak');
                $('#availableBtn').removeClass('availBtn').addClass('onBreakBtn');
                $('#availableBtn').attr('title', 'OnBreak');
                svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",false,true,"UNAVAILABLE","On Break. Lunch Break",null,"On Break",{}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);

                if (self.inbound_chat_feature == true) {
                	svcMcaTlb.api.agentStateEvent(constants.events.channelChat,"1",false,true,"UNAVAILABLE","On Break. Lunch Break",null,"On Break",{}, agentStateEventChangeCallBack, constants.channelType.chatChannelType);
                }

                if (self.nrt_work_assign_feature == true) {
                	svcMcaTlb.api.agentStateEvent(constants.events.channelNRT,"1",false,true,"UNAVAILABLE","On Break. Lunch Break",null,"On Break",{}, agentStateEventChangeCallBack, constants.channelType.noneChannelType);
                }

                self.agentStatus = 'ON_BREAK';
            }

            function setAgentOffBreak(){
                $('.userSlot').removeClass('agentBreak').addClass('agentOn');
                $('.agentIcon').removeClass('agentIconTempBreak').addClass('agentIconOn');
                $('#availableBtn').removeClass('onBreakBtn').addClass('availBtn');
                $('#availableBtn').attr('title', 'Available');
                svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",true,true,"AVAILABLE","Idle",null,"Idle",{}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);

                if (self.inbound_chat_feature == true) {
                	svcMcaTlb.api.agentStateEvent(constants.events.channelChat,"1",true,true,"AVAILABLE","Idle",null,"Idle",{}, agentStateEventChangeCallBack, constants.channelType.chatChannelType);
                }

                if (self.nrt_work_assign_feature == true) {
                	svcMcaTlb.api.agentStateEvent(constants.events.channelNRT,"1",true,true,"AVAILABLE","Idle",null,"Idle",{}, agentStateEventChangeCallBack, constants.channelType.noneChannelType);
                }

                self.agentStatus = 'LOGGED_IN';
            }

            function endConference(){
                self.subSocket.publish(JSON.stringify({ author: author, message: json.message, action: constants.actions.END_CONFERENCE_ACTION }));
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

            function handleSecondCallSlot(json, lineId){
                if(json.data){
                     var data = JSON.parse(json.data);

                     if (lineId == "2") {
	                     self.consultSlot2.updateSecondSlotData(json.action, data.agentId);
                     } else {
						 self.consultSlot.updateSecondSlotData(json.action, data.agentId);
            		 }
                }
            }

            function handleMessage(json) {

                console.log("AgentInfo HandleMessage(), action type " + json.action);
                console.log("AgentInfo HandleMessage(), json data " + JSON.stringify(json.action));;
                
                var action = json.action;
                var channelType = null;
                var inDataChannelType = null;
                var startTime = json.startTime;
                var adjustedStartTime = null;
                var serverCurrentTime = json.serverCurrentTime;

            	var currentTime = Date.now();                	

            	// record startTime for restoring call timers.
                if (startTime) {
                    console.log("AgentInfo HandleMessage(), action: "+ action +" startTime is " + startTime + " serverCurrentTime is " + serverCurrentTime + "; client currentTime is " + currentTime + "."); 
                    adjustedStartTime = startTime + currentTime - serverCurrentTime;
                    console.log("AgentInfo HandleMessage(), action: "+ action +" adjsutedStartTime is " + adjustedStartTime + "."); 
                    
                } else {
                	startTime = null;
                    console.log("AgentInfo HandleMessage(), action: "+ action +" startTime is null,  currentTime is " +currentTime + "."); 
                }
                	
                if (json.callData) {
                	channelType = json.callData.channelType;
                	
                	if (json.callData.ivrData) {
                		inDataChannelType = json.callData.ivrData.channelType;
                	}
                }
                
                if (action === constants.actions.CHAT_ACTION || channelType === constants.channelType.chatChannelType || 
                		inDataChannelType === constants.channelType.chatChannelType) {
	                // HZH Check message type 
	                handleChatMessage(json)
	                return;
                }
                
                var currentCall = null;
                var currentConsultSlot = null
                var currentVToolbar = null
                var phoneLineIdString = null;
                
                // use phone line 2 only when specified
                if (json.callData) {
                	if (json.callData.phoneLineId) {
                		if (json.callData.phoneLineId == "2") {
                			currentCall = self.call2;
                			currentConsultSlot = self.consultSlot2;
                			currentVToolbar = self.vToolbar2;
                			phoneLineIdString = "2";
                		} else {
                			currentCall = self.call;
                			currentConsultSlot = self.consultSlot;
                			currentVToolbar = self.vToolbar;
                			phoneLineIdString = "1";
                		}
                	} else {
                		
                		currentCall = self.call;
                		currentConsultSlot = self.consultSlot;
                		currentVToolbar = self.vToolbar;
                		phoneLineIdString = '1';
                	}
                }

                switch(json.action){
                    case constants.actions.LOGOUT_ACTION:
                        //window.location.reload();                        
                        svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",false,false,"DISCONNECTED",null,null,"Logged Out",{}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);

                        if (self.inbound_chat_feature == true) {
                        	svcMcaTlb.api.agentStateEvent(constants.events.channelChat,"1",false,false,"DISCONNECTED",null,null,"Logged Out",{}, agentStateEventChangeCallBack, constants.channelType.chatChannelType);
                        }

                        if (self.nrt_work_assign_feature == true) {
                        	svcMcaTlb.api.agentStateEvent(constants.events.channelNRT,"1",false,false,"DISCONNECTED",null,null,"Logged Out",{}, agentStateEventChangeCallBack, constants.channelType.noneChannelType);
                        }

                        break;	
                    case constants.actions.PING_ADMIN_ACTION:
                    	if(json.author){
                    		console.log("Agent Ping Admin from: " + json.author);
                    	}
                    	break;
                    case constants.actions.PING_REPLY_ACTION:
                    	if(json.author){
                    		console.log("Admin reply Agent Ping from: " + json.author);
                    	}
                        break;
                    case constants.actions.LOGIN_ACTION:
                        if(json.author){
                            console.log("Login response action received! json value is " + json);
                            if (!self.loggedIn) {
                                console.log("Already logged in!");
                                self.loggedIn = true;
                                nextPing();
                                author = json.author;
                                console.log("User Author 1 value is " + author);
                                $('#input').val('');
                                if (json.author !== null && json.author != "") {
                                	console.log("Logged in User (1) is set to " + json.author);
                                	$('.loggedInUser').html(''+json.author);
                                    $('#outboundVT').show();
                                
                                    self.subSocket.publish(JSON.stringify({ author: author, message: author, action: constants.actions.GET_STATUS_ACTION }));
                                }
                            } else {
                            	nextPing();
                            }
                            
                            self.activeEngagements = json.data
                            
                            svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",true,true,"AVAILABLE","Idle",null,"Idle",{"phoneLineId":"1"}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);
                            svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",true,true,"AVAILABLE","Idle",null,"Idle",{"phoneLineId":"2"}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);

                            if (self.inbound_chat_feature == true) {
                            	svcMcaTlb.api.agentStateEvent(constants.events.channelChat,"1",true,true,"AVAILABLE","Idle",null,"Idle",{}, agentStateEventChangeCallBack, constants.channelType.chatChannelType);
                            }

                            if (self.nrt_work_assign_feature == true) {
                            	svcMcaTlb.api.agentStateEvent(constants.events.channelNRT,"1",true,true,"AVAILABLE","Idle",null,"Idle",{}, agentStateEventChangeCallBack, constants.channelType.noneChannelType);
                            }

                        	openCompanionPanel1();

                            var currentCall = getAgentCurrentCallCount();
                            var callData={};
                        	callData[constants.attributes.numberOfCalls] = currentCall;
                            // Notify Companion Panel
                        	updateCompanionPanel(callData);
                        	//self.subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "UPDATE_COM_PANEL" }));                        	

                        }
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentVToolbar.handleMessage(json);
                        break;

                    case constants.actions.RING_ACTION: 
                        console.log("Ringing...");

                        try{

                            var incomingData = json.callData;

                            if (incomingData.appClassification) {
                                //constants.appClass = incomingData.appClassification;
                                currentCall.setAppClassification(incomingData.appClassification); 
                                delete incomingData.appClassification;
                            }
                            if (incomingData.lookupObject) {
                                //self.call.setLookupObject(incomingData.lookupObject);
                            	currentCall.setLookupObject(incomingData.lookupObject);
                            } else {
                                //self.call.setLookupObject(null);
                            	currentCall.setLookupObject(null);
                            }

                            if (incomingData.ivrData) {
                                var ivrDataArray = JSON.parse(incomingData.ivrData);
                                for (var key in ivrDataArray) {
                                    incomingData[key] = ivrDataArray[key];
                                }
                                delete incomingData.ivrData;
                            }

                            //self.call.setInboundData(incomingData); 
                            currentCall.setInboundData(incomingData); 

                        }catch(err){
                            console.log(err);
                        }


                        //self.call.setEventId(json.callData.eventId);
                        //self.call.createAndShowCallSlot(constants.direction.inbound_call);
                        
                        //HZH 20180503
                        if (adjustedStartTime) {
                        	currentCall.setCallRingTime(adjustedStartTime);
                        } else {
                        	currentCall.setCallRingTime(currentTime);
                        }
                        
                        currentCall.setEventId(json.callData.eventId);
                        currentCall.createAndShowCallSlot(constants.direction.inbound_call);


                        break;     
                    case constants.actions.INVITE_CONFERENCE_ACTION:
                        //self.call.setHoldMode();
                    	currentCall.setHoldMode();
                        //handleSecondCallSlot(json);
                        handleSecondCallSlot(json, phoneLineIdString);
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.INVITE_CONSULT_ACTION:
                        //self.call.setHoldMode();
                        //self.consultSlot.createAndShowConsultSlot();
                        //self.consultSlot.setAuthor(author);
                    	currentCall.setHoldMode();
                    	currentConsultSlot.createAndShowConsultSlot();
                    	currentConsultSlot.setAuthor(author);
                        handleSecondCallSlot(json, phoneLineIdString);
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.ACCEPT_ACTION: 
                        //self.call.acceptAcknowledged();
                        //HZH 20180503
                        if (adjustedStartTime) {
                        	currentCall.setCallAcceptTime(adjustedStartTime);
                        } else {
                        	currentCall.setCallAcceptTime(currentTime);
                        }

                    	currentCall.acceptAcknowledged();
                        svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",true,true,"AVAILABLE","Busy. On Call",null,"Busy",{"phoneLineId":phoneLineIdString}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);
                        // Increase agent Call Count
                        increaseAgentCallCount();                        
                        var currentCall = getAgentCurrentCallCount();
                        var callData={};
                    	callData[constants.attributes.numberOfCalls] = currentCall;
                        // Notify Companion Panel
                    	updateCompanionPanel(callData);
                    	//self.subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "UPDATE_COM_PANEL" }));
                    
                        break;
                    case constants.actions.REJECT_ACTION:
                        //self.call.rejectAcknowledged();
                        //self.call.hideAndCloseCallSlot();
                    	currentCall.rejectAcknowledged();
                    	currentCall.hideAndCloseCallSlot();
                        svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",true,true,"AVAILABLE","Idle",null,"Idle",{"phoneLineId":phoneLineIdString}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);
                        break;
                    case constants.actions.END_ACTION:
                        //self.call.hideAndCloseCallSlot();
                        //self.consultSlot.hideAndCloseCallSlot();
                        //HZH 20180503
                    	currentCall.setCallRingTime(null);
                		currentCall.setCallAcceptTime(null);

                    	currentCall.hideAndCloseCallSlot();
                        currentConsultSlot.hideAndCloseCallSlot();
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentVToolbar.handleMessage(json);
                        svcMcaTlb.api.agentStateEvent(constants.events.channelPhone,"1",true,true,"AVAILABLE","Idle",null,"Idle",{"phoneLineId":phoneLineIdString}, agentStateEventChangeCallBack, constants.channelType.phoneChannelType);
                        break;
                    case constants.actions.REMOTE_END_ACTION:
                    	currentCall.endCall();
                        break;
                    case constants.actions.END_TRANSFER_ACTION:
                        //self.call.hideAndCloseCallSlot();
                        //self.consultSlot.hideAndCloseCallSlot();
                        // Handle vtoolbarActions:
                    	//self.vToolbar.handleMessage(json);
                    	currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.CONFERENCE_ACTION:
                        // Handle vtoolbarActions:
                    	//self.vToolbar.handleMessage(json);
                    	currentVToolbar.handleMessage(json);
                        break;	
                    case constants.actions.END_CONFERENCE_ACTION:
                        //self.call.setOngoingMode();
                        //self.consultSlot.hideAndCloseCallSlot();
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentCall.setOngoingMode();
                        currentConsultSlot.hideAndCloseCallSlot();
                        // Handle vtoolbarActions:
                        currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.TRANSFER_ACTION:
                        //Handle vtoolbarActions:
                    	//self.vToolbar.handleMessage(json);
                    	currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.TRANSFER_TO_ACTION:
                        //self.call.endTransferCall();
                        //self.call.hideAndCloseCallSlot();
                        //self.consultSlot.hideAndCloseCallSlot();
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentCall.endTransferCall();
                        currentCall.hideAndCloseCallSlot();
                        currentConsultSlot.hideAndCloseCallSlot();
                        // Handle vtoolbarActions:
                        currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.COMPLETE_TRANSFER_ACTION:
                        //self.call.endTransferCall();
                        //self.call.hideAndCloseCallSlot();
                        //self.consultSlot.hideAndCloseCallSlot();
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentCall.endTransferCall();
                        currentCall.hideAndCloseCallSlot();
                        currentConsultSlot.hideAndCloseCallSlot();
                        // Handle vtoolbarActions:
                        currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.RECEIVING_TRANSFER_ACTION:
                        console.log("Got Tranferred call...");
                        //self.call.setTransferredMode();
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentCall.setTransferredMode();
                        // Handle vtoolbarActions:
                        currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.CONSULT_ACTION:
                        // TODO
                    	 // Handle vtoolbarActions:
                    	//self.vToolbar.handleMessage(json);
                    	currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.END_CONSULT_ACTION:
                        //self.call.setOngoingMode();
                        //self.consultSlot.hideAndCloseCallSlot();	
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentCall.setOngoingMode();
                        currentConsultSlot.hideAndCloseCallSlot();	
                        // Handle vtoolbarActions:
                        currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.ADD_TO_CONFERENCE_ACTION:
                        console.log("Add to Conference ..");
                        //self.call.endCall();
                        //self.call.hideAndCloseCallSlot();
                        //self.call.setOngoingMode();   

                        var conferenceData = json.callData;
                        var dest = "";

                        if (conferenceData.ivrData) {
                            var ivrDataArray = JSON.parse(conferenceData.ivrData);
                            for (var key in ivrDataArray) {
                                if (key == "SVCMCA_DESTINATION_AGENT_NAME")
                                dest = ivrDataArray[key];
                            }
                            delete conferenceData.ivrData;
                        }

                        //self.call.setAddToConferenceMode(dest);
                        //self.consultSlot.hideAndCloseCallSlot();
                        currentCall.setAddToConferenceMode(dest);
                        currentConsultSlot.hideAndCloseCallSlot();

                        break;
                    case constants.actions.JOIN_CONFERENCE_ACTION:
                        console.log("Got Conference join call...");
                        //self.call.setConferenceJoinMode();
                        // Handle vtoolbarActions:
                        //self.vToolbar.handleMessage(json);
                        currentCall.setConferenceJoinMode();
                        // Handle vtoolbarActions:
                        currentVToolbar.handleMessage(json);
                        break;
                    case constants.actions.OUTBOUND_CALL_ACTION:
                    	var tempData = currentCall.getInboundData();
                    	var callData = JSON.parse(json.data);
                    	for (var token in callData) {
                            tempData[token] = callData[token];
                        }
                        currentCall.setInboundData(tempData);
                        break;	
                    case constants.actions.OUTBOUND_RING_ACTION:
                        break;
                    case constants.actions.OUTBOUND_FAIL_ACTION:
                        var incomingData = JSON.parse(json.message);
                        //self.call.updatePhoneEventTitle("Error: "+incomingData.code);
                        currentCall.updatePhoneEventTitle("Error: "+incomingData.code);
                        svcMcaTlb.api.outboundCommError(constants.events.channelPhone, currentCall.getEventId(), incomingData.code, '{"phoneLineId":phoneLineIdString}', null);                        
                        break;
                    case constants.actions.OUTBOUND_CONNECT_ACTION:
                        var incomingData = JSON.parse(json.message);
                        if (incomingData.appClassification) {
                            //constants.appClass = incomingData.appClassification;
                            currentCall.setAppClassification(incomingData.appClassification); 
                            delete incomingData.appClassification;
                        }
                        if (incomingData.lookupObject) {
                            //self.call.setLookupObject(incomingData.lookupObject);
                            currentCall.setLookupObject(incomingData.lookupObject);
                        } else {
                            //self.call.setLookupObject(null);
                            currentCall.setLookupObject(null);
                        }
                        if (incomingData.wrapupType) {
                            //self.call.setWrapupType(incomingData.wrapupType);
                            currentCall.setWrapupType(incomingData.wrapupType);
                            delete incomingData.wrapupType;
                        }
                        //var tempData = self.call.getInboundData();
                        var tempData = currentCall.getInboundData();
                        for (var token in incomingData) {
                            tempData[token] = incomingData[token];
                        }
                        self.call.setInboundData(tempData);
                        //svcMcaTlb.api.newCommEvent(constants.events.channelPhone, constants.appClass, currentCall.getEventId(), currentCall.getInboundData(), null, function(response){
                        svcMcaTlb.api.newCommEvent(constants.events.channelPhone, currentCall.getAppClassification(), currentCall.getEventId(), currentCall.getInboundData(), null, function(response){
                            console.log("Toolbar newCommEvent response: "+JSON.stringify(response));

                            //var tempData = self.call.getInboundData();
                            var tempData = currentCall.getInboundData();
                            for (var token in response.outData) {
                                tempData[token] = response.outData[token];
                            }
                            //self.call.setInboundData(tempData);
                            //self.call.updateCallData(;
                            currentCall.setInboundData(tempData);
                            currentCall.updateCallData();
                        }, constants.channelType.phoneChannelType);
                        break;
                    case constants.actions.OUTBOUND_ACCEPT_ACTION:
                        //self.call.acceptAcknowledged();
                        //self.call.updatePhoneEventTitle("");
                        currentCall.acceptAcknowledged();
                        currentCall.updatePhoneEventTitle("");
                        
                        // Increase Agent Call Count
                        increaseAgentCallCount();                        
                        var currentCall = getAgentCurrentCallCount();
                        var callData={};
                    	callData[constants.attributes.numberOfCalls] = currentCall;
                        // Notify Companion Panel
                    	updateCompanionPanel(callData);
                    	//self.subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "UPDATE_COM_PANEL" }));
                        break;
                    case constants.actions.OUTBOUND_REJECT_ACTION:
                        //self.call.updatePhoneEventTitle("Cancelled");
                        currentCall.updatePhoneEventTitle("Cancelled");
                        currentCall.endCall();
                        //svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, constants.appClass,self.eventUuid, json, constants.reason.reject, function(response){
                        svcMcaTlb.api.closeCommEvent(constants.events.channelPhone, currentCall.getAppClassification(),currentCall.getEventId(), json, constants.reason.reject, function(response){
                            console.log("reposnse: "+JSON.stringify(response));
                        }, constants.channelType.phoneChannelType);                        
                        break;
                    case constants.actions.CONSULT_CONNECTED_ACTION:
                        //self.consultSlot.consultAcknowledged();
                        currentConsultSlot.consultAcknowledged();

                        break;
                    case constants.actions.CLOSE_CONFERENCE_ACTION:
                        //self.call.endCall();
                        //self.call.hideAndCloseCallSlot();
                        //self.consultSlot.hideAndCloseCallSlot();
                        currentCall.endCall();
                        currentCall.hideAndCloseCallSlot();
                        currentConsultSlot.hideAndCloseCallSlot();
                        break;
                    case constants.actions.TEST_RING_SELF_ACTION:
                        if(json.author){
                            console.log("Test Ring action received!");
                                author = json.author;
                                message = json.message;
                                //self.subSocket.publish(JSON.stringify({ author: author, message: message, action: constants.actions.TEST_RING_SELF_ACTION}));
                        }
                        break;
                    case constants.actions.TEST_CHAT_SELF_ACTION:
                        if(json.author){
                            console.log("Test Chat action received!");
                                author = json.author;
                                message = json.message;
                                //self.subSocket.publish(JSON.stringify({ author: author, message: message, action: constants.actions.TEST_RING_SELF_ACTION}));
                        }
                        break;
                    case constants.actions.COM_PANEL_NEW_CALL_ACTION:
                    	console.log("Companion Panel New Call Request message received!");
                    	startNewCall();
                    	break;
                    case constants.actions.COM_PANEL_REFRESH_ACTION:
                    	console.log("Companion Panel Refresh Request message received!");
                    	if (self.loggedIn) {
                            var currentCall = getAgentCurrentCallCount();
                            var callData={};
                        	callData[constants.attributes.numberOfCalls] = currentCall;
                            // Notify Companion Panel
                        	updateCompanionPanel(callData);
                    	}
                    	break;
                    default:
                        break;
                }
            }

            function handleChatMessage(json) {

                console.log("AgentInfo HandleChatMessage(), action type " + json.action);
                console.log("AgentInfo HandleChatMessage(), json data " + JSON.stringify(json.action));;
                
                var currentChat = null;
                var phoneLineIdString = null;
                var chatTestMode = false;
                
                // use phone line 2 only when specified
                if (json.callData) {
                	if (json.callData.phoneLineId) {
                		if (json.callData.phoneLineId == "2") {
                			currentChat = self.chat2;
                 			phoneLineIdString = "2";
                		} else {
                			currentChat = self.chat;
                			phoneLineIdString = "1";
                		}
                	} else {
                		
                		currentChat = self.chat;
                		phoneLineIdString = '1';
                	}
                	
                    if (json.callData.chatTestMode) {
                        if (json.callData.chatTestMode === "Y") {
                        	chatTestMode = true;
                        }
                    }

                }

                switch(json.action){
  
                    case constants.actions.CHAT_ACTION: 

                        console.log("Chat Ringing...");          

                        try{

                            var incomingData = json.callData;

                            if (incomingData.appClassification) {
                                //constants.appClass = incomingData.appClassification;
                                currentChat.setAppClassification(incomingData.appClassification); 
                                delete incomingData.appClassification;
                            }
                             
                           if (incomingData.ivrData) {
                                var ivrDataArray = JSON.parse(incomingData.ivrData);
                                for (var key in ivrDataArray) {
                                    incomingData[key] = ivrDataArray[key];
                                }
                                delete incomingData.ivrData;
                            }

                            currentChat.setInboundData(incomingData); 

                        }catch(err){
                            console.log(err);
                        }

                        currentChat.setEventId(json.callData.eventId);
                        currentChat.createAndShowChatSlot(constants.direction.inbound_call, chatTestMode);
                        
                        break;     
                     case constants.actions.ACCEPT_ACTION: 
                    	currentChat.acceptChatAcknowledged(chatTestMode);
                        break;
                    case constants.actions.REJECT_ACTION:
                     	currentChat.rejectChatAcknowledged(chatTestMode);
                    	currentChat.hideAndCloseChatSlot();
                        break;
                    case constants.actions.DISCONNECT_ACTION:
                    	currentChat.disconnectChatAcknowledged(chatTestMode);
                     	currentChat.hideAndCloseChatSlot();
                        break;
                    default:
                        break;
                }
            }

            function outboundCallCallback(response) {
            	var outData = null;
            	
            	if (response.outData) {
            		outData = response.outData;
            	} else if (response.data && response.data.outData) {
            		outData = response.data.outData;
            	}
            	
            	self.call.setInboundData(response.outData);
            	
            	self.call.setEventId(response.uuid);
                self.call.createAndShowCallSlot(constants.direction.outbound_call);
                self.subSocket.publish(JSON.stringify({ author: author, 
                                                        message: '{"eventId":"'+response.uuid+'"}', 
                                                        action: constants.actions.OUTBOUND_CALL_ACTION,
                                                        data: JSON.stringify(outData)}));
            }

            function dataUpdatedCallback(response) {
            	var dataUpdated = null;
            	
            	if (response.outData) {
            		dataUpdated = response.outData;
            	} else if (response.data && response.data.outData) {
            		dataUpdated = response.data.outData;
            	}
            	
            	if (dataUpdated) {
            		console.log("======== Live Window Toolbar Receives updated data: "+ JSON.stringify(response));
            	} else {
            		console.log("======== Live Window Toolbar Receives empty updated data: "+ JSON.stringify(response));
            	}

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
                
                console.log("interactionCommandExecutor process eventId: " + command.eventId);
                
                console.log("interactionCommandExecutor slot1 eventId: " + self.call.getEventId());
                console.log("interactionCommandExecutor slot2 eventId: " + self.call2.getEventId());
                
                var commandEventId = command.eventId;    
                var channelType = command.channel;
                

                var currentCall = null;
                var currentChat = null;
               
                var lind1EventId = self.call.getEventId();
                var lind2EventId = self.call2.getEventId();
                var chat1EventId = self.chat.getEventId();
                var chat2EventId = self.chat2.getEventId();
                
                if (lind2EventId != null && lind2EventId == commandEventId) {
                	currentCall = self.call2;
                } else {
                	currentCall = self.call;
                }

                if (chat2EventId != null && chat2EventId == commandEventId) {
                	currentChat = self.chat2;
                } else {
                	currentChat = self.chat;
                }
                
                //sjmD
                var cmd = command.command.toUpperCase();
                
                if (channelType === "CHAT") {
                	switch(cmd) {
	                 case constants.actions.ACCEPT_ACTION: 
	                       currentChat.acceptChatAcknowledged();
	                       break;
	                case constants.actions.REJECT_ACTION:
	                       currentChat.rejectChatAcknowledged();
	                       break;
	                case constants.actions.DISCONNECT_ACTION:
	                    currentChat.disconnectChatAcknowledged();
	                    break;
	                case constants.actions.NOTIFICATION_TIMER_EXP_ACTION:
	                    //self.call.notificationTimerExpAcknowledged();
	                    currentChat.notificationTimerExpAcknowledged();
	                    break;
	                }
                	
                } else {
                	switch(cmd) {
	                 case constants.actions.ACCEPT_ACTION: 
	                       currentCall.acceptAcknowledged();
	                       break;
	                case constants.actions.REJECT_ACTION:
	                       //self.call.rejectAcknowledged();
	                       currentCall.rejectAcknowledged();
	                       break;
	                case constants.actions.TRANSFER_ACTION:
	                    // HHUANG 20191213 handle transfer
	                    currentCall.transferCall();
	                    break;
	                case constants.actions.DISCONNECT_ACTION:
	                    // HHUANG 20190809 handle disconnect
	                    currentCall.endCall();
	                    break;
	                case constants.actions.NOTIFICATION_TIMER_EXP_ACTION:
	                    //self.call.notificationTimerExpAcknowledged();
	                    currentCall.notificationTimerExpAcknowledged();
	                    break;
	                }
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
                case "getActiveEngagements":
                	
                	var outObj = null;
                	
                	if (self.activeEngagements != null) {
                		outObj = self.activeEngagements;
                	} else {
                		outObj = "{\"activeCount\" :\" 0\", \"engagements\": \"\"}";
                	}
                	
                	command.outData = JSON.parse(outObj);
                	
                    break;
                case "makeAvailable":
                       //alert("makeAvailable command invoked");
                       console.log("makeAvailable command invoked");
                       break;
                case "makeUnavailable":
                       //alert("makeUnavailable command invoked");
                       console.log("makeUnavailable command invoked");
                       break;
                case "getActiveInteractionCommands":                
             	   var outData = {'channel':command.channel,                
             		'supportedCommands':[{'name':"hold", 'toggleCommand':"unhold", 'defaultEnabled':true,
             					         'defaultVisible':true, 'displayName':"Hold",'position':1, 'icon':""},
                                         {'name':"unhold",'toggleCommand':"hold",'defaultEnabled':false,
                                          'defaultVisible':false, 'displayName':"Off Hold", 'position':1, 'icon':""}, 
             					         {'name':"mute", 'toggleCommand':"unmute", 'defaultEnabled':true, 
	                			          'defaultVisible':true, 'displayeName':"Mute", 'position':2, 'icon':""},
	                					 {'name':"unmute", 'toggleCommand':"mute", 'defaultEnabled':false,
	                					  'defaultVisible':false, 'displayName':"Unmute", 'position':2, 'icon':""}, 
	                					 {'name':"record", 'toggleCommand':"stopRecord", 'defaultEnabled':true,
	                 					  'defaultVisible':true, 'displayName':"Record", 'position':3, 'icon':""}, 
	                					 {'name':"stopRecord", 'toggleCommand':"record", 'defaultEnabled':false,
	                                  	  'defaultVisible':false, 'displayName':"Stop Recording", 'position':3,'icon':""}, 
	                					 {'name':"transfer", 'toggleCommand':null, 'defaultEnabled':true,
	                                      'defaultVisible':true, 'displayName':"Transfer", 'position':4, 'icon':""}, 
	                					 {'name':"disconnect", 'toggleCommand':null, 'defaultEnabled':true,
	                			          'defaultVisible':true, 'displayName':"Hang Up", 'position':5, 'icon':""}]}; 
             			command.outData = outData;                
             		break;
                }
                command.result = 'success';
                command.sendResponse(command);
            }

            function configureAgentInfo() {
                retrieveAboutBoxInfo();

                svcMcaTlb.api.onOutgoingEvent(constants.events.channelPhone, constants.appClass, outboundCallCallback, constants.channelType.phoneChannelType);
                svcMcaTlb.api.onToolbarInteractionCommand(interactionCommandExecutor);
                console.log("INVOKING new agentCommand Listener!!!");
                svcMcaTlb.api.onToolbarAgentCommand(constants.events.channelPhone, constants.channelType.phoneChannelType, agentCommandExecutor);
                svcMcaTlb.api.onDataUpdated(constants.events.channelPhone, constants.appClass, dataUpdatedCallback, constants.channelType.phoneChannelType);

                svcMcaTlb.api.getConfiguration("TOOLBAR", function(response) { 
                    if (response.configuration) {
                        var agentId = response.configuration.agentId;
                        
                        console.log("configureAgentInfo returned with agentId " + agentId);

                        var comPanelUrl = response.configuration.companionPanelUrl;
                        var comPanelTitle = response.configuration.companionPanelTitle;
                        //var features = JSON.parse(response.configuration.features);
                        var features = response.configuration.features;

                        console.log("getConfiguration agentId: " + agentId + " Companion Panel URL: " + comPanelUrl + " Title: "+ comPanelTitle);
                        console.log("getConfiguration features: " + features);

                        setCompanionPanelUrl(comPanelUrl);
                        setCompanionPanelTitle(comPanelTitle); 
                        
                        if (features) { 
                        	for (var i=0; i<features.length; i++) {
                        	    var feature = features[i]; 
                        	    
                        	    if (feature == constants.features.INBOUND_CHAT_FEATURE) {
                        	    	enableAgentInboundChatFeature();
                        	    }

                        	    if (feature == constants.features.NRT_WORK_ASSIGN_FEATURE) {
                        	    	enableAgentNRTFeature();
                        	    }

                        		console.log("getConfiguration feature: ["+i+"]:" + feature +";");
                        	}
                        }

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
                    url : 'https://cti-prod.cna.iad.demoservices001.oraclepdemos.com/toolbar/services/agent/id?userAgent=alan.hooper',
                    data: {},
                    error: function( jqXHR, textStatus, errorThrown ) {
                        console.log("Get Agent ID failed with status: "+jqXHR.status+" - message: "+textStatus);
                        console.log("Fallback too locally generated ");

                        var unqUsername = agentId+"_"+(new Date()).getTime();
                        self.agentId = agentId; 
                        self.agentUniqueId = unqUsername;
                        self.subSocket.connect(unqUsername, function(){
                            atmoSubscribeCallback(unqUsername);
                        }, handleMessage);
                    },
                    success : function ( data, textStatus, jqXHR ) {
                        console.log("agentInfo Retrieved: "+data.agentId);
                        var unqUsername = data.agentId;
                        self.agentId = agentId; 
                        self.agentUniqueId = data.agentId;
                        self.subSocket.connect(unqUsername, function(){
                            atmoSubscribeCallback(unqUsername);
                            self.call.setAuthor(unqUsername);
                            self.call2.setAuthor(unqUsername);
                            self.chat.setAuthor(unqUsername);
                            self.chat2.setAuthor(unqUsername);
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
	                            svcMcaTlb.api.onOutgoingEvent(constants.events.channelPhone, constants.appClass, outboundCallCallback, constants.channelType.phoneChannelType);
	                            svcMcaTlb.api.onToolbarInteractionCommand(interactionCommandExecutor);
	                            svcMcaTlb.api.onToolbarAgentCommand(constants.events.channelPhone, constants.channelType.phoneChannelType, agentCommandExecutor);
	                            svcMcaTlb.api.onDataUpdated(constants.events.channelPhone, constants.appClass, dataUpdatedCallback, constants.channelType.phoneChannelType);
	                            
	                            var unqUsername = data.agentId;
	                            self.subSocket.connect(unqUsername, function(){
	                                    atmoSubscribeCallback(unqUsername);
	                                    self.call.setAuthor(unqUsername);
	                                    self.call2.setAuthor(unqUsername);
	                                    self.chat.setAuthor(unqUsername);
	                                    self.chat2.setAuthor(unqUsername);
	                                }, handleMessage);
	                                
	                            svcMcaTlb.api.postToolbarMessage(JSON.stringify( {"msgCommand":"AGENT_AVAIL"} ), function(response) {
	                                console.log("======== Response for POST from agentLogin - status: "+response.result);
	                            });
	                            
	                            // HZH Init virtical Toolbar
	                            self.vToolbar.init();
                                              
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

            function autoReconnect() {
            	if (self.agentUniqueId) {
            		console.log("Auto Reconnect to : "+ self.agentUniqueId);
            		self.subSocket.connect(self.agentUniqueId, function(){
            			atmoSubscribeCallback(self.agentUniqueId);
            		}, handleMessage);
            	}
            }
            
            function removeLoggedinAgent() {
                var uuidReqData = {
                    type: 'GET',
                    dataType: 'json',
                    async: true,
                    url : liveConn.getUrl()+'services/agent/remove',
                    data: {},
                    error: function( jqXHR, textStatus, errorThrown ) {
                        console.log("vToolbar remove agent loggedin failed with status: "+jqXHR.status+" - message: "+textStatus);
                    },
                    success : function ( data, textStatus, jqXHR ) {
                        console.log("vToolbar Retrieved: "+data.agentId);
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
                getLastState(uniqueUserName);
            };

            function getLastState(uniqueUserName){
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

            	console.log("Restore Call Slot for action "+action+ " with data :" + JSON.stringify(json));

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

            // HHUANG 20161110
            // Send Test Message
            function testRing(agentId, callData) {

                console.log("Start Testing Ring to "+agentId+" - callData: "+callData);


                self.subSocket.publish(JSON.stringify({ author: author, callData: callData, action: constants.actions.TEST_RING_SELF_ACTION}));

                //getLastState(author);

                console.log("Test Ring action message sent!");


            };

            // HHUANG 20180626
            // Send Test Chat Message
            function testChat(agentId, callData) {

                console.log("Start Testing Chat to "+agentId+" - callData: "+callData);


                self.subSocket.publish(JSON.stringify({ author: author, callData: callData, action: constants.actions.TEST_CHAT_SELF_ACTION}));
 
                console.log("Test Chat action message sent!");


            };

            function vToolbarInit() {

                console.log("Start vtoolbar init");

                self.vToolbar.init();
                window.searchAgent = self.vToolbar.searchAgent;
                $('#newCallButton').click(makeNewCall);

                console.log("finish vtoolbar init");


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
                        //setupAboutBox('ERROR_READING.UNKNOWN_BUILD', self.subSocket.getUrl()+'adminMulti.jsp' );
                        setupAboutBox('ERROR_READING.UNKNOWN_BUILD', self.subSocket.getUrl()+'admin.jsp' );
                    },
                    success : function ( data, textStatus, jqXHR ) {
                        console.log("Retrieved: "+data.buildString);
                        //setupAboutBox(data.buildString, self.subSocket.getUrl()+'adminMulti.jsp' );
                        setupAboutBox(data.buildString, self.subSocket.getUrl()+'admin.jsp' );
                    }
                };
                $.ajax(uuidReqData);
            };

            function switchCompanionControlPanel() {
            	if (self.companionPanelShow) {
            		$('#CompanionControl').hide();
            		self.companionPanelShow = false;
            	} else {
            		$('#CompanionControl').show();
            		self.companionPanelShow = true;
            	}
            }
            
            return {

                callInfo : self.call,
                callInfo2 : self.call2,
                init: function(loggedInCallback) {
                    svcMcaTlb.api.onToolbarMessage(self.toolbarMessageCallback);
                    
                    svcMcaTlb.api.getConfiguration("TOOLBAR", function(response) { 
                        if (response.configuration) {
                            var agentId = response.configuration.agentId;
                            
                            console.log("configureAgentInfo returned with agentId " + agentId);

                            var comPanelUrl = response.configuration.companionPanelUrl;
                            var comPanelTitle = response.configuration.companionPanelTitle;
                            //var features = JSON.parse(response.configuration.features);
                            var features = null;
                            if (response.configuration.features) {
                            	features = JSON.parse(response.configuration.features);
                            }

                            console.log("getConfiguration agentId: " + agentId + " Companion Panel URL: " + comPanelUrl + " Title: "+ comPanelTitle);
                            console.log("getConfiguration features: " + features);

                            setCompanionPanelUrl(comPanelUrl);
                            setCompanionPanelTitle(comPanelTitle); 
                            
                            if (features) { 
                            	for (var i=0; i<features.length; i++) {
                            	    var feature = features[i]; 
                            	    
                            	    if (feature == constants.features.INBOUND_CHAT_FEATURE) {
                            	    	enableAgentInboundChatFeature();
                            	    }

                            	    if (feature == constants.features.NRT_WORK_ASSIGN_FEATURE) {
                            	    	enableAgentNRTFeature();
                            	    }

                            		console.log("getConfiguration feature: ["+i+"]:" + feature +";");
                            	}
                            }

                            if (agentId) {
                            	checkAgentLoggedin(agentId, loggedInCallback); 
                            }
                         }
                    });

                    //checkAgentLoggedin(loggedInCallback);
                    closeCompanionPanel();
                    vToolbarInit();

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
                
                setAgentOnBreak : function() {
                	setAgentOnBreak();
                },
                setAgentOffBreak : function() {
                	setAgentOffBreak();
                },
                outboundCallCallback: function(response) {
                    outboundCallCallback(response);
                },
        	    dataUpdatedCallback: function(response) {
                    dataUpdatedCallback(response);
        	    },
                testRing: function(agentId, callData) {

                    testRing(agentId,callData);

                },
                testChat : function(agentId, callData) {

                	testChat(agentId,callData);

                },

                handleAgentStatusChange: function() {
                	handleAgentStatusChange();
                },
                
                handleMessage: function(json){
                    handleMessage(json);
                },
                
                switchCompanionControlPanel: function(){
                	switchCompanionControlPanel();
                },


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
                
                disconnect : function(){
                    self.subSocket.disconnect();
                },
                
                openCompanionPanel1 : function() {
                	openCompanionPanel1();	
                },
                
                openCompanionPanel2 : function() {
                	openCompanionPanel2();	
                },

                openCompanionPanelUrl : function(url) {
                	openCompanionPanelUrl(url);	
                },

                sendMessageToCompanionPanel : function(msg) {
                	sendMessageToCompanionPanel(msg);	
                },

                closeCompanionPanel : function() {
                	closeCompanionPanel();	
                },

                startNewCall : function() {
                	startNewCall();	
                },

                cancelNewCall : function() {
                	cancelNewCall();	
                },

                clearCompanionControlInputs: function() {
                	clearCompanionControlInputs();	
                },
                
                enableAgentInboundChatFeature: function() {
                	enableAgentInboundChatFeature();
                },
                
                disableAgentInboundChatFeature: function() {
                	disableAgentInboundChatFeature();
                },

                enableAgentNRTFeature: function() {
                	enableAgentNRTFeature();
                },
                
                disableAgentNRTFeature: function() {
                	disableAgentNRTFeature();
                }


            };
        }

        return new agentInfo();
    }    
    //============================= END agentInfo.js
    //============================= START testAgent.js
    function testAgentFactory(agent, liveConn, constants, svcMca) { 
    	       	
    	var self = this;
    	    self.subSocket = liveConn;
    	    self.agent = agent;
    	    self.eventId;

        function testAgent(){
            function acceptCall(){
            }

            function rejectCall(){
                $('#testToolbar').remove();
            }

            function endCall(){
                $('#testToolbar').remove();
            }

            function connectOutboundCall() {
                $("#connBtn").hide();
                $("#failBtn").hide();
                $("#accBtn").show();
                $("#rejBtn").show();

                var currentCall = agent.callInfo;
                var inboundData = currentCall.getInboundData();
                inboundData['wrapupType'] = constants.wrapupType.unlimitedWrapup;
                inboundData[constants.attributes.phoneLineId] = '1';

                var eventId = currentCall.getEventId();
                
                svcMca.api.newCommEvent(constants.events.channelPhone, constants.appClass, self.eventId, inboundData, null, function(response){
                //svcMca.api.newCommEvent(constants.events.channelPhone, constants.appClass, '', inboundData, null, function(response){
                    console.log("Toolbar newCommEvent response: "+JSON.stringify(response));
                    
                    var currentCall = agent.callInfo;
                    
                    var tempData = currentCall.getInboundData();
                    for (var token in response.outData) {
                        tempData[token] = response.outData[token];
                    }
                                        
                    currentCall.setInboundData(tempData);
                    currentCall.updateCallData();
                }, constants.channelType.phoneChannelType);
/*
                var msg = {};
                msg['appClassification'] = "ORA_SERVICE";
                msg['wrapupType'] = "Unlimited Wrap Up";
                msg['callData'] = inboundData;
                
                self.subSocket.publish(JSON.stringify({ author: self.author, message: JSON.stringify(msg), action : "OUTBOUND_CONNECT" }));
*/                
            }

            function failOutboundCall() {
                $("#outboundButtons").hide();
                $("#connBtn").off("click");
                $("#failBtn").off("click");
                $("#accBtn").off("click");
                $("#rejBtn").off("click");

                var currentCall = agent.callInfo;

                var callData={};
                callData[constants.attributes.phoneLineId] = '1';

                //self.subSocket.publish(JSON.stringify({ author: self.author, callData: callData, message: JSON.stringify({code:'INVALID_NUMBER', phoneLineId:'1'}), action : "OUTBOUND_FAIL" }));
               
               currentCall.updatePhoneEventTitle("Error: Connection Failure");
               svcMca.api.outboundCommError(constants.events.channelPhone, '', '', '', null);

            }

            function acceptOutboundCall() {
                $("#outboundButtons").hide();
                $("#connBtn").off("click");
                $("#failBtn").off("click");
                $("#accBtn").off("click");
                $("#rejBtn").off("click");

                var currentCall = agent.callInfo;
                
                //Outbound is always on line 1
                var inboundData = currentCall.getInboundData();
                
                inboundData['wrapupType'] = constants.wrapupType.unlimitedWrapup;
                inboundData[constants.attributes.phoneLineId] = '1';

                
                currentCall.updatePhoneEventTitle("Outgoing call");
                //set to callInfo.callStates.ongoing
                //currentCall.setCallState("on");
                currentCall.acceptAcknowledged();
                
                if (currentCall.getWrapupType() == null) {
                	currentCall.setWrapupType(constants.wrapupType.unlimitedWrapup);
                }
                //var eventId = currentCall.getEventId();
                                
                //svcMca.api.startCommEvent(constants.events.channelPhone, constants.appClass, "", inboundData, function(response){                    
                svcMca.api.startCommEvent(constants.events.channelPhone, constants.appClass, self.eventId, inboundData, function(response){                    
                    console.log("Toolbar startCommEvent reposnse: "+JSON.stringify(response));

                    var currentCall = agent.callInfo;
                    
                    //var tempData = agent.callInfo.getInboundData();
                    var tempData = currentCall.getInboundData();
                    for (var token in response.outData) {
                        tempData[token] = response.outData[token];
                    }
                    
                     
                    currentCall.setInboundData(tempData);
                    currentCall.updateCallData();
                }, constants.channelType.phoneChannelType);
/*
                var callData={};
                callData[constants.attributes.phoneLineId] = '1';

                self.subSocket.publish(JSON.stringify({ author: self.author, callData: callData, message: JSON.stringify({code:'succes', phoneLineId:'1'}), action : "OUTBOUND_ACCEPT" }));        
*/
            }

            function rejectOutboundCall() {
                $("#outboundButtons").hide();
                $("#connBtn").off("click");
                $("#failBtn").off("click");
                $("#accBtn").off("click");
                $("#rejBtn").off("click");

                // Outbound call is always on line 1
                //agent.callInfo.updatePhoneEventTitle("Call Rejected");
                var currentCall = agent.callInfo;
                //var eventId = currentCall.getEventId();
                
                currentCall.updatePhoneEventTitle("Call Rejected");
                svcMca.api.closeCommEvent(constants.events.channelPhone, constants.appClass, self.eventId, {}, constants.reason.reject, function(response){
                //svcMca.api.closeCommEvent(constants.events.channelPhone, constants.appClass, "", {}, constants.reason.reject, function(response){
                    console.log("reposnse: "+JSON.stringify(response));
                }, constants.channelType.phoneChannelType);  

            }

            function endOutboundCall() {
                $("#outboundButtons").hide();
                $("#connBtn").off("click");
                $("#failBtn").off("click");
                $("#accBtn").off("click");
                $("#rejBtn").off("click");
                $('#testToolbar').remove();
                
/*                var currentCall = agent.callInfo;
                
                currentCall.updatePhoneEventTitle("Call Ended");
                svcMca.api.closeCommEvent(constants.events.channelPhone, constants.appClass, "", {}, constants.reason.reject, function(response){
                    console.log("reposnse: "+JSON.stringify(response));
                }, constants.channelType.phoneChannelType);  
*/
            }

            function testOutboundCallCallback(response) {
            	// Outbound is always on Line 1
                //agent.callInfo.setInboundData(response.outData);                        
                //agent.callInfo.setEventId('testToolbar');
                //agent.callInfo.createAndShowCallSlot(constants.direction.outbound_call);
            	
            	var currentCall = agent.callInfo;
            	
            	currentCall.setInboundData(response.outData);                        
            	currentCall.setEventId('testToolbar');
            	//self.eventId = response.uuid;
            	self.eventId = 'testToolbar';
            	currentCall.createAndShowCallSlot(constants.direction.outbound_call);
                showOutboundCall();
            }

            function showOutboundCall() {
                $("#connBtn").click(function(){connectOutboundCall();}).show();
                $("#failBtn").click(function(){failOutboundCall();}).show();
                $("#accBtn").click(function(){acceptOutboundCall();}).hide();
                $("#rejBtn").click(function(){rejectOutboundCall();}).hide();
                $("#reject").click(function(){endOutboundCall();}).show();
                $("#outboundButtons").show();
            }



            return {
                initCallSlot : function(agentId){
                    $('#testToolbar').remove();

                    var inboundData;
                    try{ inboundData = JSON.parse($('#inboundDataJson').val()); }catch(err){ console.log(err); }

                    //agent.callInfo.setAuthor(agentId);
                    //agent.callInfo.setInboundData(inboundData);                      
                    //agent.callInfo.setEventId("testToolbar");
                    //agent.callInfo.createAndShowCallSlot(constants.direction.inbound_call);
                    self.author = agentId;
                    agent.testRing(agentId, inboundData);

                    //$('#accept').click(acceptCall);
                    //$('#reject').click(rejectCall);
                    //$('#end').click(endCall);
                },
                initChatSlot : function(agentId){
                    $('#testToolbar').remove();

                    var inboundData;
                    try{ inboundData = JSON.parse($('#inboundDataJson').val()); }catch(err){ console.log(err); }

                    self.author = agentId;
                    agent.testChat(agentId, inboundData);

                },
                initTestFloatingToolbar :function(){
                    $('#testingPanel').show();
                    $('#outbound').remove();
                    $('#outboundTest').show();
                    //"#vUrl").val(window.location.href.replace("index.html","vToolbar.html")); 
                    $("#vUrl").val(window.location.href); 
                    svcMca.api.onOutgoingEvent(constants.events.channelPhone, constants.appClass, testOutboundCallCallback, constants.channelType.phoneChannelType);
                    //svcMcaTlb.api.onToolbarInteractionCommand(interactionCommandExecutor);
                    //svcMcaTlb.api.onToolbarAgentCommand(constants.events.channelPhone, constants.channelType.phoneChannelType, agentCommandExecutor);
                   
                    $('#jwtBtn').click(function () {

                    	svcMca.api.getConfiguration("FA_TOKEN", function(response) {
                                console.log('########## [RECEIVED CONFIG - JWT RESPONSE] ');
                                self.jwt = response.configuration.faTrustToken;
                            });                    
                    });

                    $('#jwtRestBtn').click(function () {
                        var frameOrigin = null;
                        var searchString = window.location.search;
                        if (typeof searchString !== 'string' || searchString.length === 0) {
                          throw new Error("API not correctly delpoyed - missing init attributes!");
                        }
                        if (searchString.charAt(0) === '?') {
                          searchString = searchString.slice(1);
                          if (searchString.length === 0) {
                            throw new Error("API not correctly delpoyed - missing init attributes!");
                          }
                        }
                        var searchParams = searchString.split('&');
                        for (var i = 0;i < searchParams.length;i++) {
                          var pair = searchParams[i].split('=');
                          if (pair[0] && pair[1] && pair[0] === 'oraParentFrame') {
                            frameOrigin = decodeURIComponent(pair[1]).toLowerCase();
                          }
                        }

                        var srNum = $('#jwtRestSr').val();
                        var remoteUrl = frameOrigin+'/serviceApi/resources/latest/serviceRequests/'+srNum;
                        var uuidReqData = {
                            type: 'GET',
                            dataType: 'json',
                            async: true,
                            url : self.subSocket.getUrl()+'services/calls/remote?remoteUrl='+encodeURIComponent(remoteUrl)+'&jwt='+self.jwt,
                            data: {},
                            error: function( jqXHR, textStatus, errorThrown ) {
                                console.log("############## [AJAX CALL FAILED] Failed with status: "+jqXHR.status+" - message: "+textStatus);
                                $('#jwtRestSrResult').val("ERROR");
                            },
                            success : function ( data, textStatus, jqXHR ) {
                                 console.log("############## [AJAX CALL SUCCESS] Retrieved: "+data.status);
                                 $('#jwtRestSrResult').val(data.status)
                            }
                        };
                        $.ajax(uuidReqData);
                    });
                    
                }
            }
        }
        return new testAgent();
    };
    //============================= END testAgent.js

    
    window.initFullAgent = function() {
        var liveConn = liveConnFactory(atmosphere);
        var callInfo = callInfoFactory(constants, svcMca.tlb);
        var chatInfo = chatInfoFactory(constants, svcMca.tlb);
        var consultSlot = callInfoFactory(constants, svcMca.tlb);
        var agentInfo = agentInfoFactory(liveConn, callInfo, chatInfo, consultSlot, constants, svcMca.tlb);
        var testAgent = testAgentFactory(agentInfo, liveConn, constants, svcMca.tlb);
        
        window.tlbAgent = agentInfo;
        var agentId = "";
        window.switchToTestMode = function(){
            agentId = $('.loggedInUser').text();

            //agent.logoutAction();
            //agent.disconnect();
            window.initCallSlot = function(){
                console.log("INIT TEST WITH: "+agentId);
                testAgent.initCallSlot(agentId);
            };
            window.initChatSlot = function(){
                console.log("INIT TEST CHAT WITH: "+agentId);
                testAgent.initChatSlot(agentId);
            };
            testAgent.initTestFloatingToolbar();

            $('#testDialpad').click(function() {
            	
            	agentInfo.startNewCall();
            	//event.preventDefault();
    	     });        

            $('#closeDialpad').click(function() {
            	
            	agentInfo.cancelNewCall();
            	//event.preventDefault();
    	     });       

        };
        window.switchToTestRedirect = function(){
        	var redirectUrl = ('#redirectUrllink').val();
        	$('#redirectPanel').show();
        };
        window.showQueryFields = function(){
            var status = $('#queryData').css('display');
            if(status === 'none'){
                $('#queryData').show();
            }else{
                $('#queryData').hide();
            }
        };

        window.handleAgentStatusChange = function () {
        	
        	if (agentInfo.getAgentStatus() == 'OFF') {
	            agentInfo.agentLogin();
	            userLoggedInInfo();
        	} else if (agentInfo.getAgentStatus() == 'LOGGED_IN') {
        	    agentInfo.setAgentOnBreak();
        	} else if (agentInfo.getAgentStatus() == 'ON_BREAK') {
        		agentInfo.setAgentOffBreak();	
        	}
	     };

        function userLoggedInInfo() {
            $('.userSlot').removeClass('agentOff').addClass('agentOn');
            $('.agentIcon').removeClass('agentIconOff').addClass('agentIconOn');
            $('#availableBtn').removeClass('unavailBtn').addClass('availBtn');
            $('#availableBtn').attr('title', 'Available');
            agentInfo.setAgentStatus('LOGGED_IN');
            //$('#availableBtn').off("click");
        };

        $('#availableBtn').click(function() {
        	

		            svcMcaTlb.api.getConfiguration("TOOLBAR", function(response) { 
                if (response.configuration) {
                    var agentId = response.configuration.agentId;
                    
                    var comPanelUrl = response.configuration.companionPanelUrl;
                    var comPanelTitle = response.configuration.companionPanelTitle;
                    //var features = response.configuration.features;
                    var features = null;
                    if (response.configuration.features) {
                    	features = JSON.parse(response.configuration.features);
                    }

                    console.log("getConfiguration agentId: " + agentId + " Companion Panel URL: " + comPanelUrl + " Title: "+ comPanelTitle);
                    console.log("getConfiguration features: " + features);

                    setCompanionPanelUrl(comPanelUrl);
                    setCompanionPanelTitle(comPanelTitle); 
                    
                    if (features) { 
                    	for (var i=0; i<features.length; i++) {
                    	    var feature = features[i]; 
                    		console.log("getConfiguration feature: ["+i+"]:" + feature +";");
                    	}
                    }
                    
                    if (agentId) {
                        retrieveUniqueAgentId(agentId);
                    }
                 }
            });

                svcMcaTlb.api.postToolbarMessage(JSON.stringify( {"msgCommand":"AGENT_AVAIL"} ), function(response) {
                    console.log("======== Response for POST from agentLogin - status: "+response.result);
                });
                agentSignIn();
		userLoggedInInfo()
	     });

        $('#cpCtrlBtn').click(function() {
        	
        	if (agentInfo.getAgentStatus() != 'OFF') {
        		agentInfo.switchCompanionControlPanel();
        	}
        	
        	//event.preventDefault();
	     });
        
        $('#openCP1').click(function() {
        	
        	agentInfo.openCompanionPanel1();
        	//event.preventDefault();
        	//agentInfo.agentLogin();
	     });

        $('#openCP2').click(function() {
        	
        	agentInfo.openCompanionPanel2();
        	//event.preventDefault();
        	//agentInfo.agentLogin();
	     });

        $('#openCP3').click(function() {
        	
        	var url = $("#companionPanelUrl").val();
        	agentInfo.openCompanionPanelUrl(url);
        	//event.preventDefault();
        	//agentInfo.agentLogin();
	     });

        $('#sendCPMessage').click(function() {
        	
        	var msg = $("#companionPanelMessage").val();
        	agentInfo.sendMessageToCompanionPanel(msg);
        	//event.preventDefault();
	     });
        
        $('#closeCP').click(function() {
        	
        	agentInfo.closeCompanionPanel();
        	//event.preventDefault();
	     });
        
        $('#newCall').click(function() {
        	
        	agentInfo.startNewCall();
        	//event.preventDefault();
	     });        

        $('#clearIuputs').click(function() {
        	
        	agentInfo.clearCompanionControlInputs();
        	//event.preventDefault();
	     });        

        $('#cancelNewCall').click(function() {
        	
        	agentInfo.cancelNewCall();
        	//event.preventDefault();
	     });       

        agentInfo.init(userLoggedInInfo);
        console.log("###########  fullAgent init CALLED #########");
    };
})();
