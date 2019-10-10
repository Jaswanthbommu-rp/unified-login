//User Authentication Service Handler for
//GET       https://my2dev.corp.realpage.com/api/dashboard
//GET       https://my2dev.corp.realpage.com/api/profiles/details
//GET       https://my2dev.corp.realpage.com/api/SideMenu/rights
//GET       http://my2dev.corp.realpage.com/api/organization/<userID>/products?mergePersonaAccess=true

var userAuthAPIService = function ( action, url, data, successFunc ){
    //console.log('successFunc', successFunc);
    var token = $.sessionStorage.get("access_token");
    var domain = $('body').attr('data-gb-uri'); 

    $.ajax({
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + token
        },
        crossDomain: true,
        type: action,
        url: domain + url,
        data: JSON.stringify(data),
        success: function(response){
            console.log('user-auth-api-services', response);
            var fn = window[successFunc];
            fn(response);
        },
        error: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("Status: " + textStatus);
            console.log("XMLHttpRequest", XMLHttpRequest);
        }
    });

};