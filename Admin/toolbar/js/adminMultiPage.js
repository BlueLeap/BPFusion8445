define(['jquery', 'liveConn', 'constants'], function($, liveConn, constants) {
    var content = $('#content');
    var agents = $('#agents');
    var login = $('#login');
    var adminPanel = $('#adminPanel');
    var logged = false;
    var authUser = $("#input").val();
    var subSocket = liveConn;
    var author = null;
    
    var agentIdMap = new Map();

    window.onbeforeunload = function() {
        subSocket.publish(JSON.stringify({ author: author, message: '', action: 'LOGOUT' }));
    };

        
    function atmoCallback(unqUsername) {
        subSocket.publish(JSON.stringify({ author: unqUsername, message: unqUsername, action : 'ADMIN' }));                
    }
    
   function agentLineEventIds() {
    	
    	var self = this;
    	self.line1EventId = null;
    	self.line1OutboundEventId = null;
    	self.line2EventId = null;
    	self.line1Active = false;
    	self.line2Active = false;
    	self.relogin = false;
    	    		
    	self.setLine1EventId = function (id) {    	
    		self.line1EventId = id;
    	}

    	self.getLine1EventId = function () {
    		return self.line1EventId;
    	}

    	self.setLine1OutboundEventId = function (id) {    	
    		self.line1OutboundEventId = id;
    	}

    	self.getLine1OutboundEventId = function () {
    		return self.line1OutboundEventId;
    	}

    	self.setLine2EventId = function (id) {
    		self.line2EventId = id;
    	}

    	self.getLine2EventId = function () {
    		return self.line2EventId;
    	}

    	self.setLine1Active = function (flag) {
    		self.line1Active = flag;
    	}

    	self.isLine1Active = function () {
    		return self.line1Active;
    	}

    	self.setLine2Active = function (flag) {
    		self.line2Active = flag;
    	}

    	self.isLine2Active = function () {
    		return self.line2Active;
    	}
    	
    	self.isRelogin = function () {
    		return self.relogin;
    	}
    	
    	self.setRelogin = function (flag) {
    		self.relogin = flag;
    	}
    	
    	return {
        	setLine1EventId : function (id) {    	
        		self.setLine1EventId(id);
        	},

        	getLine1EventId : function () {
        		return self.getLine1EventId();
        	},

        	setLine1OutboundEventId : function (id) {    	
        		self.setLine1OutboundEventId(id);
        	},

        	getLine1OutboundEventId : function () {
        		return self.getLine1OutboundEventId();
        	},

        	setLine2EventId : function (id) {
        		self.setLine2EventId(id);
        	},

        	getLine2EventId : function () {
        		return self.getLine2EventId();
        	},

        	setLine1Active : function (flag) {
        		self.setLine1Active(flag);
        	},

        	isLine1Active : function () {
        		return self.isLine1Active();
        	},

        	setLine2Active : function (flag) {
        		self.setLine2Active(flag);
        	},

        	isLine2Active : function () {
        		return self.isLine2Active();
        	},
        	
        	isRelogin : function () {
        		return self.isRelogin();
        	},
        	
        	setRelogin : function (flag) {
        		return self.setRelogin(flag);
        	}
  		
    	}
    }

    function retrieveAgentId() {
        var uuidReqData = {
                    type: 'GET',
                    dataType: 'json',
                    async: true,
                    // HZH: admin user must use a different registration path from regular agents to avoid 
                    // registration conflicts for vToolbar and notification window.
                    //url : subSocket.getUrl()+'services/agent/id?userAgent='+authUser,
 
                    url : subSocket.getUrl()+'services/adminAgent/id?userAgent='+authUser,
                    data: {},
                    error: function( jqXHR, textStatus, errorThrown ) {
                        console.log("Get Agent ID failed with status: "+jqXHR.status+" - message: "+textStatus);
                        console.log("Fallback too locally generated ");
                        var unqUsername = authUser+"_"+(new Date()).getTime();
                        subSocket.connect(unqUsername, function(){
                            if (atmoCallback) {
                                    atmoCallback(unqUsername);
                            }                                            	
                        }, handleMessage);
                    },
                    success : function ( data, textStatus, jqXHR ) {
                    	console.log("Retrieved: "+data.agentId);
                        var unqUsername = data.agentId;
                        subSocket.connect(unqUsername, function(){
                            if (atmoCallback) {
                                    atmoCallback(unqUsername);
                            }                                            	
                        }, handleMessage);
                    }
                };
        $.ajax(uuidReqData);
    }

    function handleMessage(json) {
        if (!logged && json.action==="ADMIN") {
            logged = true;
            login.remove();
            adminPanel.show();
            
            console.log("Current json: "+JSON.stringify(json));
            
            
            var data = JSON.parse(json.data);
            for(var key in data){
            	var userInfo = data[key];
            	addAgent(userInfo.author, userInfo.agentStatus, userInfo.callData.callStatus);
            }
            author = json.author;
            addMessage(json.author, "Logged in as administrator!", 'black', new Date());
        }else{
        	
            console.log("HandleMessage, json: "+JSON.stringify(json));

            var lineId = null;
            // If lineId is missing, set line ID to "1"
            if (json.callData.phoneLineId) {
            	lineId = json.callData.phoneLineId;    
            } else {
            	if (json.callData.ivrData) {
            		
            		var ivrDataArray = JSON.parse(json.callData.ivrData);
            		for (var key in ivrDataArray) {
                        if (key == "phoneLineId")
                        	lineId = ivrDataArray[key];
                    	}
            	}
            }

            if (lineId == null) {
            	lineId = "1";
            }
       	
            switch(json.action){
                case "ADD_AGENT":
                    addAgent(json.author, json.message);
                    break;
                case "REMOVE_AGENT":
                    removeAgent(json.author);
                    break;
                case "OUTBOUND_CALL":
                    addMessage(json.author, json.message, 'black', new Date());
                    
                    // Outbound Call Always from line 1"

                    if (json.callData.phoneLineId == null) {
                    	lineId = "1";    
                    }
                    
                    var callData =  JSON.parse(json.data); 
                    
                    var agentEventId = agentIdMap.get(json.author);
                    
                    if (agentEventId) {
                    	agentEventId.setLine1OutboundEventId(callData.SVCMCA_CALL_ID);
                    }
                    
                    if(json.agentStatus){
                        updateAgentStatus(json.author, json.agentStatus, json.callData.callStatus, lineId, callData.SVCMCA_CALL_ID);
                    }
                    showOutboundCall(json.author);
                    break;
                case "TEST_RING_SELF":
                	// HHUANG 161110 Process Test Ring
                	testRingPhone(json.author, json.callData);
                    break;
                case "GET_ACTIVE_ENGAGEMENTS":
                	// HHUANG 171221 Get Active Engagements
                	getAgentActiveEngagements(json.author);
                    break;
                case "PING_ADMIN":
                	// Agent ping admin, reply to agent
                	replyAgentPing(json.author);
                	break;
                case "PING_REPLY":
                	// Don't need to handle Ping Reply from admin
                	break;
                case "RING":
                case "ACCEPT":
                case "UPDATE_IVR_DATA":
                	break;
                case "END":
                	checkAndCloseOutboundConnect(json.author)
                    // avoid display empty messages in admin screen
                	//addMessage(json.author, json.action, 'black', new Date());
                	break;
                default:  
                    addMessage(json.author, json.message, 'black', new Date());
                	break;
                	
            }
            
            updateAgentStatus(json.author, json.agentStatus, json.callData.callStatus, lineId, json.callData.eventId);
        }        
    }
        
    function collectCallData(lineId){
    	var callData={};
        var ivrData = {}
        var engagementId = null
        var chatTestMode = null;
        
        $('.keyValuePair').each(function(index){        
    		   var key = $(this).find('.key')[0].value;
               var value = $(this).find('.value')[0].value;

               ivrData[key]=value;
               
               if (key === "SVCMCA_ENGAGEMENT_ID") {
            	   engagementId = value;
               }
               
               if (key === "SVCMCA_CHAT_TEST_MODE") {
            	   chatTestMode = value;
               }

       });
       
       ivrData[constants.attributes.wrapupSeconds]=$('#wrapupSeconds input[type="text"]').val();;
       ivrData[constants.attributes.notificationSeconds]=$('#notificationSeconds input[type="text"]').val();;
       
       callData[constants.attributes.ivrData]=JSON.stringify(ivrData);
       callData[constants.attributes.appClassification] = $('#appClassification').val();
       callData[constants.attributes.wrapupType] = $('#wrapupType option:selected').text();
       callData[constants.attributes.notificationType] = $('#notificationType option:selected').text();
       callData[constants.attributes.lookupObject] = $('#lookupObj').val();
       //callData[constants.attributes.channelType] = constants.channelType.phoneChannelType;
       callData[constants.attributes.channelType] = $('#channelType').val();
       callData[constants.attributes.phoneLineId] = lineId;
       
       if (callData[constants.attributes.channelType] === "ORA_SVC_CHAT" && engagementId != null) {
    	   callData[constants.attributes.eventId] = engagementId;
       }
       
       if (chatTestMode === "Y") {
    	   callData[constants.attributes.chatTestMode] = "Y"; 
       }

       return callData;
    	
     }
 
    function collectCompanionPanelData(){
        
       	var callData={};
                
        callData[constants.attributes.comPaneleMsg1] = $('#mgmtMsg1').val();
        callData[constants.attributes.comPaneleMsg2] = $('#mgmtMsg2').val();
        callData[constants.attributes.comPanelQName1] = $('#QueueName1').val();
        callData[constants.attributes.comPanelQName2] = $('#QueueName2').val();
        callData[constants.attributes.comPanelQName3] = $('#QueueName3').val();
        //callData[constants.attributes.averageWaitTime] = $('#avgWtm').val();
        //callData[constants.attributes.numberOfCalls] = $('#numberOfCalls').val();
        callData[constants.attributes.channelType] = constants.channelType.phoneChannelType;
        
        return callData;

    }

    function ringPhone(author, lineId){
        
        var callData = collectCallData(lineId);
        
        var channelType =  callData.channelType;
        
        if (channelType == constants.channelType.phoneChannelType) {
            console.log("Ring Phone: "+ JSON.stringify({ author: author, callData: callData, action : "RING" }));
        	subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "RING" }));
        } else if (channelType == constants.channelType.chatChannelType) {
        	console.log("Start Chat: "+ JSON.stringify({ author: author, callData: callData, action : "CHAT" }));
        	subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "CHAT" }));
        }
    }

    function hangupPhone(author, lineId){
    	
    	var eventId = null;

        var agentEventId = agentIdMap.get(author);
        
        if (agentEventId) {
        	
        	if (lineId == "2") {
        		eventId = agentEventId.getLine2EventId();
        	} else {
        		eventId = agentEventId.getLine1EventId();
        	}
        }
        
        var callData = {};
        
        callData[constants.attributes.phoneLineId] = lineId;
        
        if (eventId) {
        	callData[constants.attributes.eventId] = eventId;
        }
         
        console.log("Ring Phone: "+ JSON.stringify({ author: author, callData: callData, action : "REMOTE_END" }));

        subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "REMOTE_END" }));
    }
    
    function replyAgentPing(author) {
    	
    	var callData = {};
    	
    	//console.log("Ping Admin: "+ JSON.stringify({ author: author, callData: callData, action : "PING_REPLY" }));
        subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "PING_REPLY" }));
    	
    }


    function sendCompanionPanelMessage(author) {
        var callData = collectCompanionPanelData();
        subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "UPDATE_COM_PANEL" }));    	
    }

    // HHUANG 20161108
    function testRingPhone(author, callData){

        console.log("Test Ring Phone: "+ JSON.stringify({ author: author, callData: callData, action : "RING" }));

        //var callData = collectCallData();
        subSocket.publish(JSON.stringify({ author: author, callData: callData, action : "RING" }));
    }

    
    function generateRestURL(author, lineId){
    	var callData = collectCallData();
    	callData[constants.attributes.phoneLineId] = lineId;
    	var jsonData = JSON.stringify({ author: author, callData: callData, action : "RING" });
    	var url = liveConn.getUrl()+"services/calls/invoke?jsonData="+encodeURIComponent(jsonData);
    	
    	$('#'+author).find('#restUrl').attr('value',url);
    	$('#'+author).find('#restUrl').show();
    }
    
    function addPair(){
        var pairId = (new Date()).getTime().toString();
        $('#keyValue').append('<span class="keyValuePair" id="'+pairId+'"> '+
            '<span>Key</span><input type="text" list="tokens" class="key"/> '+ 
            '<span>Value</span><input type="text" class="value"/>'+
            '<input id="removePair" type="button" title="Remove Pair"/> <br/>'+
            '</span>');
        $('#'+pairId).find('#removePair').click(function() { removePair(pairId); });
    }

    function removePair(elementId){
        $('#'+elementId).remove();
    }

    function addMessage(author, message, color, datetime) {
        content.append('<p><span style="color:' + color + '">' + author + '</span> @ ' +
            + (datetime.getHours() < 10 ? '0' + datetime.getHours() : datetime.getHours()) + ':'
            + (datetime.getMinutes() < 10 ? '0' + datetime.getMinutes() : datetime.getMinutes())
            + ': ' + message + '</p>');
    }

    function addAgent(author, agentStatus, callStatus, callDirection){
    	    	
    	var oldAgentName = $("#"+escapeSpecialChars(author)).find('.agentName').text();
    	
    	if (oldAgentName === author) {
    		var agentEventId = agentIdMap.get(author);
    		if (agentEventId) {
    			agentEventId.setRelogin(true);
    		}
    		return;
    	}
    	
        removeAgent(author);
        var newAgent = $("#objects_to_clone").find("#agent_form").clone();
        newAgent.attr("id", escapeSpecialChars(author));
        newAgent.find('.agentName').text(author);
        newAgent.find('#sendCPMsgBtnId').click(function() { sendCompanionPanelMessage(author); });
        newAgent.find('#ringBtnId').click(function() { ringPhone(author, "1"); });
        newAgent.find('#ringBtnId2').click(function() { ringPhone(author, "2"); });
        newAgent.find('#hangupBtnId').click(function() { hangupPhone(author, "1"); });
        newAgent.find('#hangupBtnId').attr("disabled","disabled");
        newAgent.find('#hangupBtnId2').click(function() { hangupPhone(author, "2"); });
        newAgent.find('#hangupBtnId2').attr("disabled","disabled");
        //newAgent.find('#forcedRingBtnId').click(function() { ringPhone(author, "1"); });
        newAgent.find('#restUrlBtnId').click(function() { generateRestURL(author, "1"); });
        newAgent.find('#restUrlBtnId2').click(function() { generateRestURL(author, "2"); });
        newAgent.appendTo(agents);
        
        var agentLineEvents = new agentLineEventIds();
        agentIdMap.delete(author);
        agentIdMap.set(author, agentLineEvents);
        
        updateAgentStatus(author, agentStatus, callStatus, "1", null);
        updateAgentStatus(author, agentStatus, callStatus, "2", null);
        
        if (callDirection) {
            newAgent.find('.callIcon').show();
            if (callDirection === constants.direction.outbound_call) {
                newAgent.find('.callIcon').removeClass('inboundCall').addClass('outboundCall');
                showOutboundCall(author);
            } else if(callDirection === constants.direction.inbound_call) {
                newAgent.find('.callIcon').removeClass('outboundCall').addClass('inboundCall');
            } else {
                newAgent.find('.callIcon').hide();
            }
        } else {
            newAgent.find('.callIcon').hide();
        }
    }

    function removeAgent(author){
        $("#"+escapeSpecialChars(author)).remove();
        agentIdMap.delete(author);
    }

    function updateAgentStatus(agentId, agentStatus, callStatus, lineId, eventId){
    	
        console.log("Admin update Agent ID with ID " + agentId + " with status: "+agentStatus+" call status: "+ callStatus +
        		" LineId:" + lineId + " eventId: " + eventId);

    	try{
    	
            var agentSlotId = '#'+escapeSpecialChars(agentId);
            
            var agentEventId = agentIdMap.get(agentId);
            
            if (agentEventId) {
            	
            	if (agentEventId.isRelogin() == true) {
            		// Agent relogin from VB toolbar ignore first update
                    console.log("Admin update Agent ID Relogin with ID " + agentId + " with status: "+agentStatus+" call status: "+ callStatus +
                    		" LineId:" + lineId + " eventId: " + eventId);
                    agentEventId.setRelogin(false);
                    return;
            	}
            	
            	if (lineId == "1") {
            		agentEventId.setLine1EventId(eventId);
            	} else {
            		agentEventId.setLine2EventId(eventId);           		
            	}
            }
            
            if(agentStatus){
                $(agentSlotId).find('.agentStatus').text(agentStatus+"; ");
                
                if (lineId == "1") {
	                if(agentStatus === "Busy"){
	                    $(agentSlotId).find('#ringBtnId').attr("disabled","disabled");
	                    $(agentSlotId).find('#hangupBtnId').removeAttr("disabled","disabled");	                    
                    	agentEventId.setLine1Active(true);
	                }else{
	                    $(agentSlotId).find('#hangupBtnId').attr("disabled","disabled");
	                    $(agentSlotId).find('#ringBtnId').removeAttr("disabled","disabled");
	                    agentEventId.setLine1Active(false);
	                }
				} else if (lineId == "2") {
	                if(agentStatus === "Busy"){
	                    $(agentSlotId).find('#ringBtnId2').attr("disabled","disabled");
	                    $(agentSlotId).find('#hangupBtnId2').removeAttr("disabled","disabled");
	                    agentEventId.setLine2Active(true);
	                }else{
	                    $(agentSlotId).find('#hangupBtnId2').attr("disabled","disabled");
	                    $(agentSlotId).find('#ringBtnId2').removeAttr("disabled","disabled");
	                    agentEventId.setLine2Active(false);
	                }
				}
            }
    		
            if(callStatus){
            	if (lineId == "1") {
                     $(agentSlotId).find('#line1CallStatus').text("Call Status: "+callStatus);
            	} else if (lineId == "2") {
            		$(agentSlotId).find('#line2CallStatus').text("Call Status: "+callStatus);
            	}
            }
    	}catch(err){
    		console.log(err);
    	}
    }
    
    function getAgentActiveEngagements(agentId) {

        var activeCount = 0;
        var activeIds = {};
        var outData = {};
        var engagements = {};

        var agentEventId = agentIdMap.get(agentId);
        
        if (agentEventId) {
        	
        	if (agentEventId.isLine1Active()) {
        		activeCount++;
        		activeIds[activeCount] = agentEventId.getLine1EventId();
        	}

        	if (agentEventId.isLine2Active()) {
        		activeCount++;
        		activeIds[activeCount] = agentEventId.getLine2EventId();
        	}

        }
        
        outData['activeEngagements'] = activeCount;
        
        if (activeCount == 0) {
        	outData['engagements'] = null;
        } else if (activeCount == 1) {
        	engagements['eventId1'] = activeIds[1]; 
        	outData['engagements'] = engagements;
        } else if (activeCount == 2) {
        	engagements['eventId1'] = activeIds[1];       	
        	engagements['eventId2'] = activeIds[2];  
        	outData['engagements'] = engagements;
        }

        // Return out data
        //subSocket.publish(JSON.stringify({ author: agentId, outData: outData, action : "onGetActiveEngagements" }));
    }
    
    function getJsonParam(jsonString, param){
    	try{
    		var jsonObj = JSON.parse(jsonString);
    		return jsonObj[param];
    		
    	}catch(err){}
    }
    
    function showOutboundCall(agent) {
        var agentSlotId = '#'+escapeSpecialChars(agent);
        $(agentSlotId).find("#connBtn").click(function(){connectOutboundCall(agent);}).show();
        $(agentSlotId).find("#failBtn").click(function(){failOutboundCall(agent);}).show();
        $(agentSlotId).find("#accBtn").click(function(){acceptOutboundCall(agent);}).hide();
        $(agentSlotId).find("#rejBtn").click(function(){rejectOutboundCall(agent);}).hide();
        $(agentSlotId).find("#outbound").show();
    }
    
    function connectOutboundCall(agent) {
        var agentSlotId = '#'+escapeSpecialChars(agent);
        $(agentSlotId).find("#connBtn").hide();
        $(agentSlotId).find("#failBtn").hide();
        $(agentSlotId).find("#accBtn").show();
        $(agentSlotId).find("#rejBtn").show();
        
        var agentEventId = agentIdMap.get(agent);
        var eventId = agentEventId.getLine1OutboundEventId();
        
        var callData={};
        callData[constants.attributes.phoneLineId] = '1';
        callData[constants.attributes.eventId] = eventId;

        var msg = {};
        msg['appClassification'] = $('#appClassification').val();
        msg['callDrection'] = constants.direction.outbound_call;
        msg['wrapupType'] = $('#wrapupType option:selected').text();
        msg[constants.attributes.wrapupSeconds] = $('#wrapupSeconds input[type="text"]').val();
        msg['lookupObject'] = $('#lookupObj').val();
        msg['callData'] = callData;
        
        subSocket.publish(JSON.stringify({ author: agent, message: JSON.stringify(msg), action : "OUTBOUND_CONNECT" }));
    }

    function failOutboundCall(agent) {
        var agentSlotId = '#'+escapeSpecialChars(agent);
        $(agentSlotId).find("#outbound").hide();
        $(agentSlotId).find("#connBtn").off("click");
        $(agentSlotId).find("#failBtn").off("click");
        $(agentSlotId).find("#accBtn").off("click");
        $(agentSlotId).find("#rejBtn").off("click");
        
        var callData={};
        callData[constants.attributes.phoneLineId] = '1';

        subSocket.publish(JSON.stringify({ author: agent, callData: callData, message: JSON.stringify({code:'INVALID_NUMBER', phoneLineId:'1'}), action : "OUTBOUND_FAIL" }));
    }

    function checkAndCloseOutboundConnect(agent) {
        var agentSlotId = '#'+escapeSpecialChars(agent);
        $(agentSlotId).find("#connBtn").hide();
        $(agentSlotId).find("#failBtn").hide();
        $(agentSlotId).find("#accBtn").hide();
        $(agentSlotId).find("#rejBtn").hide();
     }

    function acceptOutboundCall(agent) {
        var agentSlotId = '#'+escapeSpecialChars(agent);
        $(agentSlotId).find("#outbound").hide();
        $(agentSlotId).find("#connBtn").off("click");
        $(agentSlotId).find("#failBtn").off("click");
        $(agentSlotId).find("#accBtn").off("click");
        $(agentSlotId).find("#rejBtn").off("click");
        
        var callData={};
        callData[constants.attributes.phoneLineId] = '1';

        subSocket.publish(JSON.stringify({ author: agent, callData: callData, message: JSON.stringify({code:'succes', phoneLineId:'1'}), action : "OUTBOUND_ACCEPT" }));        
    }

    function rejectOutboundCall(agent) {
        var agentSlotId = '#'+escapeSpecialChars(agent);
        $(agentSlotId).find("#outbound").hide();
        $(agentSlotId).find("#connBtn").off("click");
        $(agentSlotId).find("#failBtn").off("click");
        $(agentSlotId).find("#accBtn").off("click");
        $(agentSlotId).find("#rejBtn").off("click");
        
        var callData={};
        callData[constants.attributes.phoneLineId] = '1';

        subSocket.publish(JSON.stringify({ author: agent, callData: callData, message: JSON.stringify({code:'succes', phoneLineId:'1'}), action : "OUTBOUND_REJECT" }));        
    }

    function escapeSpecialChars(text){
            return text.replace(/[-[\]{}()*+?.,@\\^$|#\s]/g, "_");
    }

    return {
        initialize: function() {
            retrieveAgentId();
        },
        addKeyValuePair: function() {
            addPair();
        }
    };
});



