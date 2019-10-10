"use strict";

var windowResized = false;
var animationSpeed = 500;
var initialLoad = true;
var userAction = false;
var winOrientation;
var resizeTimeout;
var resizeId; //to limit the multiple calls on the resize --


var isDoubleClicked = function isDoubleClicked(element) {
    if (element.data("isclicked")) return true;
    element.data("isclicked", true);
    setTimeout(function () {
        element.removeData("isclicked");
    }, animationSpeed + 500);
    return false;
};

var doWindowSize = function doWindowSize() {
    var win = $(window).width();

    //SET THE DEFAULT NAVIGATION OVERLAY OR PUSH
    $.sessionStorage.set("overlay", $.sessionStorage.overlayAppWithNav);

    if ((win <= 1000 || $.sessionStorage.get("leftNavState") === "narrow") && win >= 481) {
        if (!userAction) {
            if (!$(".leftNavWrapper").hasClass("lCollapsed")) {

                if ($.sessionStorage.get("overlay") !== true) {
                    $(".leftNavWrapper").removeClass("mCollapsed");
                }

                $(".ft-breadcrumb").removeClass("mobilerViewCrumb");
                $(".brandClick, .buildingTop").show();
                if ($.sessionStorage.get("overlay") !== true) {
                    shrinkLeftNav();
                }
            }
            if (win <= 1000) {
                $(".ft-breadcrumb").addClass("windowSmall");
            } else {
                $(".ft-breadcrumb").removeClass("windowSmall");
            }
        }
    } else if (win >= 1001) {
        //This width is set below the above size to make sure the cookie reads correctly
        if (!userAction) {
            if ($.sessionStorage.get("overlay") !== true) {
                growLeftNav();
                $(".leftNavWrapper").removeClass("mCollapsed");
            }
            $(".ft-breadcrumb").removeClass("mobilerViewCrumb").removeClass("windowSmall");
            $(".brandClick, .buildingTop").show();
        }
    } else if (win <= 480 || winOrientation === 'landscape') {
        //FORCE THE NAVIGATION TO OVERLAY
        $.sessionStorage.set("overlay", true);
        if (!userAction) {
            setMobile();
            $(".ft-breadcrumb").addClass("mobilerViewCrumb");
            $(".ft-breadcrumb").addClass("windowSmall");
        }
    }

    leftNavFormatting();
    windowResized = false;
};

var setMobile = function setMobile() {
    if (!$(".leftNavWrapper").hasClass("mCollapsed")) {
        $(".subNavMouseOver").removeClass("overActive");
        $(".leftNavWrapper").animate({
            width: "0rem"
        }, animationSpeed);
        $(".topNavHeaderText, .endPoint, .subNavItemWrapper").hide();

        if ($.sessionStorage.get("overlay") !== true) {
            $(".padded-container, .internal-footer").animate({
                paddingLeft: "0rem"
            }, animationSpeed);
        }
        $(".subNavWrapper.active").parents(".subNavMouseOver").siblings(".topNavHeaderClick").addClass('is-open');
        $(".leftNavWrapper").addClass("mCollapsed");
        $(".mobileView").addClass("mobilerView");
        $(".leftNavWrapper").removeClass("lCollapsed");
        $(".toggleButton").addClass("mOpen");
        $(".brandClick, .buildingTop").hide();
    }
};
var autoloadNonMobileFlyout = function autoloadNonMobileFlyout() {
    if ($.sessionStorage.get("overlay") !== true) {
        $(".leftNavWrapper").css("width", "5rem");
        $(".padded-container, .internal-footer").css("paddingLeft", "5rem");
    }

    $(".topNavHeaderText, .endPoint, .subNavItemWrapper").hide();
    /*
        $(".subNavWrapper.active").parents(".subNavMouseOver").siblings(".topNavHeaderClick").addClass('is-open');
        $(".subNavWrapper.active").parents(".subNavMouseOver").show();
        $(".subNavWrapper.active").show();
        var visibleViewPort = $(window).height();
        $(".topNavWrapper").css("height", 70);
        $(".subNavWrapper.active").parents(".subNavMouseOver").addClass("overActive").css("top",0).hide();
        $(".subNavWrapper.active").parents(".subNavMouseOver").children().find(".subNavItemWrapper").addClass("subNavItemOverActive");
        $(".subNavWrapper.active").parents(".subNavMouseOver").height(visibleViewPort);
        $(".subNavWrapper.active").parents(".subNavMouseOver").animate({width: "show"}, animationSpeed);
        $(".subNavWrapper.active").parents(".subNavMouseOver").children().find(".subNavItemWrapper").show();
    */
    $(".leftNavWrapper").addClass("lCollapsed");
};

