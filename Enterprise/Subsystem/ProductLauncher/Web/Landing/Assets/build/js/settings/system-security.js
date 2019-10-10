'use strict';

/*jshint esversion: 6 */

var buildLeftMenu = function buildLeftMenu(leftNavigationLinks) {
    var leftMenu = $('.systerm-security-tabs');
    var content = $('.system-security-content');

    leftNavigationLinks.sort(function (a, b) {
        return a.sequence - b.sequence;
    }).forEach(function (link, i) {
        var menuItem = $('.system-security-tab-template.hidden').clone();
        var menuItemLink = menuItem.find('a');
        var linkIdValue = link.linkText.split(' ').join('-').toLowerCase();

        menuItemLink.attr('data-tabname', linkIdValue);
        menuItemLink.text(link.linkText);
        if (link.sequence === 1) {
            menuItem.addClass('active');
            content.find('#' + linkIdValue).addClass('active');
        }

        leftMenu.append(menuItem);
        menuItem.removeClass('hidden');
    });
};

var company;
var companyId;
var companyName;
var setCompanySettingUrl;
var rightToManage = $('body').attr('data-manageright') === 'True' ? true : false;
$(document).ready(function () {
    var settingsPages = $.sessionStorage.get('settingsPages');
    var company = $.sessionStorage.get('company');
    var CompanyId = company.CompanyId,
        CompanyName = company.CompanyName;

    var notification = new Notification();

    Promise.resolve(settingsPages).then(function (settingsPages) {
        //Pull the Settings pages data
        return settingsPages ? settingsPages : userAPIService('GET', '', 'api/Pages', '');
    }).then(function (response) {
        return rpSaveSettingsPages(response);
    }).then(function (settingsPages) {
        //get landing page by id
        var pageId = settingsPages.find(function (page) {
            return page.name === 'System & Security';
        }).id;
        var url = 'api/Pages/' + pageId + '?companyId=' + CompanyId;

        return userAPIService('GET', '', url, '');
    }).then(function (page) {
        var breadcrumb = page.breadcrumb,
            name = page.name,
            leftNavigationLinks = page.leftNavigationLinks;


        $('.system-security-content').show();
        $('#raul-page-header-page').text(name); //set blue header in breadcrumps panel

        //set breadCrumbs
        /*if (breadcrumb) {
            populateSettingsBreadCrumbs(breadcrumb);
        }*/

        //build left menu
        buildLeftMenu(leftNavigationLinks);

        setCurrentTab();
    });
    unifiedPlatformSetup();

    //EVENT Linteners

    $(document).on('click', '.systerm-security-tabs li a', function () {
        activeTab('[data-tabname=' + $(this).attr('data-tabname') + ']');
    });

    $('#unified-platform-setup-wrap').on('submit', function (event) {
        event.preventDefault();
        var validDaysField = $('#password-validate-field');
        var passMinCharField = $('#password-min-characters');
        var passExpireField = $('#password-expire-field');
        var userTimeoutField = $('#user-timeout');
        var uniformPlatformsetupUrl = "api/UnifiedPlatformSetup";
        var AccessToggleValue = function AccessToggleValue() {
            return $('#companyAccessToggle').is(':checked');
        };
        var linkValidDays = validDaysField.val();

        var minCharacterForRealPagePwd = Math.round(parseFloat(passMinCharField.val()));
        var noOfDaysBeforeCurrentPwdExpire = Math.round(parseFloat(passExpireField.val()));
        var unifiedPlatformUserTimeOut = Math.round(parseFloat(userTimeoutField.val()));

        passMinCharField.val(minCharacterForRealPagePwd);
        passExpireField.val(noOfDaysBeforeCurrentPwdExpire);
        userTimeoutField.val(unifiedPlatformUserTimeOut);

        var upsReqDto = {
            "blockCompanyProductLearningPortal": AccessToggleValue(),
            "tempPwdNewUserLinkValidDays": linkValidDays,
            companyId: companyId,
            minCharacterForRealPagePwd: minCharacterForRealPagePwd,
            noOfDaysBeforeCurrentPwdExpire: noOfDaysBeforeCurrentPwdExpire,
            unifiedPlatformUserTimeOut: unifiedPlatformUserTimeOut
        };

        if ($('#password-validate-field').is(':visible') && $('#unified-platform-setup-wrap').parsley().isValid()) {
            var TempPasswordValue = $('#password-validate-field').val();

            if (!tempPwdDecimalValidate()) {
                var roundDaysValue = Math.round(parseFloat(linkValidDays) * 100) / 100;

                upsReqDto["tempPwdNewUserLinkValidDays"] = roundDaysValue;
                validDaysField.val(roundDaysValue);
            }

            userAPIService('PUT', '', uniformPlatformsetupUrl, upsReqDto).then(function (response) {
                if (response.error) {
                    //Error

                } else {
                    $('.raul-notification').remove();

                    notification.create('successUnifiedFlatform');
                }
            });
            return; //??discuss code duplication
        }
        /*   let uniformPlatformsetupUrl = "api/UnifiedPlatformSetup";
           let AccessToggleValue = () => $('#companyAccessToggle').is(':checked');
           let linkValidDays = $('#password-validate-field').val();
           
           var upsReqDto = {
               "blockCompanyProductLearningPortal": AccessToggleValue(),
               "tempPwdNewUserLinkValidDays": linkValidDays,
               "companyId": companyId
           };*/

        /*if (parseInt($('#password-validate-field').val()) === 0 || $('#password-validate-field').val() === '' || $('#password-validate-field').val() > 21) {
            if ($('.error-msg').is(':visible')) { $('.error-msg').hide(); }
            return false;
        } else {
            if (!tempPwdDecimalValidate()) {
                let roundDaysValue = Math.round(parseFloat(linkValidDays) * 100) / 100;
                  upsReqDto["tempPwdNewUserLinkValidDays"] = roundDaysValue;
                validDaysField.val(roundDaysValue);
            }
              userAPIService('PUT', '', uniformPlatformsetupUrl, upsReqDto).then(function (response) {
                console.log('Data Saved Successfully!');
                
                $('.raul-notification').remove();
                notification.create('successUnifiedFlatform');
                $('.error').hide();
                return response;
            });
        }*/
    });
});

