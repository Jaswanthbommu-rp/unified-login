'use strict';

/*jshint esversion: 6 */

var buildTabsPanel = function buildTabsPanel(container, tab) {
    var tabTemplate = container.find('.nav-item.hidden').clone();
    var tabLink = tabTemplate.find('a');
    var tabContentContainer = container.next('.tab-content');
    var tabContentTemplate = tabContentContainer.find('.tab-pane.hidden').clone();

    tabTemplate.removeClass('hidden');
    tabContentTemplate.removeClass('hidden');

    tabLink.attr('data-target', '#' + tab.id);
    tabLink.text(tab.name);
    tabContentTemplate.attr('id', tab.id);

    if (tab.name === 'Platform Services') {
        //replace with getting active link name from sessionStorage
        tabContentTemplate.addClass('active px-4');
        tabLink.addClass('active');
    }

    container.append(tabTemplate);
    tabContentContainer.append(tabContentTemplate);
};

var buildGeneralTiles = function buildGeneralTiles(container, tileInfo) {
    var id = tileInfo.id,
        name = tileInfo.name,
        icon = tileInfo.icon,
        linkToPageId = tileInfo.linkToPageId;

    var tileTemplate = container.find('.general-settings-tile-wrapper').clone();
    var pages = $.sessionStorage.get('settingsPages');

    var pageToOpen = pages.find(function (page) {
        return page.id === +linkToPageId;
    });
    var tileLink = pageToOpen ? '/home' + pageToOpen.pageUrl : '#';
    var company = $.sessionStorage.get('company');
    var CompanyId = company.CompanyId,
        CompanyName = company.CompanyName;


    if (CompanyId) {
        tileLink += '?CompanyId=' + CompanyId + '&CompanyName=' + CompanyName;
    }

    if (icon) {
        tileTemplate.find('img').attr('src', '/home/Assets/build/' + icon);
    }

    tileTemplate.find('a').attr({
        'data-id': id,
        'href': tileLink
    });
    tileTemplate.find('p').text(name);
    tileTemplate.removeClass('raul-tab-content tab-content general-settings-tile-wrapper');
    //tileTemplate.addClass('raul-card pull-left general-settings-tile mr-4');
    tileTemplate.addClass('tabs-raul-card pull-left general-settings-tile');

    container.append(tileTemplate);
};

var buildProductSettingTiles = function buildProductSettingTiles(container, tileInfo) {};

var buildTabContent = function buildTabContent(tab) {
    var id = tab.id,
        name = tab.name;

    var tabContentContainer = $('#' + id);
    var controlList = tab.controlList;


    controlList.filter(function (tile) {
        return tile.visible;
    }).sort(function (a, b) {
        return a.sequence - b.sequence;
    }).forEach(function (tileInfo) {
        buildGeneralTiles(tabContentContainer, tileInfo);
    });
};

$(document).ready(function () {
    var settingsPages = $.sessionStorage.get('settingsPages');
    var company = $.sessionStorage.get('company');
    var CompanyId = company.CompanyId,
        CompanyName = company.CompanyName;


    Promise.resolve(settingsPages).then(function (settingsPages) {
        //Pull the Settings pages data
        return settingsPages ? settingsPages : userAPIService('GET', '', 'api/Pages', '');
    }).then(function (response) {
        return rpSaveSettingsPages(response);
    }).then(function (settingsPages) {
        //get landing page by id
        var landingPageId = settingsPages.find(function (page) {
            return page.name.toLowerCase() === 'company settings';
        }).id;
        var url = 'api/Pages/' + landingPageId + '?companyId=' + CompanyId;

        return userAPIService('GET', '', url, '');
    }).then(function (page) {
        var tabList = page.tabList,
            breadcrumb = page.breadcrumb,
            name = page.name;

        //$('#raul-page-header-page').text(name); //set blue header in breadcrumps panel

        var container = $('#landing-tabs-panel');

        $.sessionStorage.set('tabsList', tabList);

        // commented by chandrashekhar for update breadcrumbes with static data
        /*if(breadcrumb) {
            populateSettingsBreadCrumbs(breadcrumb);
        }*/

        tabList.filter(function (tab) {
            return tab.visible;
        }).sort(function (a, b) {
            return a.sequence - b.sequence;
        }).forEach(function (tab) {
            //add tabs and tabc content to page 
            buildTabsPanel(container, tab);
            buildTabContent(tab);
        });
    }).catch(function (err) {
        //add error handler  
        console.log(err);
    });

    //EVENT Handlers
    $('#settings-search-bar').submit(function (e) {
        e.preventDefault();

        /*let form = $(this);
          if (form.parsley().isValid) {
            let searchQuery = $('#setting-search').val();
              window.location.href = `/setting/searchresults${window.location.search}&search=${searchQuery}`;
        }*/
    });
});

//Search results dropdown
/*$(document).on('keyup', '#setting-search', function () {

    var val = $(this).val().trim();
    val = val.replace(/\s+/g, '');
    console.log(val.length);
    if (val.length > 2) {
        //for checking 3 characters
        $('.search-results-dropdown').slideDown(function () {
            getSearchResult();

        });
    } else {
        $('.search-results-dropdown').fadeOut();
    }
});

$(document).on('blur', '#setting-search', function () {
    $('.search-results-dropdown').fadeOut();

    $('#settings-search-bar').parsley().reset();
});

// fetch the Search results from API
function getSearchResult() {

    let stringVal = '';
    let recentSettingsUrl = "api/tabs";
    return Promise.resolve(userAPIService('GET', '', recentSettingsUrl, '')).then(function (response) {

        for (let i = 0; i < response.length; i++) {
            
            stringVal += '<li class="p-3"><div class="raul-list-item-first-line">' + response[i].name + '</div><div class="raul-list-item-second-line">General Setup / System & Security / Identity Provider</div ></li>';
            if (i > 3) {
                break
            }
        }

        $('#searchlist').html(stringVal);
        $('.readmore').show();

    });

}*/