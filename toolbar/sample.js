/**
 * Controller JavaScript to manage the functions of the page. The init function
 * is located near the bottom, which will bind actions to all buttons.
 */

var xmlToJSON = (function () {

    this.version = "1.3.4";

    var options = { // set up the default options
        mergeCDATA: true, // extract cdata and merge with text
        grokAttr: true, // convert truthy attributes to boolean, etc
        grokText: true, // convert truthy text/attr to boolean, etc
        normalize: true, // collapse multiple spaces to single space
        xmlns: true, // include namespaces as attribute in output
        namespaceKey: '_ns', // tag name for namespace objects
        textKey: '_text', // tag name for text nodes
        valueKey: '_value', // tag name for attribute values
        attrKey: '_attr', // tag for attr groups
        cdataKey: '_cdata', // tag for cdata nodes (ignored if mergeCDATA is true)
        attrsAsObject: true, // if false, key is used as prefix to name, set prefix to '' to merge children and attrs.
        stripAttrPrefix: true, // remove namespace prefixes from attributes
        stripElemPrefix: true, // for elements of same name in diff namespaces, you can enable namespaces and access the nskey property
        childrenAsArray: true // force children into arrays
    };

    var prefixMatch = new RegExp(/(?!xmlns)^.*:/);
    var trimMatch = new RegExp(/^\s+|\s+$/g);

    this.grokType = function (sValue) {
        if (/^\s*$/.test(sValue)) {
            return null;
        }
        if (/^(?:true|false)$/i.test(sValue)) {
            return sValue.toLowerCase() === "true";
        }
        if (isFinite(sValue)) {
            return parseFloat(sValue);
        }
        return sValue;
    };

    this.parseString = function (xmlString, opt) {
        return this.parseXML(this.stringToXML(xmlString), opt);
    }

    this.parseXML = function (oXMLParent, opt) {

        // initialize options
        for (var key in opt) {
            options[key] = opt[key];
        }

        var vResult = {},
            nLength = 0,
            sCollectedTxt = "";

        // parse namespace information
        if (options.xmlns && oXMLParent.namespaceURI) {
            vResult[options.namespaceKey] = oXMLParent.namespaceURI;
        }

        // parse attributes
        // using attributes property instead of hasAttributes method to support older browsers
        if (oXMLParent.attributes && oXMLParent.attributes.length > 0) {
            var vAttribs = {};

            for (nLength; nLength < oXMLParent.attributes.length; nLength++) {
                var oAttrib = oXMLParent.attributes.item(nLength);
                vContent = {};
                var attribName = '';

                if (options.stripAttrPrefix) {
                    attribName = oAttrib.name.replace(prefixMatch, '');

                } else {
                    attribName = oAttrib.name;
                }

                if (options.grokAttr) {
                    vContent[options.valueKey] = this.grokType(oAttrib.value.replace(trimMatch, ''));
                } else {
                    vContent[options.valueKey] = oAttrib.value.replace(trimMatch, '');
                }

                if (options.xmlns && oAttrib.namespaceURI) {
                    vContent[options.namespaceKey] = oAttrib.namespaceURI;
                }

                if (options.attrsAsObject) { // attributes with same local name must enable prefixes
                    vAttribs[attribName] = vContent;
                } else {
                    vResult[options.attrKey + attribName] = vContent;
                }
            }

            if (options.attrsAsObject) {
                vResult[options.attrKey] = vAttribs;
            } else { }
        }

        // iterate over the children
        if (oXMLParent.hasChildNodes()) {
            for (var oNode, sProp, vContent, nItem = 0; nItem < oXMLParent.childNodes.length; nItem++) {
                oNode = oXMLParent.childNodes.item(nItem);

                if (oNode.nodeType === 4) {
                    if (options.mergeCDATA) {
                        sCollectedTxt += oNode.nodeValue;
                    } else {
                        if (vResult.hasOwnProperty(options.cdataKey)) {
                            if (vResult[options.cdataKey].constructor !== Array) {
                                vResult[options.cdataKey] = [vResult[options.cdataKey]];
                            }
                            vResult[options.cdataKey].push(oNode.nodeValue);

                        } else {
                            if (options.childrenAsArray) {
                                vResult[options.cdataKey] = [];
                                vResult[options.cdataKey].push(oNode.nodeValue);
                            } else {
                                vResult[options.cdataKey] = oNode.nodeValue;
                            }
                        }
                    }
                } /* nodeType is "CDATASection" (4) */
                else if (oNode.nodeType === 3) {
                    sCollectedTxt += oNode.nodeValue;
                } /* nodeType is "Text" (3) */
                else if (oNode.nodeType === 1) { /* nodeType is "Element" (1) */

                    if (nLength === 0) {
                        vResult = {};
                    }

                    // using nodeName to support browser (IE) implementation with no 'localName' property
                    if (options.stripElemPrefix) {
                        sProp = oNode.nodeName.replace(prefixMatch, '');
                    } else {
                        sProp = oNode.nodeName;
                    }

                    vContent = xmlToJSON.parseXML(oNode);

                    if (vResult.hasOwnProperty(sProp)) {
                        if (vResult[sProp].constructor !== Array) {
                            vResult[sProp] = [vResult[sProp]];
                        }
                        vResult[sProp].push(vContent);

                    } else {
                        if (options.childrenAsArray) {
                            vResult[sProp] = [];
                            vResult[sProp].push(vContent);
                        } else {
                            vResult[sProp] = vContent;
                        }
                        nLength++;
                    }
                }
            }
        } else if (!sCollectedTxt) { // no children and no text, return null
            if (options.childrenAsArray) {
                vResult[options.textKey] = [];
                vResult[options.textKey].push(null);
            } else {
                vResult[options.textKey] = null;
            }
        }

        if (sCollectedTxt) {
            if (options.grokText) {
                var value = this.grokType(sCollectedTxt.replace(trimMatch, ''));
                if (value !== null && value !== undefined) {
                    vResult[options.textKey] = value;
                }
            } else if (options.normalize) {
                vResult[options.textKey] = sCollectedTxt.replace(trimMatch, '').replace(/\s+/g, " ");
            } else {
                vResult[options.textKey] = sCollectedTxt.replace(trimMatch, '');
            }
        }

        return vResult;
    }


    // Convert xmlDocument to a string
    // Returns null on failure
    this.xmlToString = function (xmlDoc) {
        try {
            var xmlString = xmlDoc.xml ? xmlDoc.xml : (new XMLSerializer()).serializeToString(xmlDoc);
            return xmlString;
        } catch (err) {
            return null;
        }
    }

    // Convert a string to XML Node Structure
    // Returns null on failure
    this.stringToXML = function (xmlString) {
        try {
            var xmlDoc = null;

            if (window.DOMParser) {

                var parser = new DOMParser();
                xmlDoc = parser.parseFromString(xmlString, "text/xml");

                return xmlDoc;
            } else {
                xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
                xmlDoc.async = false;
                xmlDoc.loadXML(xmlString);

                return xmlDoc;
            }
        } catch (e) {
            return null;
        }
    }

    return this;
}).call({});




