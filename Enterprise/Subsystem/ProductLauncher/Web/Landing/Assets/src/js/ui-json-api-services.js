//JSON API Service Handler for
//GET       /mailboxes
//POST      /mailboxes
//GET       /mailboxes/id
//PUT       /mailboxes/id
//DELETE    /mailboxes/id

var jsonAPIService = function (action, server, url, data, successFunc ){
    //console.log('successFunc', successFunc);
    var defaultServer = 'settings-domain';
    if(server !== ''){
        defaultServer = server;
    }
    $.ajax({
        headers: {
            "Accept" : "application/vnd.api+json",
            "Content-Type": "application/vnd.api+json",
            "companyId" : 1
        },
        type: action,
        url: $("meta[name="+ defaultServer +"]").attr('content') + url,
        data: data,
        success: function(response){
            console.log('json-api-services', response);
            var fn = window[successFunc];
            fn(response);
        },
        error: function(XMLHttpRequest, textStatus, errorThrown) {
            console.log("Status: " + textStatus);
            alert("Error: " + errorThrown);
        }
    });

};


// Create the proper data object to be consumed by the JSON API

var createDataObject = function (element){

    var data = {};

    $(element).find('input, textarea, select').each(function(x, field) {
        if(field.name && field.type === "radio"){
            if(field.checked){
                data[field.name]=field.value;
            }
        } else if(field.name && field.type === "checkbox"){
            if (field.name.indexOf('[]')>0) {
                if (!$.isArray(data[field.name])) {
                    data[field.name]=[];
                }
                if(field.checked){
                    data[field.name].push(field.value);
                }
            } else {
                if(field.checked){
                    data[field.name]=field.value;
                } else {
                    data[field.name]=false;
                }
            }
        } else if (field.name){
            if (field.name.indexOf('[]')>0) {
                if (!$.isArray(data[field.name])) {
                    data[field.name]=[];
                }
                data[field.name].push(field.value);
            } else {
                data[field.name]=field.value;
            }
        }
    });

    return data;

};
