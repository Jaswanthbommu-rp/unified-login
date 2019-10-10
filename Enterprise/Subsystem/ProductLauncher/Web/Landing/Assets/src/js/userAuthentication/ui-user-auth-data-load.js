/*jshint esversion: 6 */

//Verify that User Data is in the Session

var rpSuccessFunc;

var rpGetUserPersona = function(response){
    $.sessionStorage.set("userPersona",response);
};

var rpGetUserData = function(response){

    $.sessionStorage.set("profileResources",response.dashboardElements.resources);
    $.sessionStorage.set("profileDetails",response.dashboardElements.profileDetail);
    $.sessionStorage.set("realpageID",response.dashboardElements.profileDetail.userLogin.realPageId);
    $.sessionStorage.set("realpageOrganizationID", response.dashboardElements.profileDetail.organization[0].realPageId);

    //Get products for App switcher
    var realPageId = $.sessionStorage.get('realpageID');
    var url = `api/user/${realPageId}/products`;

    userAuthAPIService('GET', url, '', 'populateInitialData');
};

function buildAppSwitcher() {
    var products = $.sessionStorage.get('products');
    RAUL.AppSwitcher.fromOptions({ switcherData: products });

    //remove this event handler after disabled styles will be added in Raul
    $('.raul-header-app-switcher').on('click', function () {
        let disabledProducts = products.products.filter(product => product.status == 7);

        disabledProducts.forEach(product => {
            $('#raul-switcher-context').find('[href="'+ product.url + '"]').addClass('disabled');
        });
    });
    

};

function populateInitialData(productsResponse) {
    var products = productsResponse || $.sessionStorage.get("products"); 
    var profileDetails = $.sessionStorage.set("products", products);
    var profileDetails = $.sessionStorage.get("profileDetails");
    

    //BEGIN : Place the Username, Title, Company Name
    var username = profileDetails.firstName + " " + profileDetails.middleName + " " + profileDetails.lastName;
    var jobTitle = profileDetails.title;
    var header = $('ui-header');

    header.attr('user-name', username);
    header.attr('user-title', jobTitle);

    header.click('raul-header-user-angle', function () {
        header.find('[href="#user-settings"]').addClass('hidden');
    });
    

    buildAppSwitcher();
    
    checkLeftNavRights();

    //COMMENT this since company name is populated from init-app.js
    /*if (organizations.length > 0) {
        $('.rp-user-credential-organization').html(organizations[0].name);
    }*/

    //END : Place the Username, Title, Company Name

    var fn = window[rpSuccessFunc];
    if ($.isFunction(fn)) {
        fn();
    }
}

function rpReadCookie(name) {
    //console.log("TRYING TO READ THE COOKIE",document.cookie.split(';'));

    var nameEQ = encodeURIComponent(name) + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' '){
            c = c.substring(1, c.length);
        }
        if (c.indexOf(nameEQ) === 0){
            console.log("FOUND THE COOKIE",decodeURIComponent(c.substring(nameEQ.length, c.length)));
            return decodeURIComponent(c.substring(nameEQ.length, c.length));
        }
    }
    return null;
}

var rpUserSession = function(successFunc){

    rpSuccessFunc = successFunc;

    if(!$.sessionStorage.get("access_token") || $.sessionStorage.get("access_token") !== rpReadCookie('access_token')){
        //Get the access token before making User Authentication Calls
        $.sessionStorage.set("access_token",rpReadCookie('access_token'));

        //Pull the dashboard information for the logged in user
        userAuthAPIService('GET','api/dashboard/', '', 'rpGetUserData');

        //Pull the persona information for the logged in user
        userAuthAPIService('GET','api/persona/', '', 'rpGetUserPersona');

    } else {
        populateInitialData();// added for non SPA

        // uncomment for SPA
        //Call the Populate Page Data Function
        //var fn = window[rpSuccessFunc]; 

        //if ($.isFunction(fn)) { 
            //fn();
        //}
    }

};

let checkLeftNavRights = () => {
    let gbURI = $('body').attr('data-gb-uri');

    userAPIService('GET', gbURI, 'api/sidemenu/rights', '')
        .then(response => {
            let { data } = response;

            $('#raul-left-navigation-items li.hidden').each(function (item) {
                let rightTitle = $(this).attr('data-right');
                if (rightTitle === 'Settings') {
                    $(this).removeClass('hidden');
                    return;
                }

                let isInLeftNav = data.rights.some(right => right.toLowerCase() === rightTitle.toLowerCase());

                if (isInLeftNav) {
                    $(this).removeClass('hidden');
                }
            });
            
        }).catch(err => console.log('error', err));
};