function parseXMLToJSON(data) {
    result = xmlToJSON.parseString(data, { childrenAsArray: false });
    console.log("XML TO JSON = ", result)
    try {
        let dialogs = result["Update"]["data"]["dialogs"]
        let participants = dialogs['Dialog']['participants']['Participant']
        let participant1 = participants[0]
        let participant2 = ""
        if (participants.length > 1) {
            participant2 = participants[1]
        }
        let state = dialogs['Dialog']['state']
        console.log(state, "MMMMMMMMMMMMM")

        console.log("participants", participants)
        console.log("participant 1 ", participant1)
        console.log("participant 2", participant2)
        console.log("participants.length", participants.length)
        console.log("DIALOGS", dialogs)
    } catch (err) {
        let dialog = result["Update"]["data"]["dialog"]
        let state = dialog['state']
        console.log(state, "MMMMMMMMMMMMM")
        let participants = dialogs['Dialog']['participants']
        console.log("DIALOG", dialog)
    }




}

var

    //Private finesse object.
    _finesse,
    //Store agent information
    _username, _password, _extension, _domain, _login,

    //Private reference to JabberWerx eventing object.
    _jwClient;




var callactive = false
var call_active_callback = null
var call_dropped_callback = null
var call_held_callback = null
var call_resume_callback = null

var sessionToken1 = ""
var restAPIURL = ""
var acctID = ""

_domain = get_domain()


getSessionTokenAndRestAPIUrl()

function replaceAll(str, stringToReplace, replaceWith) {
    var result = str, index = 1;
    while (index > 0) {
        result = result.replace(stringToReplace, replaceWith);
        index = result.indexOf(stringToReplace);
    }
    return result;
}

/**
* Print text to console output.
*/
function print2Console(type, data) {
    console.log(type)
    console.log(data)
}

function onClientError(rsp) {
    //if XMPP events error out retry in some time
    console.log("ERROR " + rsp);
    console.log("set Timeout")
    setTimeout(function(){
        console.log("Timeout trigered")
        _eventConnect()
    },10000)
}