function activeTab(tab) {
    if (tab == "[data-tabname=custom-user-fields]") {
        customFieldIntialize();
    } else if (tab == "[data-tabname=identity-provider]") {
        buildIdentityProviderPage();
    } else if (tab == "[data - tabname= unified-platform-setup]") {
        unifiedPlatformSetup();
    }

    var tabname = $('a' + tab).attr('data-tabname');
    $('.system-security-content > .tab-pane').hide();
    $('.systerm-security-tabs li').removeClass('active');
    $(tab).closest('li').addClass('active');
    $('#' + tabname).fadeIn();
}

function setCurrentTab() {
    var queries1 = document.location.search.substr(1).split('&');
    queries1.forEach(function (query) {
        var companyValues = query.split('=');

        switch (companyValues[1]) {
            case 'IdentityProviderType':
                activeTab('[data-tabname=identity-provider]');
                break;
            case 'customfield':
                activeTab('[data-tabname=custom-user-fields]');
                break;
            case 'UnifiedPlatformSetup':
                activeTab('[data-tabname=unified-platform-setup]');
                break;
        }
    });
}

function unifiedPlatformSetup() {
    company = $.sessionStorage.get('company');
    companyId = company.CompanyId;
    companyName = company.CompanyName;

    setCompanySettingUrl = "/setting/companysettings?CompanyId=" + companyId + "&CompanyName=" + companyName;

    var uniformPlatformsetupUrl = "api/UnifiedPlatformSetup?companyId=" + companyId;

    return Promise.resolve(userAPIService('GET', '', uniformPlatformsetupUrl, '')).then(function (response) {
        populateUnifiedPlatformData(response);
        var data = { "settingid": response[0].id };
        recentlyUsedSettings(data);
    });
}

$(document).on('click', '#unified-platform-Cancel', function () {
    location.href = setCompanySettingUrl;
});

function populateUnifiedPlatformData(response) {
    var _response$ = response[0],
        blockCompanyProductLearningPortal = _response$.blockCompanyProductLearningPortal,
        tempPwdNewUserLinkValidDays = _response$.tempPwdNewUserLinkValidDays,
        noOfDaysBeforeCurrentPwdExpire = _response$.noOfDaysBeforeCurrentPwdExpire,
        minCharacterForRealPagePwd = _response$.minCharacterForRealPagePwd,
        unifiedPlatformUserTimeOut = _response$.unifiedPlatformUserTimeOut;


    var validDaysField = $('#password-validate-field');
    var passMinCharField = $('#password-min-characters');
    var passExpireField = $('#password-expire-field');
    var userTimeoutField = $('#user-timeout');

    //if there are no values from API, set default
    minCharacterForRealPagePwd = minCharacterForRealPagePwd || passMinCharField.val();
    noOfDaysBeforeCurrentPwdExpire = noOfDaysBeforeCurrentPwdExpire || passExpireField.val();
    unifiedPlatformUserTimeOut = unifiedPlatformUserTimeOut || userTimeoutField.val();

    if (!rightToManage) {
        validDaysField.hide();
        passMinCharField.hide();
        passExpireField.hide();
        userTimeoutField.hide();
        $('#companyAccessToggle').prop('checked', blockCompanyProductLearningPortal).attr("disabled", true);
        validDaysField.after("<div class='validDays'>" + tempPwdNewUserLinkValidDays + "</div>");
        passMinCharField.after('<div class=\'validDays\'>' + minCharacterForRealPagePwd + '</div>');
        passExpireField.after('<div class=\'validDays\'>' + noOfDaysBeforeCurrentPwdExpire + '</div>');
        userTimeoutField.after('<div class=\'validDays\'>' + unifiedPlatformUserTimeOut + '</div>');
        $('.i-p-action-btns').hide();
    } else {
        $('#companyAccessToggle').prop('checked', blockCompanyProductLearningPortal);
        validDaysField.val(tempPwdNewUserLinkValidDays);
        passMinCharField.val(minCharacterForRealPagePwd);
        passExpireField.val(noOfDaysBeforeCurrentPwdExpire);
        userTimeoutField.val(unifiedPlatformUserTimeOut);
    }
}

// Temparory Password field Decimal values check
var tempPwdDecimalValidate = function tempPwdDecimalValidate() {
    var num = $('#password-validate-field').val();
    if (num % 1 !== 0) {

        var decimalVal = $('#password-validate-field').val().split('.');
        if (decimalVal[1]) {
            var Decimallength = decimalVal[1].toString().length;
            if (Decimallength > 2) return false;
        }
    }
    return true;
};