/*jshint esversion: 6 */

$(document).ready(function () {

    buildPage();


    //EVENT Handlers
    $(document).on('click', '.pages-tabs li a', function (e) {
        e.preventDefault();

        /*handler for drilldown menu*/
        if (this.hasAttribute('data-drilldown-button')) {
            /*Set the top position for submenus*/
            let top = $('.pages-wrapper').offset().top;
            $('.pages-tabs').offset({ top });

            /*Navigate between menus*/
            $('.open').removeClass('open');
            $(this).siblings('[data-drilldown-sub]').addClass("open");
            $(this).parent('li').addClass("close-pages");
            $(this).parent('li').siblings().addClass("close-pages");

            //set back button text
            $('.left-menu-back span').text($(this).text());

            activeTab('[data-tabname=' + $(this).attr('data-tabname') + ']');

            return;
        }

        /*handler for open pageContent*/
        let pageId = this.id;

        buildPageContent(pageId).then(() => {
            activeTab('[data-tabname=' + $(this).attr('data-tabname') + ']');
        });
        
    });

    //Prevent toggle enable on info click
    $('#dynamic-pages-content').on('click', '.toggle-setting a[data-tooltip]', function (e) {
        e.preventDefault();
    });
    
    //responsive mobile navigation trigger
    $(document).on('click', '.page-leftnav-toggle', function () {
        $('#dynamic-pages-content').removeClass('page-content');
        $('.settings-pages-leftnav').addClass('fix-leftnav');
    });

    /*Move to previous menu*/
    $('.left-menu-back').click(function () {
        let openMenu = $('.open');
        let newOpenMenu = openMenu.closest('ul:not(.open)');

        if (!newOpenMenu.length) {
            window.location = '/setting' + window.location.search; 
        }

        $('.open').removeClass('open');

        newOpenMenu.addClass("open");
        newOpenMenu.find('li').removeClass('close-pages');

        //set back button text
        $('.left-menu-back span').text(newOpenMenu.siblings('a').text() || $.sessionStorage.get('tileName'));
    });

});

let activeTab = (tab) => {
    let tabname = $('a' + tab).attr('data-tabname');
    $('#dynamic-pages-content > .tab-pane').hide();
    $('.pages-tabs li').removeClass('active');
    $(tab).closest('li').addClass('active');
    $('#' + tabname).fadeIn();
};

let pageElem = new Page();

let buildPage = () => {
    let pageId = 1261;
    
    userAPIService('GET', '', `/api/DynamicSettings/${pageId}/NavigationList`, '')
        .then(pagesResponse => {
            let pageHead = $('.tile-name');
            let {
                id,
                name,
                childNodes
            } = pagesResponse;

            pageHead.find('span').text(name);
            $.sessionStorage.set('tileName', name);

            pageHead.attr({ id });

            buildPagesTabs(childNodes);

            $('.raul-content-loader-wrapper').addClass('hidden');

           
            //$('.pages-tab:first-child a').trigger('click');
        });
        
};

let buildPagesTabs = (childNodesList) => {
    let tabsContainer = $('#dynamic-pages-list');
    
    childNodesList.sort((a, b) => a.sequence - b.sequence).forEach( (page, i) => {
        let { id, name, childNodes } = page;

        pageElem.createPageTab(id, name, childNodes, tabsContainer);
    });

    
};

let buildPageContent = (pageId) => {
    //$('.settings-section-container').empty();

    $('#dynamic-pages-content').addClass('page-content');
    $('.settings-pages-leftnav').removeClass('fix-leftnav');

    /*If content already exists, show it*/
    if ($(`#page-${pageId}`).length) {
        $(`#page-${pageId}`).show();

        return Promise.resolve(true);
    }

    let pageContent = {
        "id": 1262,
        "name": "Chocolate",
        controls: [
            {
                controls: [
                    {
                        value: false,
                        defaultValue: false,
                        labelText: "Show video tutorials to users",
                        infoText: "",
                        isVisible: true,
                        id: 110,
                        type: "toggle",
                        sequence: 1
                    },
                    {
                        value: false,
                        defaultValue: false,
                        labelText: "Show video tutorials to users2",
                        infoText: "Show video tutorials to users2",
                        isVisible: true,
                        id: 111,
                        type: "toggle",
                        sequence: 0
                    }
                ],
                isVisible: true,
                id: 11,
                title: "Dynamic Section1",
                description: "Dynamic Section1 description",
                type: "section",
                sequence: 0
            },
            {
                controls: [
                    {
                        value: false,
                        defaultValue: false,
                        labelText: "Show video tutorials to users3",
                        infoText: "",
                        isVisible: true,
                        id: 112,
                        type: "toggle",
                        sequence: 1
                    },
                    {
                        value: false,
                        defaultValue: false,
                        labelText: "Show video tutorials to users4",
                        infoText: "Show video tutorials to users2",
                        isVisible: true,
                        id: 113,
                        type: "toggle",
                        sequence: 0
                    }
                ],
                isVisible: true,
                id: 12,
                title: "Dynamic Section2",
                description: "",
                type: "section",
                sequence: 1
            }
        ]
    };

    //replace with API call with pageId
    if (pageId === '1262') {
        return Promise.resolve(pageContent)
            .then(pageContent => {
                let { id, name, controls } = pageContent;
                let tabContentContainer = $('#dynamic-pages-content');

                pageElem.createPageContent(id, name, tabContentContainer);

                let sectionsContainer = $(`#page-${id} .settings-section-container`);

                controls.sort((a, b) => a.sequence - b.sequence);

                if (controls.some(control => control.type === 'section')) {//if there are sections within the page
                    
                    controls.forEach(item => {
                        let { id, title, isVisible, description } = item;

                        let sectionId = item.id;
                        let newSection = new Section(id, title, description, isVisible );

                        newSection.addTo(sectionsContainer);

                        item.controls.forEach(control => {
                            let controlElem = new Control(control);

                            controlElem.addTo($(`#section-${sectionId} .controls-container`));
                        });

                    });

                    /*let firstListElem = controlsContainer.find('li:first-child');

                    if (firstListElem.hasClass('hidden')) {
                        firstListElem.next('li:not(.hidden)').addClass('control-item-border');
                    }*/
                } else {
                    let newSection = new Section();

                    newSection.addTo(sectionsContainer);

                    controls.forEach(control => {
                        let controlElem = new Control(control);

                        controlElem.addTo($(`#section-wrap .controls-container`));
                    });
                }
                
            });
    } else {
        return Promise.resolve(true);
    }

    
};