function getCallVariable2(xmlString){
    // Parse the XML string into an XML document object
    try{
        const parser = new DOMParser();
        const xmlDoc = parser.parseFromString(xmlString, "text/xml");
        console.log(xmlDoc, "xmlDoc")
    
        const callVariable2 = xmlDoc.getElementsByTagName("CallVariable")[1];
        const value = callVariable2.getElementsByTagName("value")[0].childNodes[0].nodeValue;
        console.log(callVariable2, "callVariable2")
        console.log(value, "value")
        return value
    }
    catch(err){
        return null
    }

}
//save a previous state for comparing
var previous_callstate = "NONE"
function parse(data) {
    console.log("IN PARSE FUNCTION")
    // parseXMLToJSON(data)
    try {

        

        var fromAddress = $(data).find("fromAddress").text();
        var toAddress = $(data).find("toAddress").text();

        console.log("fromAddress= ", fromAddress)
        console.log("toAddress= ", toAddress)


        //jquery object
        let xml_doc = $($.parseXML(data))
        //get source
        let src = xml_doc.find('source').first().text()
        console.log(src)
        console.log(src.indexOf('Dialog'))
        //Call related
        if (src.indexOf('Dialog') != -1) {
            //check call state
            let id = xml_doc.find('id').first().text()
            console.log(id)
            let evt = xml_doc.find('event').first().text()
            console.log("EVENT", evt)
            let callee_state = ""
            let call_state = ""
            let dialog_state = ""
            let agent_state = ""
            xml_doc.find('state').each(function (x) {
                
                if ($(this).parent().get(0).tagName == "Participant") {
                    console.log("participants >>", $(this).siblings('mediaAddress').first().text())
                    if ($(this).siblings('mediaAddress').first().text() == _extension) {
                        agent_state = $(this).text()
                    }
                    //there might be other participants who drop off - i think its finesse itself
                    else if ($(this).siblings('mediaAddress').first().text() == fromAddress) {
                        callee_state = $(this).text()
                    }
                    else if ($(this).siblings('mediaAddress').first().text() == toAddress) {
                        callee_state = $(this).text()
                    }
                    
                }
                //incoming call structure Dialogs -> Dialog
                else if ($(this).parent().get(0).tagName == "Dialog") {
                    call_state = $(this).text()
                }
                //active call structure dialog
                else if ($(this).parent().get(0).tagName == "dialog") {
                    call_state = $(this).text()
                }
            })
            console.log("STUDENT STATUS", callee_state)
            console.log("AGENT STATUS", agent_state)
            console.log("DIALOG STATUS", call_state)
            console.log("PREVIOUS dialog state",previous_callstate)
            console.log("EVENT",evt)
            console.log("CHECK OPEN ONCALL?")
            if (call_state == "ACTIVE" && evt == "PUT" && previous_callstate != call_state) {
               console.log("opening on call...")
               //all popup opening done from here
               openOnCallPopup()



            }
            if ((call_state.indexOf("ALERTING") > -1) && (fromAddress != _extension)) {
                incomingFinesseCall(fromAddress, data) // show incoming call popup and open workspace record
            }
            else if(agent_state == "WRAP_UP"){ // someone else dropped call
                removePopupWindows()
            }

            else if(evt == "DELETE"){ //  we dropped call
                showNotifications("Call disconnected")
                removePopupWindows()
            }
            // else if (call_state == "FAILED" && dialog_state == "FAILED" && evt == "PUT") { // call cut from callee
            //     // removePopupWindows()
            //     alert("call cut from callee")
                
            // }
            // else if (call_state == "ACTIVE" && dialog_state == "ACTIVE" && callee_state == "ACTIVE" && evt == "PUT") { // Recipient answers call
            //     alert("Recipient answers call")
                
            // }
            // else if (callee_state == "ACTIVE" && evt == "DELETE") { // call answered from callee
            //     // answerFinesseCall()
                
            // }
            previous_callstate = call_state
        }
        //Agent related
        else if (src.indexOf('/finesse/api/User') > -1) {
                //check agent state
                let state = xml_doc.find('state').first().text()
                let pending_state = xml_doc.find('pendingState').first().text()
                console.log("AGENT UPDATED")
                console.log(pending_state, state)
        }


        
    }
    catch (e) {
        console.log(e)
    }

}


/**
* Event handler that prints events to console.
*/
function _eventHandler(data) {


    data = data.selected.firstChild.data;
    console.log("MYEVENT = ", data);
    parse(data)


    console.log("CATEGORY = ", $(data).find("Dialog"))


    // try to get the callid
    var callid = $(data).find("id");


    let category = $(data).find("Dialog").text()

    // if (callid.text() !== "" && category!="") {
    //     // $("#field-call-control-callid").val(callid.text());
    //     localStorage.setItem("callid", callid.text())
    // }


    if (callid.text() !== "" && data.indexOf("<Dialog>") > -1) {
        // $("#field-call-control-callid").val(callid.text());
        // localStorage.setItem("callid", callid.text())
        setCallId(callid.text())
    }
    callid = null;
}

/**
* Connects to the BOSH connection. Any XMPP library or implementation can be
* used to connect, as long as it conforms to BOSH over XMPP specifications. In
* this case, we are using Cisco's Ajax XMPP library (aka JabberWerx). In order
* to make a cross-domain request to the XMPP server, a proxy should be
* configured to forward requests to the correct server.
*/
function _eventConnect() {
    if (window.jabberwerx) {
        var
            //Construct JID with username and domain.
            jid = _username + "@" + _domain,

            //Create JabbwerWerx object.
            _jwClient = new jabberwerx.Client("cisco");

        //Arguments to feed into the JabberWerx client on creation.
        let url1 = "https://" + _domain + ":" + get_bosh_url()

        jwArgs = {
            //Defines the BOSH path. Should match the path pattern in the proxy
            //so that it knows where to forward the BOSH request to.
            //https://uccxp1.curtin.edu.au:7443/http-bind/


            httpBindingURL: url1,
            //Calls this function callback on successful BOSH connection by the
            //JabberWerx library.
            errorCallback: onClientError,
            successCallback: function () {
                //Get the server generated resource ID to be used for subscriptions.
                _finesse.setResource(_jwClient.resourceName);
            }
        };


        jabberwerx._config.unsecureAllowed = true;
        //Bind invoker function to any events that is received. Only invoke
        //handler if XMPP message is in the specified structure.
        _jwClient.event("messageReceived").bindWhen(
            "event[xmlns='http://jabber.org/protocol/pubsub#event'] items item notification",
            _eventHandler);
        _jwClient.event("clientStatusChanged").bind(function (evt) {
            if (evt.data.next == jabberwerx.Client.status_connected) {
                // attempt to login the agent
                _finesse.signIn(_username, _extension, true, _signInHandler, _signInHandler);
            } else if (evt.data.next == jabberwerx.Client.status_disconnected) {
                // _finesse.signOut(_username, _extension, null, _signOutHandler, _signOutHandler);
            }
        });

        //Connect to BOSH connection.
        _jwClient.connect(jid, _password, jwArgs);
    } else {
        alert("CAXL library not found. Please download from http://developer.cisco.com/web/xmpp/resources")
    }
}

