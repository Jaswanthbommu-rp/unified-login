/*jshint esversion: 6 */

$(document).ready(function () {
        let settingsPages = $.sessionStorage.get('settingsPages'); 
        let company = $.sessionStorage.get('company');
        let { CompanyId, CompanyName } = company;
        let tileLink = '';
        if (CompanyId) {
            tileLink += `/setting/companysettings?CompanyId=${CompanyId}&CompanyName=${CompanyName}`;
        } else { // for customer user
            tileLink += `/setting/companysettings`;
        }

        let container = $('#companysetting');
        container.attr('href', tileLink);

        Promise.resolve(settingsPages)
            .then(settingsPages => {
                //Pull the Settings pages data
                return settingsPages ? settingsPages : userAPIService('GET', '', 'api/Pages', '');
            }).then(response => {
                return rpSaveSettingsPages(response);
            }).then(settingsPages => { //get landing page by id
                let landingPageId = settingsPages.find(page => page.name.toLowerCase() === 'settings').id;
                let url = `api/Pages/${landingPageId}?companyId=${CompanyId}`;

                return userAPIService('GET', '', url, '');
            }).then(page => {
                let { breadcrumb, name } = page;

               
               $('#raul-page-header-page').text(name); //set blue header in breadcrumps panel

                 // commented by chandrashekhar for update breadcrumbes with static data
               /* if (breadcrumb) {
                    populateSettingsBreadCrumbs(breadcrumb);
                }*/

            }).catch(err => {
                //add error handler  
                console.log(err);
            });






    // Recently Used Settings
    let recentSettingsUrl = "api/recently-used";
    let recentSettingValue;

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


