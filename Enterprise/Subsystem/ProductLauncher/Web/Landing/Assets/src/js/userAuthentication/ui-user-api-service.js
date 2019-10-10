/*jshint esversion: 6 */

let userAPIService = (action, domainName, url, data) => {
    return new Promise((resolve, reject) => {
        let token = $.sessionStorage.get("access_token");
            
        if (!domainName) { //default for Red book API 
            domainName = $('body').attr('data-rb-uri'); 
        }

        $.ajax({
            headers: {
                "Content-Type": "application/json",
                "Authorization": "Bearer " + token
            },
            crossDomain: true,
            type: action,
            url: domainName + url,
            data: JSON.stringify(data),
            success: function (response) {
                console.log('user-auth-api-services', response);
                resolve(response);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
                console.log("XMLHttpRequest", XMLHttpRequest);

                if (XMLHttpRequest.responseJSON) {
                    resolve(XMLHttpRequest.responseJSON);
                } else {
                    resolve(XMLHttpRequest);
                }
                
            }
        });

    });
};
