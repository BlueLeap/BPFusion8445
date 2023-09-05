var test_mode = false

function get_domain(){
    if(test_mode){
        return "hq-uccx.abc.inc"
    }
    // return "uccxp1.curtin.edu.au"
    return "devuccx.vu.edu.au"
}


function get_bosh_url(){
    if(test_mode){
        return `8445/http-bind`
    }
    return `7443/http-bind/`
}

function get_port(){
    return 8443
}

console.log("IN CONFIG" + get_domain() + get_bosh_url())
