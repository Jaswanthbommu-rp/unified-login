/*jshint esversion: 6 */

console.log('common functions');

class Notification {
    constructor() {
        this.message = {
            type: 'success',
            preventDisplay: false,
            showCloseButton: false,
        };

        this.successUnifiedFlatform = {
            title: 'Unified Platform Setup settings saved successfully.',
            ttl: 2000,
        };

        this.successIdentityProvider = {
            title: 'Identity Provider settings saved successfully.',
            ttl: 2000,
        };

        this.sequenceError = {
            type: 'danger',
            showCloseButton: true,
            title: 'Field sequence number has been duplicated. please contact RealPage.',
        };
    }

    create(type) {
        return new RAUL.Notification(Object.assign(this.message, this[type]));
    }
}

let rpSaveSettingsPages = (response) => {
    $.sessionStorage.set('settingsPages', response);

    return response;
};

let populateSettingsBreadCrumbs = (breadcrumbs) => {
    let pages = $.sessionStorage.get('settingsPages');

    breadcrumbs = breadcrumbs.split('.');// split breadcrumbs since it is a string with id separate by dots

    let breadCrumbsData = breadcrumbs.map((item) => {
        return pages.find(page => page.id === +item);
    });

    let breadCrumbsContainer = $('#raul-page-header-breadcrumbs');
    let breadCrumbsMobileContainer = $('#raul-page-header-breadcrumbs-dropdown');
    let hiddenTemplate = $('.rp-breadcrumbs-template.hidden');
    let hiddenMobileTemplate = $('.raul-page-header-breadcrumbs-dropdown-breadcrumb.rp-breadcrumbs-template.hidden');

    if (breadCrumbsData.length) {
        breadCrumbsData.forEach( (item, i) => {
           
            let company = $.sessionStorage.get('company');
            let { CompanyId, CompanyName } = company;

            let breadCrumbTemplate = hiddenTemplate.clone();
            let breadCrumbMobileTemplate = hiddenMobileTemplate.clone();

            breadCrumbTemplate.removeClass('rp-breadcrumbs-template');
            breadCrumbMobileTemplate.removeClass('rp-breadcrumbs-template');

            let pageLink = item.pageUrl;
            if (CompanyId) {
                pageLink += `?CompanyId=${CompanyId}&CompanyName=${CompanyName}`;
            }

            breadCrumbTemplate.find('a').text(item.name).attr('href', `${pageLink}`);
            breadCrumbsContainer.append(breadCrumbTemplate);

            //append breadcrumb link for mobile
            breadCrumbMobileTemplate.text(item.name).attr('href', `${pageLink}`);
            breadCrumbsMobileContainer.append(breadCrumbMobileTemplate);
        });

        $('.rp-breadcrumbs-wrapper').removeClass('hidden');

        let userRole = $.sessionStorage.get('userRole');

        if (userRole === 'employee') { // for employee user - hide Realpage breadcrumb
            breadCrumbsContainer.find('.rp-breadcrumbs-wrapper.rp-breadcrumbs-template').addClass('hidden');
            breadCrumbsMobileContainer.find('.rp-breadcrumbs-wrapper.rp-breadcrumbs-template').addClass('hidden');
        }
    }

};



//responsive mobile navigation trigger

$(document).on('click', '.system-security-leftnav-toggle', function () {
    $('.system-security-leftnav').addClass('fix-leftnav');
});

$(document).on('click', '.system-security-leftnav-close', function () {
    $('.system-security-leftnav').removeClass('fix-leftnav');
});

function recentlyUsedSettings(data) {
    let url = 'api/recently-used';

    return userAPIService('POST', '', url, data);
}

// temporary logout panel trigger 
$(document).on('click', '#raul-header-user', function () {
    $('#raul-user-context').slideToggle();
})

$(document).on('click', '#raul-header-menu-button', function () {
    if ($(this).hasClass('activ')) {
        $(this).removeClass('activ');
        $('body').removeClass('raul-left-navigation-expanded');
    } else {
        $(this).addClass('activ');
        $('body').addClass('raul-left-navigation-expanded');
    }
});

$(document).click(function (e) {
    if (!$(e.target).is('#raul-header-user, #raul-header-user *')) {
        $('#raul-user-context').slideUp();
    }
});




// Showing Notification messages
var showNotification = function (type, title, message, url, duration) {
    RAUL.notifications.create();
    RAUL.notifications.open(type, title, message, url);
};