/**
* Disconnects from the BOSH connection.
*/
function _eventDisconnect() {
    if (_jwClient) {
        _jwClient.disconnect();
        _jwClient = null;
    }
}

/**
* Generic handler that prints response to console.
*/
function _handler(data, statusText, xhr) {

    // if (xhr) {
    //     console.log("RESPONSE", xhr.status);
    // } else {
    //     console.log("RESPONSE", data);
    // }
}

/**
* GetState handler that prints response to console.
*/
function _getStateHandler(data) {
    // console.log("RESPONSE", data.xml);

    console.log("GET AGENT STATE = ", data.xml)
}

/**
* Handler for the make call that will validate the response and automatically
* store the call id retrieve from the response data.
*/
function _makeCallHandler(data, statusText, xhr) {

    return true
}

/**
* Sign in handler. If successful, hide sign in forms, display actions, and
* connect to BOSH channel to receive events.
*/
function _signInHandler(data, statusText, xhr) {
    //if we get connection later on change agent status
    if (statusText == "success") {
        makeAgentReady()
        console.log(data, statusText, xhr)
        console.log("SIGN IN RESPONSE", xhr.status);

        //Ensure success.
        if (xhr.status === 202) {

            //@Don
            //alert("Login in Successfully")
            return true

        }
    }
    return false
}

function _signOutHandler(data, statusText, xhr) {

    //Ensure success.
    if (xhr.status === 202) {
        // Disconnect from getting events
        _eventDisconnect();

        // Clean up the values of objects
        _username = null;
        _password = null;
        _extension = null;

        // Clean up the Finesse object
        _finesse = null;

    }
}

/**
* Init function. Wait until document is ready before binding actions to buttons
* on the page.
*/

function removePopupWindows() {
    // get current popup windows //

    ORACLE_SERVICE_CLOUD.extension_loader.load("CUSTOM_APP_ID", "1")
        .then(function (extensionProvider) {
            extensionProvider.registerUserInterfaceExtension(function (IUserInterfaceContext) {
                IUserInterfaceContext.getPopupWindowContext().then(function (IPopupWindowContext) {
                    var popupWindows;
                    IPopupWindowContext.getCurrentPopupWindows().then(function (currentPopupWindows) {
                        popupWindows = currentPopupWindows;
                        // Perform some operations on popupWindows.

                        console.log(popupWindows, "popupwindows")

                        popupWindows.forEach(function (item) {
                            item.close() // close the popupwindow
                        })
                    });
                });
            });
        });

    // get current popup windows //
}

function removeIncomingCallandOncallPopUps() {
    ORACLE_SERVICE_CLOUD.extension_loader.load("CUSTOM_APP_ID", "1")
        .then(function (extensionProvider) {
            extensionProvider.registerUserInterfaceExtension(function (IUserInterfaceContext) {
                IUserInterfaceContext.getPopupWindowContext().then(function (IPopupWindowContext) {
                    var popupWindows;
                    IPopupWindowContext.getCurrentPopupWindows().then(function (currentPopupWindows) {
                        popupWindows = currentPopupWindows;
                        // Perform some operations on popupWindows.

                        console.log(popupWindows, "popupwindows")

                        popupWindows.forEach(function (item) {
                            console.log(item.title, "TITLE")
                            if (item.title == "Call" || item.title == "Ongoing Call" ){
                                item.close() // close the popupwindow
                            }
                            // if (item.title == "Call"){
                            //     item.close() // close the popupwindow
                            // }
                            
                        })
                    });
                });
            });
        });

    // get current popup windows //
}




function showNotifications(label) {
    // show notification //

    ORACLE_SERVICE_CLOUD.extension_loader.load("CUSTOM_APP_ID", "1")
        .then(function (IExtensionProvider) {
            IExtensionProvider.registerUserInterfaceExtension(function (IUserInterfaceContext) {
                IUserInterfaceContext.getNotificationContext().then(function (INotificationContext) {
                    var notificationConfig = INotificationContext.createNotificationConfig();
                    var action1 = notificationConfig.createAction();
                    action1.setLabel(label);
                    console.log(action1.getLabel());
                    notificationConfig.addAction(action1);
                    INotificationContext.showNotification(notificationConfig);
                });
            });
        });

    // show notification //
}

function getSessionTokenAndRestAPIUrl() {
    /**
     * get sesssion token and rest api url
     */

    // Getting IExtensionGlobalContext object for accountId 


    ORACLE_SERVICE_CLOUD.extension_loader.load('Global Extension')
        .then(function (IExtensionProvider) {
            IExtensionProvider.getGlobalContext().then(function (IExtensionGlobalContext) {
                acctID = IExtensionGlobalContext.getAccountId();
                restAPIURL = IExtensionGlobalContext.getInterfaceServiceUrl("REST");
                console.log(restAPIURL,"restAPIURL")


                interfaceURL = IExtensionGlobalContext.getInterfaceUrl();
                IExtensionGlobalContext.getSessionToken().then(function (sessionToken) {
                    sessionToken1 = sessionToken;
                    console.log("Got token", sessionToken1)
                });

            });
        });
    return [sessionToken1, restAPIURL]
}

