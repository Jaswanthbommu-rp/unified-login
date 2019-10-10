/*jshint esversion: 6 */

let buildTabsPanel = (container, tab) => {
    let tabTemplate = container.find('.nav-item.hidden').clone();
    let tabLink = tabTemplate.find('a');
    let tabContentContainer = container.next('.tab-content');
    let tabContentTemplate = tabContentContainer.find('.tab-pane.hidden').clone();

    tabTemplate.removeClass('hidden');
    tabContentTemplate.removeClass('hidden');

    tabLink.attr('data-target', `#${tab.id}`);
    tabLink.text(tab.name);
    tabContentTemplate.attr('id', tab.id);

    if (tab.name === 'Platform Services') {//replace with getting active link name from sessionStorage
        tabContentTemplate.addClass('active px-4');
        tabLink.addClass('active');
    }

    container.append(tabTemplate);
    tabContentContainer.append(tabContentTemplate);
};

let buildGeneralTiles = (container, tileInfo) => {
    let { id, name, icon, linkToPageId } = tileInfo;
    let tileTemplate = container.find('.general-settings-tile-wrapper').clone();
    let pages = $.sessionStorage.get('settingsPages'); 

    let pageToOpen = pages.find(page => page.id === +linkToPageId);
    let tileLink = pageToOpen ? '/home' + pageToOpen.pageUrl : '#'; 
    let company = $.sessionStorage.get('company');
    let { CompanyId, CompanyName } = company;

    if (CompanyId) {
        tileLink += `?CompanyId=${CompanyId}&CompanyName=${CompanyName}`;
    }

    if (icon) {
        tileTemplate.find('img').attr('src', `/home/Assets/build/${icon}`);   
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

let buildProductSettingTiles = (container, tileInfo) => {

};

let buildTabContent = (tab) => {
    let { id, name } = tab;
    let tabContentContainer = $(`#${id}`);
    let { controlList } = tab;

    controlList.filter(tile => tile.visible)
        .sort((a, b) => a.sequence - b.sequence)
        .forEach(tileInfo => {
            buildGeneralTiles(tabContentContainer, tileInfo);
        });
};

$(document).ready(function () {
        let settingsPages = $.sessionStorage.get('settingsPages');    
        let company = $.sessionStorage.get('company');
        let { CompanyId, CompanyName } = company;
    
        Promise.resolve(settingsPages)
        .then( settingsPages => {
            //Pull the Settings pages data
            return settingsPages ? settingsPages : userAPIService('GET', '', 'api/Pages', '');
        }).then( response => {  
            return rpSaveSettingsPages(response);
        }).then( settingsPages => { //get landing page by id
            let landingPageId = settingsPages.find(page => page.name.toLowerCase() === 'company settings').id;
            let url = `api/Pages/${landingPageId}?companyId=${CompanyId}`;

            return userAPIService('GET', '', url, ''); 
        }).then(page => {
            let { tabList, breadcrumb, name } = page;

          
            //$('#raul-page-header-page').text(name); //set blue header in breadcrumps panel

            let container = $('#landing-tabs-panel');

            $.sessionStorage.set('tabsList', tabList);

              // commented by chandrashekhar for update breadcrumbes with static data
            /*if(breadcrumb) {
                populateSettingsBreadCrumbs(breadcrumb);
            }*/

            tabList.filter(tab => tab.visible)
                .sort((a, b) => a.sequence - b.sequence)
                .forEach(tab => { //add tabs and tabc content to page 
                    buildTabsPanel(container, tab);
                    buildTabContent(tab);
                });

        }).catch( err => {
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