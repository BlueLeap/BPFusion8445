define(['atmosphere'], function(atmosphere) {
    
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
});