function saveCustomObjects(customObject) {
    /**
     * https://docs.oracle.com/en/cloud/saas/service/18b/cxsvc/c_osvc_custom_objects.html
     * 
     * @param customObject: json body of the custom object to be saved in oracle
     */
    // return new Promise(function (resolve, reject) {
    //     try {
    //         console.log("IN SAVE CUSTOM OBJECT")
    //         getAgentObject().then(function (agentObject) {
    //             console.log("updating existing")
    //             let xhr = new XMLHttpRequest();
    //             let url = restAPIURL + "/connect/v1.3/BLbrowser.finesse/" + agentObject.id;
    //             xhr.open("PATCH", url, true);
    //             xhr.setRequestHeader('Authorization', 'Session ' + sessionToken1);
    //             xhr.setRequestHeader('Content-Type', 'application/json');
    //             xhr.onreadystatechange = function () {
    //                 if (xhr.readyState === 4) {
    //                     if (xhr.status === 200) {
    //                         console.log("FINESSE OBJECT SAVED")
    //                         resolve()
    //                     }
    //                     else {
    //                         reject(xhr.status)
    //                     }
    //                 }
    //             };
    //             let data = JSON.stringify(customObject);
    //             xhr.send(data);

    //         }).catch(e => {
    //             console.log("CREATING NEW")
    //             console.log(restAPIURL, acctID, sessionToken1)
    //             let xhr = new XMLHttpRequest();
    //             let url = restAPIURL + "/connect/v1.3/BLbrowser.finesse";
    //             xhr.open("POST", url, true);
    //             xhr.setRequestHeader('Authorization', 'Session ' + sessionToken1);
    //             xhr.setRequestHeader('Content-Type', 'application/json');
    //             xhr.onreadystatechange = function () {
    //                 if (xhr.readyState === 4) {
    //                     if (xhr.status === 201) {
    //                         console.log("FINESSE OBJECT SAVED")
    //                         resolve()
    //                     }
    //                     else {
    //                         reject(xhr.status)
    //                     }
    //                 }
    //             };
    //             let data = JSON.stringify(customObject);
    //             xhr.send(data);

    //         })
    //     }
    //     catch (e) {
    //         reject()
    //     }
    // });

  return new Promise((resolve, reject) => {
    resolve(true)
  })

}


function getAgentObject() {

    // let xhr = new XMLHttpRequest();
    // let url = restAPIURL + "/connect/v1.3/queryResults/?query=select agentid, password, extension from BLbrowser.finesse where acct="+acctID;
    // xhr.open("GET", url, true);
    // xhr.setRequestHeader('Authorization', 'Session ' + sessionToken1);
    // xhr.setRequestHeader('Content-Type', 'application/json');
    // xhr.onreadystatechange = function () {
    //     console.log(xhr,"full list ")
    //     if (xhr.readyState === 4){
    //         if(xhr.status === 200) {
    //             let respon = JSON.parse(xhr.responseText)
    //             console.log("got data>>>>>>>>>>>>>>>>>>>>>>>>>>>.")
    //             console.log(respon)
    //         }
    //     }
    // };
    // xhr.send(null);



    // return new Promise(function (resolve, reject) {
    //     try {


    //         let xhr = new XMLHttpRequest();
    //         let url = restAPIURL + "/connect/v1.3/queryResults/?query=select id,agentid, password, extension from BLbrowser.finesse where acct=" + acctID;
    //         xhr.open("GET", url, true);
    //         xhr.setRequestHeader('Authorization', 'Session ' + sessionToken1);
    //         xhr.setRequestHeader('Content-Type', 'application/json');
    //         xhr.onreadystatechange = function () {
    //             if (xhr.readyState === 4) {
    //                 if (xhr.status === 200) {
    //                     let respon = JSON.parse(xhr.responseText)
    //                     let agt = {}
    //                     if (respon.items[0].count > 0) {
    //                         for (let i = 0; i < respon.items[0].columnNames.length; i++) {
    //                             agt[respon.items[0].columnNames[i]] = respon.items[0].rows[0][i]
    //                         }

    //                     }
    //                     if (agt.id == undefined) {
    //                         console.log("NO MATCH")
    //                         reject("No rows")
    //                     }
    //                     else {
    //                         resolve(agt)
    //                     }
    //                 }
    //                 else {
    //                     reject(xhr.status)
    //                 }
    //             }
    //         };
    //         xhr.send(null);
    //     } catch (e) {
    //         reject(e)
    //     }
    // });
    return new Promise((resolve, reject) => {
      resolve(localStorage.getItem('agentObject'))
    })

}


function setAgentObject(agentObject) {
    localStorage.setItem('agentObject', 
      JSON.stringify({ 
        "signedin": true, 
        "username": agentObject.agentid, 
        "password": agentObject.password,
        "extension": agentObject.extension
      })
    )
    return saveCustomObjects(agentObject)
}

function removeAgentObject() {

    console.log("IN REMOVE CUSTOM OBJECT")

    // getAgentObject().then(function (agentObject) {
    //     let xhr = new XMLHttpRequest();
    //     let url = restAPIURL + "/connect/v1.3/BLbrowser.finesse/" + agentObject.id;
    //     xhr.open("DELETE", url, true);
    //     xhr.setRequestHeader('Authorization', 'Session ' + sessionToken1);
    //     xhr.setRequestHeader('Content-Type', 'application/json');
    //     xhr.onreadystatechange = function () {
    //         if (xhr.readyState === 4 && xhr.status === 200) {
    //             console.log("FINESSE OBJECT DELETED")
    //         }
    //     };
    //     xhr.send(null);

    // })

}

function getCallId() {
    let callId = localStorage.getItem("callid")
    return callId
}

function setCallId(callId) {
    localStorage.setItem("callid", callId)
}




function makeAgentReady() {
    var newState = "READY"
    if (newState) {
        _finesse.changeState(_username, newState, null, _handler, _handler);
    }
}