var shrinkLeftNav = function shrinkLeftNav() {
    if (!$(".leftNavWrapper").hasClass("lCollapsed")) {

        if ($.sessionStorage.get("leftNavState") === "narrow" && initialLoad && $.sessionStorage.get("overlay") !== true) {
            //change handled by css class ".lCollapsed"
            $(".leftNavWrapper").css("width", "5rem");
            $(".padded-container, .internal-footer").css("paddingLeft", "5rem");
            autoloadNonMobileFlyout();
        } else {
            $(".topNavHeaderText, .endPoint, .subNavItemWrapper").hide();
            //change handled by css class ".lCollapsed"

            if ($.sessionStorage.get("overlay") !== true) {
                $(".leftNavWrapper").animate({
                    width: "5rem"
                }, animationSpeed, function () {
                    autoloadNonMobileFlyout();
                });

                $(".padded-container, .internal-footer").animate({
                    paddingLeft: "5rem"
                }, animationSpeed);
            }
        }

        initialLoad = false;

        setTimeout(function () {
            userAction = false;
        }, animationSpeed + 200);
    }
};

var shrinkLeftNavMobile = function shrinkLeftNavMobile() {

    if (!$(".leftNavWrapper").hasClass("mCollapsed")) {
        clearHoverNav();
        $(".leftNavWrapper").animate({
            width: "0rem"
        }, animationSpeed);
        $(".topNavHeaderText, .endPoint, .subNavItemWrapper").hide();

        if ($.sessionStorage.get("overlay") !== true) {
            $(".padded-container, .internal-footer").animate({
                paddingLeft: "0rem"
            }, animationSpeed);
        }

        $(".subNavWrapper.active").parents(".subNavMouseOver").siblings(".topNavHeaderClick").addClass('is-open');
        $(".leftNavWrapper").addClass("mCollapsed");
    }

    initialLoad = false;

    setTimeout(function () {
        userAction = false;
    }, animationSpeed + 200);
};

var growLeftNav = function growLeftNav() {
    clearHoverNav();
    $(".leftNavWrapper").animate({
        //change handled by css class ".lCollapsed"
        width: "300px"
    }, animationSpeed);

    if ($.sessionStorage.get("overlay") !== true) {
        $(".padded-container, .internal-footer").animate({
            paddingLeft: "300px"
        }, animationSpeed);
    }

    setTimeout(function () {
        $(".topNavHeaderText, .endPoint, .subNavItemWrapper").show("fast");
    }, animationSpeed);
    $(".leftNavWrapper").removeClass("lCollapsed");
    $(".brandClick").show();
    $(".subNavWrapper.active").parents(".subNavMouseOver").siblings(".topNavHeaderClick").find(".topNavHeader").find(".endPoint").removeClass("fa-chevron-down").addClass("fa-chevron-up");

    initialLoad = false;

    setTimeout(function () {
        userAction = false;
    }, animationSpeed + 200);
};

var growLeftNavMobile = function growLeftNavMobile() {
    console.log("grow left nav");
    clearHoverNav();
    $(".leftNavWrapper").animate({
        //change handled by css class ".lCollapsed"
        width: "300px"
    }, animationSpeed);

    if ($.sessionStorage.get("overlay") !== true) {
        $(".padded-container, .internal-footer").animate({
            paddingLeft: "300px"
        }, animationSpeed);
    }

    setTimeout(function () {
        $(".topNavHeaderText, .endPoint, .subNavItemWrapper").show("fast");
    }, animationSpeed);
    $(".leftNavWrapper").removeClass("mCollapsed");
    $(".subNavWrapper.active").parents(".subNavMouseOver").siblings(".topNavHeaderClick").find(".topNavHeader").find(".endPoint").removeClass("fa-chevron-down").addClass("fa-chevron-up");

    initialLoad = false;

    setTimeout(function () {
        userAction = false;
    }, animationSpeed + 200);
};

