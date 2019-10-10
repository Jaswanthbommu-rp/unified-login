'use strict';

/*jshint esversion: 6 */

$(document).ready(function () {
    var settingsPages = $.sessionStorage.get('settingsPages');
    var company = $.sessionStorage.get('company');
    var CompanyId = company.CompanyId,
        CompanyName = company.CompanyName;

    var tileLink = '';
    if (CompanyId) {
        tileLink += '/setting/companysettings?CompanyId=' + CompanyId + '&CompanyName=' + CompanyName;
    } else {
        // for customer user
        tileLink += '/setting/companysettings';
    }

    var container = $('#companysetting');
    container.attr('href', tileLink);

    Promise.resolve(settingsPages).then(function (settingsPages) {
        //Pull the Settings pages data
        return settingsPages ? settingsPages : userAPIService('GET', '', 'api/Pages', '');
    }).then(function (response) {
        return rpSaveSettingsPages(response);
    }).then(function (settingsPages) {
        //get landing page by id
        var landingPageId = settingsPages.find(function (page) {
            return page.name.toLowerCase() === 'settings';
        }).id;
        var url = 'api/Pages/' + landingPageId + '?companyId=' + CompanyId;

        return userAPIService('GET', '', url, '');
    }).then(function (page) {
        var breadcrumb = page.breadcrumb,
            name = page.name;


        $('#raul-page-header-page').text(name); //set blue header in breadcrumps panel

        // commented by chandrashekhar for update breadcrumbes with static data
        /* if (breadcrumb) {
             populateSettingsBreadCrumbs(breadcrumb);
         }*/
    }).catch(function (err) {
        //add error handler  
        console.log(err);
    });

    // Recently Used Settings
    var recentSettingsUrl = "api/recently-used";
    var recentSettingValue = void 0;

    return Promise.resolve(userAPIService('GET', '', recentSettingsUrl, '')).then(function (response) {
        if (response.length === 0) {
            $('.recent-used-settings').addClass('hidden');
        }

        var url = '/setting/systemsecurity' + window.location.search;

        for (var i = 0; i < response.length; i++) {

            switch (response[i].name) {
                case 'customfield':
                    recentSettingValue = 'Custom User Fields';
                    break;
                case 'UnifiedPlatformSetup':
                    recentSettingValue = 'Unified Platform Setup';
                    break;
                case 'IdentityProviderType':
                    recentSettingValue = 'Identity Provider';
                    break;
                case 'PartialPage':
                    if ($('#IdentityProviderType').length > 0) {
                        recentSettingValue = '';
                    } else {
                        recentSettingValue = 'Identity Provider';
                    }
                    break;
            }

            if (recentSettingValue != "") {
                $('.recent-search-items').append('<li><a href= ' + url + '&selectedTab=' + response[i].name + '  id="' + response[i].name + '">' + recentSettingValue + '</a></li>');
            }

            recentSettingValue = "";
        }
    });
});