function initFinesseObject() {
    getAgentObject().then(function (agentObject) {
        _username = agentObject?.agentid ? agentObject['agentid'] : undefined
        _password = agentObject?.password ? agentObject['password'] : undefined
        _extension = agentObject?.extension ? agentObject['extension'] : undefined
        _finesse = new Finesse(_username, _password)
        _eventConnect();
        makeAgentReady()
    })
}

function setupForOngoingCall(dropcallback, holdcallback, resumecallback) {
    getAgentObject().then(function (agentObject) {
        _username = agentObject?.agentid ? agentObject['agentid'] : undefined
        _password = agentObject?.password ? agentObject['password'] : undefined
        _extension = agentObject?.extension ? agentObject['extension'] : undefined
        _finesse = new Finesse(_username, _password)
        //_eventConnect() - only one listener in cti bar context
        _callactive = true
        call_dropped_callback = dropcallback
        call_held_callback = holdcallback
        call_resume_callback = resumecallback
    })

}
function getAgentState() {
    getAgentObject().then(function (agentObject) {
        _username = agentObject?.agentid ? agentObject['agentid'] : "Agent002"
        _password = agentObject?.password ? agentObject['password'] : "ciscopsdt"
        _finesse = new Finesse(_username, _password)
        console.log("IN getAgentState()")



        var newState = "NOT_READY"
        if (newState) {
            _finesse.changeState(_username, newState, null, _handler, _handler);
        }


        _finesse.getState(_username, _getStateHandler, _handler);
    })
}


function isAgentSignedIn() {
    console.log("CHECKING IF AGENT SIGNED IN")
    let agentObject = localStorage.getItem('agentObject')
    agentObject = JSON.parse(agentObject)
    let signedin = agentObject?.signedin ? agentObject['signedin'] : false

    return signedin
}

function agentSignIn() {

let _username = "blueleap2";
let _password = "13579";
let _extension = "3752";

console.log("agentSignIn CALLED")

let signin_callback = function (data, statusText, xhr) {
let stat = _signInHandler(data, statusText, xhr)
if (stat) {
let agentObject = {
"extension": _extension,
"agentid": _username,
"password": _password,
"ACCT": acctID
}
console.log(acctID)
console.log(agentObject)
}
else {
console.log("SIGNIN FAILED")
failure()
}
}
//Create Finesse object and sign in user. On successful sign in, a
//handler will be invoked to present more API options in UI.
console.log("SHOULD ATTEMPT LOGIN HERE")
_finesse = new Finesse(_username, _password);
_finesse.signIn(_username, _extension, true, signin_callback, signin_callback);

}





function agentSignOut(callback) {
    let handler = function (data, statusText, xhr) {
        _signOutHandler(data, statusText, xhr)
        localStorage.removeItem("agentObject")
        removeAgentObject()
        callback()
    }
    getAgentObject().then(function (agentObject) {
        //removing intifiness object which also creates listenr
        // _username = agentObject?.agentid ? agentObject['agentid'] : undefined
        // _password = agentObject?.password ? agentObject['password'] : undefined
        // _extension = agentObject?.extension ? agentObject['extension'] : undefined
      console.log('agentObject=============>',agentObject)
      agent =JSON.parse(agentObject)
      console.log('agent ======>', agent)
      _username = agent.username
      _password = agent.password
      _extension = agent.extension

      _finesse = new Finesse(_username, _password)        
      _finesse.signOut(_username, _extension, null, handler, handler);
    })
}

function makeFinesseCall(dialNum, _username, _password, _extension, success, failure) {
    console.log("MAKING CALL TO " + dialNum)
    // getAgentObject().then(function (agentObject) {
        // _username = agentObject?.agentid ? agentObject['agentid'] : undefined
        // _password = agentObject?.password ? agentObject['password'] : undefined
        // _extension = agentObject?.extension ? agentObject['extension'] : undefined


		// dialNum = "0399193708";
		// _username = "blueleap1";
		// _password = "13579";
		// _extension = "3751"

		_finesse = new Finesse(_username, _password)
		console.log('finesse ======>', _finesse)
		console.log('dialNum ======>', dialNum)
		//_eventConnect();
		console.log("makeFinesseCall SET THE CALLBACK")
		call_active_callback = success


		let callsuccess = function (data, statusText, xhr) {
			console.log('callsuccess')
				_makeCallHandler(data, statusText, xhr)
				success()

		}
		let callerror = function (data, statusText, xhr) {
				_handler(data, statusText, xhr)
				failure()

		}
		_finesse.makeCall(dialNum, _extension, callsuccess, callerror);





    // })

}

function dropCallSuccessHandler(data, statusText, xhr) {
    console.log("dropCallSuccessHandler", statusText, xhr.responseText)
    // removePopupWindows()
    // localStorage.removeItem("fromAddress")
    // showNotifications("Drop call ok")

    getCurrentDialogs() // from this function, if no current dialog is happening, it will remove incoming and on call windows
}

function dropCallHandler(data, statusText, xhr) {
    // removePopupWindows()

    //error case
    showNotifications("Drop call failed")
}

function dropFinesseCall() {

    if (!isAgentSignedIn()) {

        showNotifications('Agent should be Signed In')

        return
    }


    let callId = getCallId()
    console.log("DROP CALL ID " + callId)
    getAgentObject().then(function (agentObject) {
        _username = agentObject?.agentid ? agentObject['agentid'] : undefined
        _password = agentObject?.password ? agentObject['password'] : undefined
        _extension = agentObject?.extension ? agentObject['extension'] : undefined
        _finesse = new Finesse(_username, _password)

        _finesse.dropCall(callId, _extension, dropCallSuccessHandler, dropCallHandler);
    })

}


