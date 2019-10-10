"use strict";

(function ($) {

    $.root_ = $('body');
    var root = this;
    var ignore_key_elms = ["#raul-header", "#raul-white-bar", "#raul-left-navigation", "#raul-footer", "#raul-notifications-context", ".overlay-cover", "#rp-cart-root", ".padded-container", "#raul-shopping-context", "#raul-page-header", "#raul-left-navigation", "#content", "#raul-footer", "#main", "div.page-footer", "#shortcut", "#divSmallBoxes", "#divMiniIcons", "#divbigBoxes", "#voiceModal", "main", "header", "footer", "script", ".ui-chatbox", ".pace"];
    var containerName = ".contentWrapper";

    //Flag Initial Load of the Application
    var initialLoad = true;

    if (localStorage.getItem('left-nav') === 'dark') {
        $('.left-nav-toggle i').removeClass('fa-toggle-off');
        $('.left-nav-toggle i').addClass('fa-toggle-on');
    }

    $('.left-nav-toggle i').on('click', function (event) {
        if ($(this).hasClass('fa-toggle-off')) {
            $(this).removeClass('fa-toggle-off');
            $(this).addClass('fa-toggle-on');
            RAUL.leftnav.$leftnav.removeClass('raul-left-navigation-light');
            RAUL.leftnav.$leftnav.addClass('raul-left-navigation-dark');
            localStorage.setItem('left-nav', 'dark');
        } else {
            $(this).removeClass('fa-toggle-on');
            $(this).addClass('fa-toggle-off');
            RAUL.leftnav.$leftnav.removeClass('raul-left-navigation-dark');
            RAUL.leftnav.$leftnav.addClass('raul-left-navigation-light');
            localStorage.setItem('left-nav', 'light');
        }
    });
    checkURL();

    /*
     * BEGIN: EVENT SETUP
     */
    function createSpaEvents() {

        // DO on hash change
        $(window).on('hashchange', function (e) {
            checkURL();
        });

        $(document).on('click', 'a[href!="#"]', function (e) {
            e.preventDefault();
            processURL(e);
        });
    }

    /*
     * END: EVENT SETUP
     */

    //SET UP THE SPA EVENTS ON INITIAL LOAD
    createSpaEvents();

    //CLEAR SESSION STORAGE ON PAGE REFRESH
    sessionStorage.clear();

    /*
     * BEGIN: CHANGE PAGE URL
     */
    function changePage(newPage) {
        //CLEAR OUT PREVIOUS WINDOW EVENT LISTENERS
        //$( window ).unbind();
        //$( document ).unbind();

        //SET UP THE SPA EVENTS
        //createSpaEvents();

        if (document.cookie.indexOf('access_token=') === -1) {
            window.location = "https://myldev.corp.realpage.com/identity/connect/authorize?state=refreshToken&scope=omnichannel%20rplandingapi%20openid%20profile%20offline_access&response_type=code&approval_prompt=auto&redirect_uri=https%3A%2F%2Fmydev.realpage.com&client_id=omnichannel";
        } else {
            //SET WINDOW TO NEW PAGE
            window.location.hash = newPage;
        }
    }
    /*
     * END: CHANGE PAGE URL
     */

    /*
     * BEGIN: PAGE SETUP
     */
    function pageSetup() {
        //BEGIN: PUT WHATEVER YOU NEED IN THIS FUNCTION FOR INITIALIZATION OF EACH PAGE

        //ADDRESS THE UI-JP DEPENDENCIES
        $('[ui-jp]').uiJp();

        //REPLACE BREADCRUMBS WITH NEW BREADCRUMBS
        var newBreadcrumbs = $('#freshBreadcrumbs');
        $('#raul-page-header').html(newBreadcrumbs.html());
        newBreadcrumbs.remove();

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

    //Expose the pageSetup function to other pages
    window.pageSetup = pageSetup;

    /*
     * PROCESS THE URL THAT COMES FROM LINKS
     */
    function processURL(e) {

        var $this = $(e.currentTarget);

        //Don't process links that have the following condition
        if ($this.attr('href') === undefined || $this.attr('href').indexOf("javascript:void(0)") >= 0 || $this.attr('href') === "#" || $this.attr('href') === "") {
            return 0;
        }

        //Handle applications wanting to open URL in a blank page or wanting to load a full URL
        if ($this.attr('target') === '_blank') {
            window.open($this.attr('href'));
            return 0;
        } else if ($this.attr('href').indexOf("http") >= 0) {
            window.location.href = $this.attr('href');
            return 0;
        }

        if ($this.parents("nav").length == 1) {

            // YES, the link element is inside the nav container
            // if parent is not active then get hash, or else page is assumed to be loaded
            if (!$this.parent().hasClass("active") && !$this.attr('target')) {

                // update window with hash
                // you could also do here:  thisDevice === "mobile" - and save a little more memory

                if ($.root_.hasClass('mobile-view-activated')) {
                    $.root_.removeClass('hidden-menu');
                    $('html').removeClass("hidden-menu-mobile-lock");
                    window.setTimeout(function () {
                        if (window.location.search) {
                            changePage(window.location.href.replace(window.location.search, '').replace(window.location.hash, '') + '#' + $this.attr('href'));
                        } else {
                            window.location.hash = $this.attr('href');
                        }
                    }, 150);
                    // it may not need this delay...
                } else {
                    if (window.location.search) {
                        changePage(window.location.href.replace(window.location.search, '').replace(window.location.hash, '') + '#' + $this.attr('href'));
                    } else {
                        changePage($this.attr('href'));
                    }
                }

                // clear DOM reference
                // $this = null;
            }
        } else {
            //Not inside of the navigation
            changePage($this.attr('href'));
        }
    }
    window.processURL = processURL;

    /*
     * CHECK TO SEE IF URL EXISTS
     */
    function checkURL() {

        var rawUrl = location.href.split('#').splice(1).join('#');
        if (rawUrl.indexOf("#") >= 0) {
            rawUrl = rawUrl.split('#').splice(1).join('#');
        }

        console.log("rawUrl: " + rawUrl);

        //BEGIN: Check to see if the href is invalid OR an anchor tag that shouldn't change the page view
        if (rawUrl.indexOf("javascript:void(0)") >= 0 || rawUrl === "#") {
            //console.log("NOT RUNNING THE LINK: " + rawUrl);
            return 0;
        } else if (rawUrl.indexOf("http") === -1 && rawUrl.indexOf("#") === -1) {
            rawUrl = "#" + rawUrl;
            //console.log("RUNNING THE LINK AS: " + rawUrl);
        }
        //END: Check to see if the href is invalid OR an anchor tag that shouldn't change the page view

        var url = location.href.split('#').splice(1).join('#');

        if (url.indexOf("#") >= 0) {
            url = url.split('#').splice(1).join('#');
        }

        //BEGIN: IE11 Work Around
        if (!url) {

            try {
                var documentUrl = window.document.URL;
                if (documentUrl) {
                    if (documentUrl.indexOf('#', 0) > 0 && documentUrl.indexOf('#', 0) < documentUrl.length + 1) {
                        url = documentUrl.substring(documentUrl.indexOf('#', 0) + 1);
                    }
                }
            } catch (err) {}
        }
        //END: IE11 Work Around

        var container = $(containerName);
        // Do this if url exists (for page refresh, etc...)

        if (url && url !== "/") {
            console.log("URL: " + url);
            // remove all active class
            $('nav li.active').removeClass("active");
            // match the url and add the active class
            $('nav li:has(a[href="' + url + '"])').addClass("active");
            var title = $('nav a[href="' + url + '"]').attr('title');

            // change page title from global var
            document.title = title || document.title;

            if (location.href.indexOf("?") >= 0) {
                window.location.href = '/' + url;
            } else {
                // parse url to jquery
                loadURL(url, container);
            }
        } else {
            // grab the first URL from nav
            //var $this = $('nav > ul > li:first-child > a[href!="#"]');

            //update hash
            //window.location.hash = $this.attr('href');
            window.location.href = '/#/dashboard';

            //clear dom reference
            //$this = null;
        }
    }

    /*
     * LOAD AJAX PAGES
     */
    function loadURL(url, container) {
        $.ajax({
            type: "GET",
            url: url,
            dataType: 'html',
            cache: true, // (warning: setting it to false will cause a timestamp and will call the request twice)
            beforeSend: function beforeSend() {
                if (container[0] == $(containerName)[0]) {
                    // destroy form controls: Datepicker, select2, autocomplete, mask, bootstrap slider

                    if ($.fn.select2 && $(containerName + ' select.select2')[0]) {
                        $(containerName + ' select.select2').select2('destroy');
                    }

                    if ($.fn.mask && $(containerName + ' [data-mask]')[0]) {
                        $(containerName + ' [data-mask]').unmask();
                    }

                    if ($.fn.datepicker && $(containerName + ' .datepicker')[0]) {
                        $(containerName + ' .datepicker').off();
                        $(containerName + ' .datepicker').remove();
                    }

                    if ($.fn.slider && $(containerName + ' .slider')[0]) {
                        $(containerName + ' .slider').off();
                        $(containerName + ' .slider').remove();
                    }

                    // end destroy form controls

                }
                // end cluster destroy

                // empty container and var to start garbage collection (frees memory)
                container.removeData().html("");

                // place cog
                container.html('<span class="ajax-loading-animation"><i class="fa fa-cog fa-spin"></i> Loading...</span>');

                // Only draw breadcrumb if it is main content material
                if (container[0] == $(containerName)[0]) {

                    // clear everything else except these key DOM elements
                    // we do this because sometime plugins will leave dynamic elements behind
                    console.log("BODY ELEMENTS: ", $('body').find('> *').filter(':not(' + ignore_key_elms + ')'));
                    $('body').find('> *').filter(':not(' + ignore_key_elms + ')').empty().remove();

                    // scroll up
                    $("html").animate({
                        scrollTop: 0
                    }, "fast");
                }
                // end if
            },
            success: function success(data) {

                //add the document.ready call for page setup
                data += '<script>\n' + '        $(document).ready(function(){\n' + '           pageSetup();\n' + '        });\n' + '    </script>';

                // dump data to container
                container.css({
                    opacity: '0.0'
                }).html(data).delay(50).animate({
                    opacity: '1.0'
                }, 300);
                // clear data var

                //scroll to the top of the page
                window.scrollTo(0, 0);

                data = null;
                container = null;
            },
            error: function error(xhr, status, thrownError, _error) {
                //console.log("XHR",xhr);
                //console.log("status",status);
                //console.log("thrownError",thrownError);
                //console.log("error",error);

                if (xhr.status === 401) {
                    var reloadParentURL = location.href.split('#');
                    window.location.href = reloadParentURL[0];
                } else {
                    container.html('<h4 class="ajax-loading-error"><i class="fa fa-warning txt-color-orangeDark"></i> Error requesting <span class="txt-color-red">' + url + '</span>: ' + xhr.status + ' <span style="text-transform: capitalize;">' + thrownError + '</span></h4>');
                }
            },
            async: true
        });
    }
})(jQuery);