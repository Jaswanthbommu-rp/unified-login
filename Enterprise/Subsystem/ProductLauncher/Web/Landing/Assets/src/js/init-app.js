/*jshint esversion: 6 */

/*
* BEGIN: PAGE SETUP for non SPA
*/
let defineUserRole = () => {
    let orgId = $('body').attr('data-orgid');

    let userRole = (orgId === '-1') ? 'employee' : 'customer';

    $.sessionStorage.set('company', '');
    $.sessionStorage.set('userRole', userRole);
    
    //set company to sessionStorage
    let queries = document.location.search.substr(1).split('&');
    let company = {};

    queries.forEach(query => {
        let companyValues = query.split('=');

        company[companyValues[0]] = decodeURI(companyValues[1]);
    });

    let { CompanyId, CompanyName } = company;

    if (CompanyId && CompanyName) {
        CompanyName = decodeURIComponent(CompanyName);
        $.sessionStorage.set('company', company);

        let header = $('ui-header');
        header.attr('company-name', CompanyName); //set company name to header
    } else {
        // if there is no right to view settings pages - redirect user to home page
        window.location.href = `${window.location.origin}/#/`;
    }

     return userRole;
    }

  
//};

/*let showShellComponents = () => {
   // $('#raul-page-header-breadcrumbs').removeClass('hidden');
   // $('#raul-page-header-breadcrumbs-back').removeClass('d-none').addClass('d-inline-block');
    $('.icon-container').has('#raul-header-settings').removeClass('hidden');
};*/



let pageSetup = () => {
    //BEGIN: PUT WHATEVER YOU NEED IN THIS FUNCTION FOR INITIALIZATION OF EACH PAGE

    //Manage rights for Settings pages
    if (window.location.pathname.startsWith('/home/setting')) {
        let rightToView = ($('body').attr('data-viewright') === 'True') ? true : false;
        let userRole = defineUserRole();

        if (!rightToView) { // ???? for both employee and customer ????
            // if there is no right to view settings pages - redirect user to home page
            window.location.href = `${window.location.origin}/#/`;
        }

        $('#raul-left-navigation').find('.settings-gear').attr('href', '/home/setting' + window.location.search);
        /*if (userRole === 'customer') {
            showShellComponents();
        }*/
    }
   
    //CHECK IF THE USER SESSION HAS BEEN CREATED. IF NOT, CREATE IT AND THEN RUN THE SUCCESS FUNCTION ON THE PAGE LEVEL.
    rpUserSession('populatePageData');

    //REPOSITION THE ACTION BAR
    $(document).ready(function () {
        repositionActionFooter();
    });
    
    //END: PUT WHATEVER YOU NEED IN THIS FUNCTION FOR INITIALIZATION OF EACH PAGE

};
/*
* END: PAGE SETUP
*/

$(document).ready(function () {
    if (document.cookie.indexOf('access_token=') === -1) {
        window.location.href = '/home/signout';
    }

    pageSetup();

});