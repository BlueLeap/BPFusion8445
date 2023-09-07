<!DOCTYPE html>



<html>
<head>
    <meta charset="utf-8">
    <title>Admin Page</title>
    <link rel="stylesheet" href="css/adminMulti.css" type="text/css" />
    <script type="text/javascript" data-main="js/bootAdminMulti" src="js/require-2.1.12.js"></script>
    <!--script type="text/javascript" data-main="js/bootAdmin" src="js/require-2.1.12.js"></script-->
</head>
<body>
    <div id="login">
       <input id="input" type="text" style="display: none;" value="admin" />
    </div>
    <div id="adminPanel">
        <div id="content"></div>
        <div id="callConfig">
            <p class="callConfiguration">Call configuration - Build 1.0-SNAPSHOT_b69_20220302.111221</p><br/>
            <p class="formField">
                <label for="appClassification">Application-Classification</label>
                <input id="appClassification"  type="text" list="appClassificationTokens"/>
                <datalist id="appClassificationTokens">
                    <option>ORA_SERVICE</option>
                    <option>ORA_SALES</option>
                    <option>ORA_HRHD</option>
                </datalist>
            </p>
            <p class="formField">
                <label for="channelType">Channel Type</label>
                <select id="channelType">
                    <option>ORA_SVC_PHONE</option>
                    <option>ORA_SVC_CHAT</option>
                </select>
            </p>
            <p class="formField">
                <label for="notificationType">Notification Type</label>
                <select id="notificationType" onchange="showNotificationSeconds()">
                    <option>Unlimited</option>
                    <option>Timed</option>
                 </select>
                <span id="notificationSeconds" style="display:none;"><input type="text" style="width: 20px;"/> Seconds</span>
            </p>
            <p class="formField">
                <label for="wrapupType">Wrapup Type</label>
                <select id="wrapupType" onchange="showSeconds()">
                    <option>Unlimited Wrap Up</option>
                    <option>Timed Wrap Up</option>
                    <option>No Wrap Up</option>
                </select>
                <span id="wrapupSeconds" style="display:none;"><input type="text" style="width: 20px;"/> Seconds</span>
            </p>
            <p class="formField">
                <label for="lookupObj">Lookup object</label>
                <input id="lookupObj" type="text" list="bos" />
            </p>
        </div>
        <div id="phoneCompanion">
            <p class="companionPanelMsg">Companion Panel Messages</p><br/>
            <p class="formFieldMsg">
                <label for="mgmtMsg1">Mgmt Msg 1</label>
                <input id="mgmtMsg1" type="text" value="Make sure you have requested holiday vacation before the deadline"/>
             </p>
            <p class="formFieldMsg">
                <label for="mgmtMsg2">Mgmt Msg 2</label>
                <input id="mgmtMsg2" type="text" value="Special of the week: $45 off all Triple Play Packages. See more..."/>
            </p>
            <div class="agentDataLine agentDataLineStype">
            <p class="formFieldQName">
                <label for="QueueName1">Queue Name: 1</label>
                <input id="QueueName1" type="text" value="Computers"/>
            </p>
            <p class="formFieldQName">
                <label for="QueueName2">2</label>
                <input id="QueueName2" type="text" value="Peripherals"/>
            </p>
            <p class="formFieldQName">
                <label for="QueueName3">3</label>
                <input id="QueueName3" type="text" value="Storage"/>
            </p>
            </div>
        </div>
        <div id="agents">
            <p class="connectedAgents">Connected Agents:</p>
        </div>
        <fieldset id="keyValue">
            <legend>IVR Data</legend>
            <input id="addPairBtn" type="button" value="Add Pair"/>
            <br/><br/>
            <span class="keyValuePair">
                <span>Key</span>
                <input id="SVCMCA_ANI" type="text" list="tokens" class="key" value="SVCMCA_ANI"/> 
                <span>Value</span>
                <input type="text" class="value" />
                <br/>
            </span>
        </fieldset>
    </div>
    <datalist id="tokens">
        <option value="SVCMCA_ANI">
        <option value="SVCMCA_SR_NUM">
        <option value="SVCMCA_EMAIL">
        <option value="SVCMCA_ORG_NAME">
        <option value="SVCMCA_ENGAGEMENT_ID">
        <option value="SVCMCA_UWO_ID">
        <option value="SVCMCA_INTERACTION_ID">
        <option value="SVCMCA_CHAT_TEST_MODE">
    </datalist>
    <datalist id="bos">
        <option value="ServiceRequest">
        <option value="Contact">
        <option value="Account">
    </datalist>                        
    <div id="objects_to_clone" style="display:none;">
        <div id="agent_form" class="agentData">
            <div class="callIcon inboundCall"></div>
            <div id="agent_data">
              <div class="agentDataLine agentDataLineStype">
                <span class="agentLabel"> Agent: </span>
                <span class="agentName"></span>
                <span class="agentStatus"></span>
                <button id="sendCPMsgBtnId" type="button" style="float:right; height: 20px; width: 160px">Set Companion Messages</button>
              </div>
              <div class="agentDataLine agentDataLineStype">
                <span class="agentLineName">Line 1: </span> 
                <span id="line1CallStatus" class="callStatus"></span>
                <button id="restUrlBtnId" type="button" style="float:right; height:20px; width: 90px"> REST Ring Url </button>
                <button id="hangupBtnId" type="button" style="float:right; height:20px; width: 70px"> Hang Up </button>
                <button id="ringBtnId" type="button" style="float:right; height:20px; width: 50px"> Ring </button>
                <!--button id="forcedRingBtnId" type="button" style="float:right; display:none">Forced Ring</button-->
               </div>
	           <div class="agentDataLine agentDataLineStype">
                <span class="agentLineName">Line 2: </span> 
                <span id="line2CallStatus" class="callStatus"></span>
                <button id="restUrlBtnId2" type="button" style="float:right; height:20px; width: 90px">REST Ring Url</button>
                <button id="hangupBtnId2" type="button" style="float:right; height:20px; width: 70px">Hang Up</button>
                <button id="ringBtnId2" type="button" style="float:right; height:20px; width: 50px">Ring</button>
                 <!--button id="forcedRingBtnId2" type="button" style="float:right; display:none">Forced Ring</button-->
               </div>
                <div id="skipCheckboxes" class="skipCheckboxes">
                  <input value="SVCMCA_BYPASS_CUSTOMER_VERIFICATION" title="Skip Customer Validation" type="checkbox" /> 
                  <input value="SVCMCA_BYPASS_IDENTIFY_CONTACT" title="Skip Identify Contact" type="checkbox" /> 
                  <input value="SVCMCA_BYPASS_AUTO_SCREEN_POP" title="Skip Screen Pop" type="checkbox" />
                </div> 
                <input type="text" id="restUrl" class="restUrl"/>
            </div>
            <div id="outbound" style="display:none;">
                <button id="connBtn" type="button" >Connect</button>
                <button id="failBtn" type="button" >Fail</button>
                <button id="accBtn" type="button" >Accept</button>
                <button id="rejBtn" type="button" >Decline</button>
            </div>
        </div>        
    </div>
</body>
</html>