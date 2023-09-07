

(function() {
	requirejs.config({
	    baseUrl: "js",
	    paths: {
	    	'jquery': 'jquery-2.0.3',
	    	'atmosphere': 'atmosphere'
	    },
	    shim: {
	        'jquery': {
	            exports: ['jQuery', '$']
	        }
	    }
	});
	
	requirejs(['jquery', 'adminMultiPage'], function($, admin) {
	// TODO
            
	    $(document).ready(function() {
                admin.initialize();                
                $('#addPairBtn').click(admin.addKeyValuePair);
	    });         

	    window.showSeconds = function(){
	    	
	    	var selected = $('#wrapupType option:selected').text();
	    	$('#wrapupSeconds input[type="text"]').val("");
	    	
	    	if(selected == "Timed Wrap Up"){
	    		$('#wrapupSeconds').show();
	    	}else{
	    		$('#wrapupSeconds').hide();
	    		
	    	}
	    	
	    }

	    window.showNotificationSeconds = function(){
	    	
	    	var selected = $('#notificationType option:selected').text();
	    	$('#notificationSeconds input[type="text"]').val("");
	    	
	    	if(selected == "Timed"){
	    		$('#notificationSeconds').show();
	    	}else{
	    		$('#notificationSeconds').hide();	    		
	    	}
	    	
	    }

	    console.log("###########  adminMain CALLED #########");
	});
})();