function retrieveFinesseCall() {
    let callId = getCallId()
    console.log("DROP CALL ID " + callId)

    getAgentObject().then(function (agentObject) {
        _username = agentObject?.agentid ? agentObject['agentid'] : undefined
        _password = agentObject?.password ? agentObject['password'] : undefined
        _extension = agentObject?.extension ? agentObject['extension'] : undefined
        _finesse = new Finesse(_username, _password)
        _finesse.retrieveCall(callId, _extension, _handler, _handler);
    })
}


function holdFinesseCall(success, failure) {
    let callId = getCallId()
    console.log("HOLD CALL ID " + callId)
    getAgentObject().then(function (agentObject) {
        _username = agentObject?.agentid ? agentObject['agentid'] : undefined
        _password = agentObject?.password ? agentObject['password'] : undefined
        _extension = agentObject?.extension ? agentObject['extension'] : undefined
        _finesse = new Finesse(_username, _password)
        _finesse.holdCall(callId, _extension,
            function (data, statusText, xhr) {
                _handler(data, statusText, xhr);
                success()
            },
            function (data, statusText, xhr) {
                _handler(data, statusText, xhr);
                failure();
            });
    })
}

function getContactIdFromNumber(workspaceRecord, fromAddress){

    fromAddress = String(fromAddress).slice(1)

    // return new Promise((resolve, reject)=>{
    //     getRequest(
    //         restAPIURL 
    //         +
    //         "/connect/v1.3/queryResults?query=select%20id%20from%20contacts%20where%20phones.rawnumber=" + "'918075236372'" ,
    //          "Session " + sessionToken1).then(data => {
    //             //var data = JSON.parse(data);
    //             console.log(data, "JSON RESPONSE FROM SELECT QUERY")
    //             console.log(data.items[0].rows[0][0])
    //             resolve(data.items[0].rows[0][0])
        
                    
    //         });

            
    // })

    getRequest(
        restAPIURL 
        +
        "/connect/v1.3/queryResults?query=select%20id%20,%20Name.First,Name.Last%20from%20contacts%20where%20phones.rawnumber=" + `'${fromAddress}'` ,
         "Session " + sessionToken1).then(data => {
            //var data = JSON.parse(data);
            console.log(data, "JSON RESPONSE FROM SELECT QUERY")
            console.log(data.items[0].rows[0][0])
            let contactId = data.items[0].rows[0][0]
            try {
                let name = data.items[0].rows[0][1] +" "+ data.items[0].rows[0][2]
                localStorage.setItem("fromName", name)
            }
            catch(err){
                console.log(err)
            }
            workspaceRecord.editWorkspaceRecord('Contact', contactId);
    
                
        });

}


function getContactIdFromStudentId(workspaceRecord, studentId){


    getRequest(
        restAPIURL 
        +
        "/connect/v1.3/queryResults?query=select%20id%20,%20Name.First,Name.Last%20from%20contacts%20where%20login=" + `'${studentId}'` ,
        //"/connect/v1.3/queryResults?query=select%20id%20from%20contacts%20where%20login=" + `'${studentId}'` ,
         "Session " + sessionToken1).then(data => {
            //var data = JSON.parse(data);
            console.log(data, "JSON RESPONSE FROM SELECT QUERY getContactIdFromStudentId")
            console.log(data.items[0].rows[0][0])
            let contactId = data.items[0].rows[0][0]
            try {
                let name = data.items[0].rows[0][1] +" "+ data.items[0].rows[0][2]
                localStorage.setItem("fromName", name)
            }
            catch(err){
                console.log(err)
            }
            workspaceRecord.editWorkspaceRecord('Contact', contactId);
    
                
        }).catch(err=>{
            console.log(err, "ERROR IN getContactIdFromStudentId")
        });

}

function incomingFinesseCall(fromAddress, xmlEvent) {


    localStorage.setItem("fromAddress", fromAddress)
    console.log("SETTING ADDRESS IN LOCAL STORAGE")
    // render page
    console.log("incomingFinesseCall CALLED")

    var studentId = getCallVariable2(xmlEvent)
    // var studentId = "11000033"

        // open workspace for the contact id
    
    console.log("fromAddress in EDIT WORKSPACE RECORD", fromAddress)
    ORACLE_SERVICE_CLOUD.extension_loader.load("CUSTOM_APP_ID", "1")
        .then(function (extensionProvider) {
            extensionProvider.registerWorkspaceExtension(function (workspaceRecord) {
                if (studentId){
                    getContactIdFromStudentId(workspaceRecord, studentId)
                }
                else{
                    getContactIdFromNumber(workspaceRecord, fromAddress)
                }
                
                
            });
        });



    ORACLE_SERVICE_CLOUD.extension_loader.load("CUSTOM_APP_ID", "1")
        .then(function (extensionProvider) {
            extensionProvider.registerUserInterfaceExtension(function (IUserInterfaceContext) {
                IUserInterfaceContext.getPopupWindowContext().then(function (IPopupWindowContext) {

                    //create only if missing
                    let popupWindows;
                    let found = false
                    IPopupWindowContext.getCurrentPopupWindows().then(function(currentPopupWindows)
                        {
                        popupWindows = currentPopupWindows;
                        console.log(popupWindows)
                        popupWindows.forEach(function (item) {
                            if(item.id == ""){
                                found = true
                            }
                        })
                        if (!found){
                            let incomingCallPopupWindow = IPopupWindowContext.createPopupWindow('IncomingCallPopupWindow');
                            incomingCallPopupWindow.setHeight('510px');
                            incomingCallPopupWindow.setWidth('670px');
                            incomingCallPopupWindow.setTitle("Call");
                            incomingCallPopupWindow.setContentUrl("incoming-call.html");
                            incomingCallPopupWindow.render();

                        }
                    });

                    
                });
            });
        });





    // ORACLE_SERVICE_CLOUD.extension_loader.load("CUSTOM_APP_ID", "1")
    // .then(function (extensionProvider) {
    //     extensionProvider.registerWorkspaceExtension(function (workspaceRecord) {
    //         var recordId = workspaceRecord.getWorkspaceRecordId();
    //         var currentWorkspaceObj = workspaceRecord.getCurrentWorkspace();

    //         console.log(recordId, currentWorkspaceObj , "GGGGGGGGGGGGGGGG")
    //     });
    // });

    function callbackFunction(workspaceRecord) {
        // Perform some operation after opening the workspace record.
        // alert("edit workspace record")
    }

}