var clearHoverNav = function clearHoverNav() {
    $(".subNavMouseOver").removeClass("overActive");
    $(".subNavMouseOver").children().find(".subNavItemWrapper").removeClass("subNavItemOverActive").hide();
    $(".subNavMouseOver").css("top", "initial");
    $(".topNavWrapper, .subNavMouseOver").height("auto");
};

var clearDropNav = function clearDropNav() {
    $(".subNavWrapper").hide();
    $(".subNavWrapper.active").parents(".subNavMouseOver").siblings(".topNavHeaderClick").removeClass('is-open');
    $(".subNavWrapper").removeClass("active");
    $(".endPoint").removeClass("fa-chevron-up").addClass("fa-chevron-down");
};

var leftNavFormatting = function leftNavFormatting() {

    if ($.sessionStorage.get("overlay") === true) {
        if ($(".leftNavWrapper").length > 0) {
            $(".leftNavWrapper").css("width", "0rem");
            $(".leftNavWrapper").addClass("mCollapsed");
            $(".padded-container, .internal-footer").css("paddingLeft", "0rem");
        }
    } else if ($.sessionStorage.get("leftNavState") === "narrow") {
        if ($(".leftNavWrapper").length > 0) {
            $(".leftNavWrapper").css("width", "5rem");
            $(".padded-container, .internal-footer").css("paddingLeft", "5rem");
        }
    } else {
        if ($(".leftNavWrapper").length > 0) {
            $(".leftNavWrapper").css("width", "300px");
            $(".padded-container, .internal-footer").css("paddingLeft", "300px");
        }
    }
};

$(".toggleButton").click(function () {
    console.log("clicking the toggle button");
    if (isDoubleClicked($(this))) {
        return;
    }
    userAction = true;

    var win = $(window).width();

    if (win <= 480 || winOrientation === 'landscape' || $.sessionStorage.get("overlay") === true) {
        if ($(".leftNavWrapper").hasClass("mCollapsed")) {
            growLeftNavMobile();
        } else {
            shrinkLeftNavMobile();
        }
    } else {
        if ($(".leftNavWrapper").hasClass("lCollapsed")) {
            $.sessionStorage.set("leftNavState", "wide");
            growLeftNav();
        } else {
            $.sessionStorage.set("leftNavState", "narrow");
            shrinkLeftNav();
        }
    }
});

$(".topNavHeaderClick").on("click", function (event) {

    event.preventDefault();
    //Call the SPA library and process the URL
    processURL(event);

    if (isDoubleClicked($(this))) {
        return;
    }

    $('.topNavHeaderClick').removeClass('active');
    $('.subNavClick').removeClass('subNavItemActive');
    $(this).toggleClass('active');

    if ($(this).hasClass("alreadyOpen") && $(this).parents(".leftNavWrapper").hasClass("lCollapsed")) {
        $(this).removeClass("alreadyOpen");
        $(this).removeClass("is-open");
        $(this).siblings(".subNavMouseOver").hide();
        $(this).siblings(".subNavMouseOver").removeClass("overActive");
        $(this).siblings(".subNavMouseOver").children(".subNavWrapper").removeClass("active");
        $(this).siblings(".subNavMouseOver").children(".subNavWrapper").children(".subNavItemWrapper").removeClass("subNavItemOverActive");
        return;
    } else {
        $(".topNavHeaderClick").removeClass("alreadyOpen");
        $(this).addClass("alreadyOpen");
        $(this).siblings(".subNavMouseOver").show();
        var currentSubHead = $(this).siblings(".subNavMouseOver").children(".subNavWrapper");

        if ($(currentSubHead).hasClass("active")) {
            clearDropNav();
            $(currentSubHead).removeClass("active");
        } else {
            clearDropNav();
            if (!$(this).parents(".leftNavWrapper").hasClass("lCollapsed")) {
                $(currentSubHead).slideToggle("slow");
            } else {
                $(currentSubHead).hide();
            }
            $(currentSubHead).addClass("active");
            $(".subNavWrapper.active").parents(".subNavMouseOver").siblings(".topNavHeaderClick").addClass('is-open');
            $(this).find(".endPoint").removeClass("fa-chevron-down").addClass("fa-chevron-up");
        }

        var visibleViewPort = $(window).height();
        if ($(this).parents(".leftNavWrapper").hasClass("lCollapsed")) {
            clearHoverNav();
            $(this).siblings(".subNavMouseOver").addClass("overActive").css("top", 0).hide();
            $(this).siblings(".subNavMouseOver").children().find(".subNavItemWrapper").addClass("subNavItemOverActive");
            $(this).siblings(".subNavMouseOver").height(visibleViewPort);
            $(this).siblings(".subNavMouseOver").animate({ width: "show" }, animationSpeed);
            $(this).siblings(".subNavMouseOver").find(".subNavWrapper").show();
            $(this).siblings(".subNavMouseOver").children().find(".subNavItemWrapper").show();
        } else {
            return;
        }
    }
});

