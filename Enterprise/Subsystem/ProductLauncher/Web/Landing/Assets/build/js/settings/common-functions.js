'use strict';

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

/*jshint esversion: 6 */

console.log('common functions');

var Notification = function () {
    function Notification() {
        _classCallCheck(this, Notification);

        this.message = {
            type: 'success',
            preventDisplay: false,
            showCloseButton: false
        };

        this.successUnifiedFlatform = {
            title: 'Unified Platform Setup settings saved successfully.',
            ttl: 2000
        };

        this.successIdentityProvider = {
            title: 'Identity Provider settings saved successfully.',
            ttl: 2000
        };

        this.sequenceError = {
            type: 'danger',
            showCloseButton: true,
            title: 'Field sequence number has been duplicated. please contact RealPage.'
        };
    }

    _createClass(Notification, [{
        key: 'create',
        value: function create(type) {
            return new RAUL.Notification(Object.assign(this.message, this[type]));
        }
    }]);

    return Notification;
}();

var rpSaveSettingsPages = function rpSaveSettingsPages(response) {
    $.sessionStorage.set('settingsPages', response);

    return response;
};

var populateSettingsBreadCrumbs = function populateSettingsBreadCrumbs(breadcrumbs) {
    var pages = $.sessionStorage.get('settingsPages');

    breadcrumbs = breadcrumbs.split('.'); // split breadcrumbs since it is a string with id separate by dots

    var breadCrumbsData = breadcrumbs.map(function (item) {
        return pages.find(function (page) {
            return page.id === +item;
        });
    });

    var breadCrumbsContainer = $('#raul-page-header-breadcrumbs');
    var breadCrumbsMobileContainer = $('#raul-page-header-breadcrumbs-dropdown');
    var hiddenTemplate = $('.rp-breadcrumbs-template.hidden');
    var hiddenMobileTemplate = $('.raul-page-header-breadcrumbs-dropdown-breadcrumb.rp-breadcrumbs-template.hidden');

    if (breadCrumbsData.length) {
        breadCrumbsData.forEach(function (item, i) {

            var company = $.sessionStorage.get('company');
            var CompanyId = company.CompanyId,
                CompanyName = company.CompanyName;


            var breadCrumbTemplate = hiddenTemplate.clone();
            var breadCrumbMobileTemplate = hiddenMobileTemplate.clone();

            breadCrumbTemplate.removeClass('rp-breadcrumbs-template');
            breadCrumbMobileTemplate.removeClass('rp-breadcrumbs-template');

            var pageLink = item.pageUrl;
            if (CompanyId) {
                pageLink += '?CompanyId=' + CompanyId + '&CompanyName=' + CompanyName;
            }

            breadCrumbTemplate.find('a').text(item.name).attr('href', '' + pageLink);
            breadCrumbsContainer.append(breadCrumbTemplate);

            //append breadcrumb link for mobile
            breadCrumbMobileTemplate.text(item.name).attr('href', '' + pageLink);
            breadCrumbsMobileContainer.append(breadCrumbMobileTemplate);
        });

        $('.rp-breadcrumbs-wrapper').removeClass('hidden');

        var userRole = $.sessionStorage.get('userRole');

        if (userRole === 'employee') {
            // for employee user - hide Realpage breadcrumb
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
    var url = 'api/recently-used';

    return userAPIService('POST', '', url, data);
}

// temporary logout panel trigger 
$(document).on('click', '#raul-header-user', function () {
    $('#raul-user-context').slideToggle();
});

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
var showNotification = function showNotification(type, title, message, url, duration) {
    RAUL.notifications.create();
    RAUL.notifications.open(type, title, message, url);
};