function answerFinesseCallSuccessHandler() {

    //close incoming call popup


    // show on call page, when call is successfully is anwering

    ORACLE_SERVICE_CLOUD.extension_loader.load("CUSTOM_APP_ID", "1")
        .then(function (extensionProvider) {
            extensionProvider.registerUserInterfaceExtension(function (IUserInterfaceContext) {
                IUserInterfaceContext.getPopupWindowContext().then(function (IPopupWindowContext) {


                    let popupWindows;
                    IPopupWindowContext.getCurrentPopupWindows().then(function (currentPopupWindows) {
                        popupWindows = currentPopupWindows;
                        // Perform some operations on popupWindows.

                        console.log(popupWindows, "popupwindows")

                        popupWindows.forEach(function (item) {
                            if(item.id == "IncomingCallPopupWindow")
                                item.close() // close the popupwindow
                        })
                    });



                    // let OnCallPopupwindow = IPopupWindowContext.createPopupWindow('OnCallPopupwindow');
                    // OnCallPopupwindow.setHeight('510px');
                    // OnCallPopupwindow.setWidth('670px');
                    // OnCallPopupwindow.setTitle("Ongoing Call");
                    // OnCallPopupwindow.setContentUrl("oncall.html");
                    // OnCallPopupwindow.render();
                });
            });
        });

}


function openOnCallPopup(){

    ORACLE_SERVICE_CLOUD.extension_loader.load("CUSTOM_APP_ID", "1")
    .then(function (extensionProvider) {
        extensionProvider.registerUserInterfaceExtension(function (IUserInterfaceContext) {
            IUserInterfaceContext.getPopupWindowContext().then(function (IPopupWindowContext) {

                let OnCallPopupwindow = IPopupWindowContext.createPopupWindow('OnCallPopupwindow');
                OnCallPopupwindow.setHeight('510px');
                OnCallPopupwindow.setWidth('670px');
                OnCallPopupwindow.setTitle("Ongoing Call");
                OnCallPopupwindow.setContentUrl("oncall.html");
                OnCallPopupwindow.render();
            });
        });
    });

}

function answerFinesseCallFailureHandler() {

    showNotifications("Answer call failed")
}

function answerFinesseCall() {

    if (!isAgentSignedIn()) {

        showNotifications('Agent should be Signed In')

        return
    }


    let callId = getCallId()
    console.log("ANSWER CALL ID " + callId)
        
    getAgentObject().then(function (agentObject) {
        _username = agentObject?.agentid ? agentObject['agentid'] : undefined
        _password = agentObject?.password ? agentObject['password'] : undefined
        _extension = agentObject?.extension ? agentObject['extension'] : undefined
        _finesse = new Finesse(_username, _password)
        _finesse.answerCall(callId, _extension, answerFinesseCallSuccessHandler, answerFinesseCallFailureHandler);
    });
}

function signinbtnclick() {
    alert("btn clicked")
}

setInterval(function () {
    //makeAgentReady() - causing unnecessary errors
    getCurrentDialogs()
}, 10000)


function getDialogId(xmlString) {
    const parser = new DOMParser();
    const xmlDoc = parser.parseFromString(xmlString, 'text/xml');
    const dialogId = xmlDoc.getElementsByTagName('id')[0].textContent;
    return dialogId
}


function getAgentDialogsCallback(data, statusText, xhr) {
    try {
        //console.log("\n\ngetAgentDialogsCallback\n\n", xhr.responseText)
        let callId = getDialogId(xhr.responseText)
        setCallId(callId)
    }
    catch (err) {
        //console.log(err)
        //CLOSING WINDOW ALSO DONE BASED ON EVENTS
        // removeIncomingCallandOncallPopUps() // if any error occured while fetching ongoing dialogs
        // //then no active call is happening at the moment
        // localStorage.removeItem("fromAddress")
        // localStorage.removeItem("fromName")
    }

}

function getCurrentDialogs() {
    let dialogs = _finesse.getAgentDialogs(_username, getAgentDialogsCallback, getAgentDialogsCallback)
}

const windowFeatures = "left=100,top=100,width=500,height=400";
const openDialpad = window.open("https://bluleap-mediabar.web.app/dialpad.html", "mozillaWindow", windowFeatures);