$(".contentWrapper").on("click", function (event) {
    if ($(".leftNavWrapper").hasClass("lCollapsed")) {
        clearHoverNav();
    }
});
$(".subNavClick").on("click", function (event) {

    event.preventDefault();
    //Call the SPA library and process the URL
    processURL(event);

    if (!$(this).hasClass("subNavItemActive")) {
        $(".subNavClick").removeClass("subNavItemActive");
        $(this).addClass("subNavItemActive");
    }
    if ($('.leftNavWrapper').hasClass('lCollapsed')) {
        $(this).closest('.topNavWrapper').find('.topNavHeaderClick').click();
    }
});

//Clear event before setting the event
$(document).off('click', '.change-nav-version .bootstrap-switch');
//Setting the event
$(document).on('click', '.change-nav-version .bootstrap-switch', function (event) {});

$('input[name="nav-theme"]').on('switchChange.bootstrapSwitch', function (event, state) {
    if (state === true) {
        $(".leftNavWrapper").removeClass("white-version");
        $.sessionStorage.set("theme", "dark");
    } else {
        $(".leftNavWrapper").addClass("white-version");
        $.sessionStorage.set("theme", "light");
    }
});

$('input[name="nav-button-size"]').on('switchChange.bootstrapSwitch', function (event, state) {
    if (state === true) {
        $(".leftNavWrapper").removeClass("compressed-version");
        $.sessionStorage.set("compressed", false);
    } else {
        $(".leftNavWrapper").addClass("compressed-version");
        $.sessionStorage.set("compressed", true);
    }
});

$(window).resize(function () {
    if (!userAction) {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function () {
            doWindowSize();
        }, 500);
    }
});

$(window).on("load", function () {

    //IF YOU WANT THE NAVIGATION TO OVERLAY, SET THIS TO TRUE
    ///////////////////////////////////////////////////////
    $.sessionStorage.overlayAppWithNav = false;
    ///////////////////////////////////////////////////////


    //DEFAULT SETTING FOR LEFT NAVIGATION
    $.sessionStorage.set("overlay", $.sessionStorage.overlayAppWithNav);

    //DEFAULT SETTING FOR LEFT NAV
    if (!$.sessionStorage.get("theme")) {
        $.sessionStorage.set("theme", "light");
    }

    //DEFAULT SETTING FOR BUTTONS
    if (!$.sessionStorage.get("compressed")) {
        $.sessionStorage.set("compressed", true);
    }

    if ($.sessionStorage.get("theme") === "light") {
        if ($(".leftNavWrapper").length > 0) {
            $(".leftNavWrapper").addClass("white-version");
            setTimeout(function () {
                $('input[name="nav-theme"]').bootstrapSwitch('toggleState');
            }, 500);
        }
    }

    if ($.sessionStorage.get("compressed") === true) {
        if ($(".leftNavWrapper").length > 0) {
            $(".leftNavWrapper").addClass("compressed-version");
            setTimeout(function () {
                $('input[name="nav-button-size"]').bootstrapSwitch('toggleState');
            }, 500);
        }
    }

    //FORMAT THE LEFT NAV BASED ON PUSH OR OVERLAY
    leftNavFormatting();

    $(".navbar .rp-icon-grid-2").click(function () {
        $(".app-switcher-menu").toggle();
    });

    doWindowSize();
});
$(function () {
    $(".subNavWrapper.active").parents(".subNavMouseOver").siblings(".topNavHeaderClick").addClass('is-open');
});