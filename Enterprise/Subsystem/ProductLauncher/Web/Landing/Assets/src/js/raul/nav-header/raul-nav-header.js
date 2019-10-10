if (typeof RAUL === 'undefined') { var RAUL = { header: {} }; }
else { RAUL.header = {}; }

var isTouch = (('ontouchstart' in window) || (navigator.msMaxTouchPoints > 0));

RAUL.header.closeOtherContexts = function(target){
    console.log(target);
    if(target !== 'navigation' && RAUL.header.$leftnav && $(window).width() < 768){
        $('body').removeClass('raul-left-navigation-mobile-open');
    }
    if(target !== 'more' && RAUL.header.$more && RAUL.header.$more.css('right') === "0px"){
        RAUL.header.$more.css('width','0');
        RAUL.header.$more.css('box-shadow','none');
    }
    if(target !== 'switcher' && RAUL.header.$switcher && RAUL.header.$switcher.is(":visible")){
        RAUL.header.$switcher.fadeOut();
    }
    if(target !== 'notifications' && RAUL.header.$notifications && RAUL.header.$notifications.is(":visible")){
        RAUL.header.$notifications.fadeOut();
    }
    if(target !== 'user' && RAUL.header.$user && RAUL.header.$user.is(":visible")){
        RAUL.header.$user.slideUp();
    }
    if(target !== 'shopping' && RAUL.header.$shopping && RAUL.header.$shopping.is(":visible")){
        RAUL.header.$shopping.fadeOut();
    }
    if(target !== 'breadcrumbs' && RAUL.header.$breadcrumbsDropdown && RAUL.header.$breadcrumbsDropdown.is(":visible")){
        RAUL.header.$breadcrumbsDropdown.slideUp();
    }
};

$(document).on('click touchstart', function(e){
    if($(e.target).closest('.raul-header-app-switcher').length === 1 || $(e.target).closest('.raul-switcher-context').length === 1){
        RAUL.header.closeOtherContexts('switcher');
    } else if($(e.target).closest('.raul-left-navigation').length === 1 || $(e.target).closest('.raul-header-menu-button').length === 1) {
        RAUL.header.closeOtherContexts('navigation');
    } else {
        RAUL.header.closeOtherContexts('document');
    }
});

window.addEventListener('storage', function(e) {
    console.log(e);
});

RAUL.$mainContainer = $('#raul-page-container');

RAUL.header.$header = $('#raul-header');
RAUL.header.$breadcrumbsDropdown = $('#raul-page-header').find('#raul-page-header-breadcrumbs-dropdown');
RAUL.header.$breadcrumbBack = $('#raul-page-header').find('#raul-page-header-breadcrumbs-back');

RAUL.header.$leftnav = $('#raul-left-navigation');
RAUL.header.$leftnavTrigger = RAUL.header.$header.find('#raul-header-menu-button');
var theCookie = "; " + document.cookie;
var cookieParts = theCookie.split("; expandedLeftNav=");
if (cookieParts.length == 2) {
    var $navState = cookieParts.pop().split(";").shift();
}

if (cookieParts.length != 2) {
    if ($('body').hasClass('raul-left-navigation-expanded')) {
        document.cookie = 'expandedLeftNav=true';
    } else {
        document.cookie = 'expandedLeftNav=false';
    }
}
$(window).resize(function() {
    if ($(window).width() > 768) {
        $('body').removeClass('raul-left-navigation-mobile-open');
    }
});
//$(document).ready(function () {
    //set initials to avatar icon
    //var userName = $('#raul-header-user-handle').text();
    //var spacePos = userName.indexOf(' ');
    //var initials = userName.charAt(0) + userName.charAt(spacePos + 1);
    //$('.raul-header-user-avatar').text(initials);
//});
// Left Nav Behaviors
RAUL.header.changeNavState = function(){
    if ($(window).width() > 768) {
        if ($('body').hasClass('raul-left-navigation-expanded')) {
            $('body').removeClass('raul-left-navigation-expanded');
            document.cookie = 'expandedLeftNav=false';
        } else {
            $('body').addClass('raul-left-navigation-expanded');
            document.cookie = 'expandedLeftNav=true';
        }
    } else {
        $('body').toggleClass('raul-left-navigation-mobile-open');
    }
};

RAUL.header.setLeftNavBehavior = function(){
    RAUL.header.$leftnavTrigger.off().on('click', function(e) {
        e.stopPropagation();
        RAUL.header.changeNavState();
        var arrow = $('.raul-left-navigation-item-arrow');
        var subItems = $('.raul-left-navigation-subitems');
        if (subItems.hasClass('subitems-open') && ($('body').hasClass('raul-left-navigation-expanded') || $('body').hasClass('raul-left-navigation-mobile-open')) && $(window).width() > 768) {
            arrow.removeClass('fa-angle-up').addClass('fa-angle-down');
            subItems.removeClass('subitems-open');
        }
        $('.raul-left-navigation-item').css('transition', 'width 0.5s');
        setTimeout(function(){
            $('.raul-left-navigation-item').removeAttr('style');
        }, 600);
    });
};

RAUL.header.setLeftNavBehavior();

setTimeout(function(){
    RAUL.header.$leftnav.show();
},0);




// Logo Behaviors
RAUL.header.$logo = RAUL.header.$header.find('#raul-header-logo');




// Title Behaviors
RAUL.header.$title = RAUL.header.$header.find('#raul-header-title');




// Search Behaviors
RAUL.header.$search = RAUL.header.$header.find('#raul-header-search-input');
RAUL.header.$searchTrigger = RAUL.header.$header.find('#raul-header-search-icon');
RAUL.header.$searchMobile = RAUL.header.$header.find('#raul-header-search-input-mobile');
RAUL.header.$searchMobileTrigger = RAUL.header.$header.find('#raul-header-search-icon-mobile');

RAUL.header.$searchMobileTrigger.on('click', function(e){
    RAUL.header.$leftnavTrigger.fadeOut(300);
    RAUL.header.$logo.fadeOut(300);
    RAUL.header.$title.fadeOut(300);
    RAUL.header.$searchMobileTrigger.fadeOut(300);
    RAUL.header.$userTrigger.fadeOut(300).delay().removeClass('d-sm-block');
    RAUL.header.$switcherTrigger.fadeOut(300);
    RAUL.header.$notificationsTrigger.fadeOut(300);
    RAUL.header.$moreTrigger.fadeOut(300);
    setTimeout(function() {
        RAUL.header.$searchMobile.css({'width':'100%','padding':'0 10px'}).focus();
    }, 300);
});
RAUL.header.$searchMobile.on('blur',function(){
    RAUL.header.$searchMobile.css({'width':'0','padding':'0'});
    setTimeout(function() {
        RAUL.header.$leftnavTrigger.fadeIn();
        RAUL.header.$logo.fadeIn();
        RAUL.header.$title.fadeIn();
        RAUL.header.$searchMobileTrigger.fadeIn();
        RAUL.header.$userTrigger.fadeIn().delay().addClass('d-sm-block');
        RAUL.header.$switcherTrigger.fadeIn(300);
        RAUL.header.$notificationsTrigger.fadeIn(300);
        RAUL.header.$moreTrigger.fadeIn(300);
    }, 300);
});




// Home Icon Behaviors
RAUL.header.$homeTrigger = RAUL.header.$header.find('#raul-header-home');

RAUL.header.$homeTrigger.on('click', function(e){
    //e.stopPropagation();
});




// App Switcher Behaviors
RAUL.header.$switcher = $('#raul-switcher-context');
RAUL.header.$switcherTrigger = RAUL.header.$header.find('#raul-header-app-switcher');

RAUL.header.$switcherTrigger.on('click', function(e){
    if(RAUL.header.$switcher.is(":visible") && e.target.id === 'raul-header-app-switcher'){
        RAUL.header.$switcher.fadeOut();
    } else{
        if(e.target.id === 'raul-header-app-switcher'){
            var _all = '', _favorites = '';
            var _families = [];
            var $all = $('<div class="apps-all"></div>');
            for(var i = 0, app; app = RAUL.header.$switcher.data[i]; i++){
                if(_families.indexOf(app.family) === -1 && app.family !== ''){
                    _families.push(app.family);
                }
            }

            for(var i = 0, family; family = _families[i]; i++){

                _all += '<div class="family">';
                _all +=     '<h3 class="family-name ' + family.split(' ').join('-').replace(/&-/g,"").toLowerCase() + '">';
                _all +=         '<i class="family-icon"></i>';
                _all +=         '<span class="family-text">' + family + '</span>'+
                            '</h3>'+
                            '<div class="products">';
                                for(var j = 0, app; app = RAUL.header.$switcher.data[j]; j++){
                                    if(app.family === family){
                                        _all += '<a class="product-url" href="' + app.productUrl + '">';
                                        _all +=     '<span class="product ' + app.productName.split(' ').join('-').replace(/&-/g,"").toLowerCase() + '">';
                                        _all +=         '<i class="product-icon fa fa-times"></i>';
                                        if(app.productName.length > 32){
                                            _all +=     '<span class="product-name">' + app.productName.substring(0, 32) + '... </span>';
                                        } else{
                                            _all +=     '<span class="product-name">' + app.productName + '</span>';
                                        }
                                        _all +=     '</span>';
                                        _all += '</a>';
                                    }

                                }
                _all +=     '</div>';
                _all += '</div>';

            }

            _favorites += '<div class="products">';
            for(var j = 0, app; app = RAUL.header.$switcher.data[j]; j++){
                if(app.isFavorite){
                    _favorites +=   '<a class="product-url" href="' + app.productUrl + '">';
                    _favorites +=       '<span class="product">';
                    _favorites +=           '<i class="product-icon fa fa-times"></i>';
                     if(app.productName.length > 32){
                          _favorites +=     '<span class="product-name">' + app.productName.substring(0, 32) + '... </span>';
                     } else{
                          _favorites +=     '<span class="product-name">' + app.productName + '</span>';
                     }
                    _favorites +=       '</span>';
                    _favorites +=   '</a>';
                }
            }
            _favorites += '</div>';

            RAUL.header.$switcher.find('#apps-all').html(_all);
            RAUL.header.$switcher.find('#apps-favorites').html(_favorites);
        }

        RAUL.header.$switcherTrigger.closest('.icon-container').append(RAUL.header.$switcher.css({
            'position':'absolute',
            'right': -30,
            'top': ( RAUL.header.$switcherTrigger.height() + 35 )
        }).fadeIn())
    }
    e.preventDefault();
    RAUL.header.closeOtherContexts('switcher');
});




// Help Behaviors
RAUL.header.$helpTrigger = RAUL.header.$header.find('#raul-header-help');

RAUL.header.$helpTrigger.on('click', function(e){
    //e.stopPropagation();
});




// Settings Behaviors
RAUL.header.$settingsTrigger = RAUL.header.$header.find('#raul-header-settings');

RAUL.header.$settingsTrigger.on('click', function(e){
    //e.stopPropagation();
});




// Shopping Behaviors
RAUL.header.$shopping = $('#raul-shopping-context');
RAUL.header.$shoppingTrigger = RAUL.header.$header.find('#raul-header-shopping');

if(!RAUL.header.$shopping){
    RAUL.header.$shoppingTrigger.on('click', function(e){
        //e.stopPropagation();
    });
} else {
    RAUL.header.$shoppingTrigger.on('click',function (e) {
        if(RAUL.header.$shopping.is(":visible")){
            RAUL.header.$shopping.fadeOut();
        } else{
            RAUL.header.$shoppingTrigger.closest('.icon-container').append(RAUL.header.$shopping.css({
                'position':'absolute',
                'right': 0 ,
                'top': ( RAUL.header.$shoppingTrigger.height() + 34 )
            }).fadeIn());
        }
        e.stopPropagation();
        e.preventDefault();
        RAUL.header.closeOtherContexts('shopping');
    });
    RAUL.header.$shopping.on('click', function(e){
        e.stopPropagation();
    });
}




// Notifications Behaviors
RAUL.header.$notifications = $('#raul-notifications-context');
RAUL.header.$notificationsTrigger = RAUL.header.$header.find('#raul-header-notifications');

if(!RAUL.header.$notifications){
    RAUL.header.$notificationsTrigger.on('click', function(e){
        e.stopPropagation();
    });
} else {
    RAUL.header.$notificationsTrigger.on('click',function (e) {
        if(RAUL.header.$notifications.is(":visible")){
            RAUL.header.$notifications.fadeOut();
        } else{
            RAUL.header.$notificationsTrigger.closest('.icon-container').append(RAUL.header.$notifications.css({
                'position':'absolute',
                'right': 0 ,
                'top': ( RAUL.header.$notificationsTrigger.height() + 36 )
            }).fadeIn())
        }
        e.stopPropagation();
        e.preventDefault();
        RAUL.header.closeOtherContexts('notifications');
    });
    RAUL.header.$notifications.on('click', function(e){
        e.stopPropagation();
    });
}




// User Behaviors
RAUL.header.$user = $('#raul-user-context');
RAUL.header.$userTrigger = RAUL.header.$header.find('#raul-header-user');

RAUL.header.$userTrigger.on('click', function(e){

    var _icon = $(this);
    if(RAUL.header.$user.is(":visible")){
        RAUL.header.$user.slideUp();
    } else{
        RAUL.header.$userTrigger.append(RAUL.header.$user.css({
            'position':'absolute',
            'right': 0 ,
            'top': ( RAUL.header.$header.height() )
        }).slideDown())

    }
    e.stopPropagation();
    RAUL.header.closeOtherContexts('user');
});
RAUL.header.$user.on('click', function(e){
    e.stopPropagation();
});




// More Behaviors
RAUL.header.$more = $('#raul-header-more-context');
RAUL.header.$moreTrigger = RAUL.header.$header.find('#raul-header-more');

RAUL.header.$moreTrigger.on('click', function(e){
    if(RAUL.header.$more.css('width') === "260px"){
        RAUL.header.$more.css('width','0px');
        RAUL.header.$more.css('box-shadow','none');
    } else {
        RAUL.header.$more.css('width','260px');
        RAUL.header.$more.css('box-shadow','rgba(0, 0, 0, .2) -4px 0 16px 0');
    }
    e.stopPropagation();
    RAUL.header.closeOtherContexts('more');

});
RAUL.header.$more.on('click', function(e){
    e.stopPropagation();
});




// More User Behaviors
RAUL.header.$moreUser = RAUL.header.$user.clone();
RAUL.header.$moreUserTrigger = $('#raul-header-more-context').find('.raul-header-user');

if(RAUL.header.$moreUserTrigger){
    RAUL.header.$moreUser.addClass('hidden-sm-up').css({
        'background': '#fff',
        'border': 'none',
        'margin':'0',
        'width':'100%',
        'height':'auto',
        'max-height':'200px',
        'overflow-y':'auto',
        'box-shadow':'none'
    });
    RAUL.header.$moreUserTrigger.on('click',function (e) {
        if(RAUL.header.$moreUser.is(":visible")){
            RAUL.header.$moreUser.slideUp();
        } else{
            RAUL.header.$moreUser.slideDown();
        }
        e.stopPropagation();
    });
}
RAUL.header.$moreUserTrigger.after(RAUL.header.$moreUser);




// More Home Behaviors
RAUL.header.$moreHometrigger = $('#raul-header-more-context').find('.raul-header-context-home');

if(RAUL.header.$moreHometrigger) {
    RAUL.header.$moreHometrigger.on('click', function(e){
        e.stopPropagation();
    });
}




RAUL.header.$moreHelpTrigger = $('#raul-header-more-context').find('.raul-header-context-help');
if(RAUL.header.$moreHelpTrigger) {
    RAUL.header.$moreHelpTrigger.on('click', function(e){
        e.stopPropagation();
    });
}

RAUL.header.$moreSettingsTrigger = $('#raul-header-more-context').find('.raul-header-context-help');
if(RAUL.header.$moreSettingsTrigger) {
    RAUL.header.$moreSettingsTrigger.on('click', function(e){
        e.stopPropagation();
    });
}



RAUL.header.$moreShopping = RAUL.header.$shopping.clone();
RAUL.header.$moreShoppingTrigger = $('#raul-header-more-context').find('.raul-header-context-shopping');
if(RAUL.header.$moreShoppingTrigger) {
    if(!RAUL.header.$moreShopping){
        RAUL.header.$moreShoppingTrigger.on('click',function () {
            window.location = RAUL.header.$moreShoppingTrigger.data('url');
        });
    } else {
        RAUL.header.$moreShopping.addClass('hidden-xl-up').css({
            'background': '#E4E6E7',
            'border': 'none',
            'margin': '0',
            'width': '100%',
            'height': 'auto',
            'max-height': '200px',
            'overflow-y': 'auto'
        });
        RAUL.header.$moreShoppingTrigger.on('click', function (e) {
            var _icon = $(this);
            if (RAUL.header.$moreShopping.is(":visible")) {
                RAUL.header.$moreShopping.slideUp();
            } else {
                RAUL.header.$moreShopping.slideDown();
            }
            e.stopPropagation();
        });
    }
}
RAUL.header.$moreShoppingTrigger.after(RAUL.header.$moreShopping);


RAUL.header.$moreNotifications = RAUL.header.$notifications.clone();
RAUL.header.$moreNotificationsTrigger = $('#raul-header-more-context').find('.raul-header-context-notifications');
if(RAUL.header.$moreNotificationsTrigger) {

    if(!RAUL.header.$moreNotifications){
        RAUL.header.$moreNotificationsTrigger.on('click',function () {
            window.location = RAUL.header.$moreNotificationsTrigger.data('url');
        });
    } else {
        RAUL.header.$moreNotifications.addClass('hidden-xl-up').css({
            'background': '#E4E6E7',
            'border': 'none',
            'margin': '0',
            'width': '100%',
            'height': 'auto',
            'max-height': '200px',
            'overflow-y': 'auto'
        });

        RAUL.header.$moreNotificationsTrigger.on('click', function (e) {
            if (RAUL.header.$moreNotifications.is(":visible")) {
                RAUL.header.$moreNotifications.slideUp();
            } else {
                RAUL.header.$moreNotifications.slideDown();
            }
            e.stopPropagation();
        });
    }

}
RAUL.header.$moreNotificationsTrigger.after(RAUL.header.$moreNotifications);

if (typeof RAUL === 'undefined') { var RAUL = { leftnav: {} }; }
else { RAUL.leftnav = {}; }

RAUL.leftnav.$leftnav = $('#raul-left-navigation');

RAUL.leftnav.$leftnav.find('.has-subitems').each(function(){
    $(this).on('click', function(){
        if($('body').hasClass('raul-left-navigation-expanded') == true || $('body').hasClass('raul-left-navigation-mobile-open')) {
            var arrow = $(this).find('.raul-left-navigation-item-arrow');
            var subItems = $(this).parent().find(' > .raul-left-navigation-subitems');
            if (subItems.hasClass('subitems-open') == false) {
                $(this).addClass('active');
                subItems.slideDown(500);
                arrow.removeClass('fa-angle-down').addClass('fa-angle-up');
                setTimeout(function(){
                    subItems.addClass('subitems-open');
                    subItems.removeAttr('style');

                }, 600);
            } else {
                $(this).removeClass('active');
                subItems.slideUp(500);
                arrow.removeClass('fa-angle-up').addClass('fa-angle-down');
                setTimeout(function(){
                    subItems.removeAttr('style');
                    subItems.removeClass('subitems-open');
                }, 600);
            }
        }
    });
});

if(localStorage.getItem('nav-theme') === 'dark'){
    RAUL.leftnav.$leftnav.removeClass('raul-left-navigation-light');
    RAUL.leftnav.$leftnav.addClass('raul-left-navigation-dark');
} else if(localStorage.getItem('nav-theme') === 'light'){
    RAUL.leftnav.$leftnav.removeClass('raul-left-navigation-dark');
    RAUL.leftnav.$leftnav.addClass('raul-left-navigation-light');
}

RAUL.leftnav.setHeight = function(){
    var _height = 0;
    RAUL.leftnav.$leftnav.find('.raul-left-navigation-item').each(function(i,e){
        _height += $(e).outerHeight();
    });
    if(RAUL.leftnav.$leftnav.height() < _height && !isTouch){
        //RAUL.leftnav.$leftnav.find('#raul-left-navigation-items').width('315px');
    } else {
        //RAUL.leftnav.$leftnav.find('#raul-left-navigation-items').width('300px');
    }
};

RAUL.leftnav.setHeight();

$(window).on('resize', function(e) {
    // Bootstrap 4 Beta Mobile Breakpoint
    if(e.currentTarget.outerWidth < 768){
        if(localStorage.getItem('nav-pinned') === 'true'){
            localStorage.setItem('nav-pinned', false);
            RAUL.header.setLeftNavBehavior();
        }
    } else {
        if(localStorage.getItem('nav-pinned') === 'false'){
            localStorage.setItem('nav-pinned', true);
            RAUL.header.setLeftNavBehavior();
        }
    }
    RAUL.leftnav.setHeight();
});

if (typeof RAUL === 'undefined') { var RAUL = { pageheader: {} }; }
else { RAUL.pageheader = {}; }

RAUL.pageheader.$pageHeader = $('#raul-page-header');
RAUL.pageheader.$breadcrumbsDropdown = RAUL.pageheader.$pageHeader.find('#raul-page-header-breadcrumbs-dropdown');
RAUL.pageheader.$breadcrumbBack = RAUL.pageheader.$pageHeader.find('#raul-page-header-breadcrumbs-back');

RAUL.pageheader.$breadcrumbBack.on('click', function(e){
    if(!RAUL.pageheader.$breadcrumbsDropdown.is(':visible')){
        RAUL.pageheader.$breadcrumbsDropdown.slideDown();
    } else {
        RAUL.pageheader.$breadcrumbsDropdown.slideUp();
    }
    e.stopPropagation();
    RAUL.header.closeOtherContexts('breadcrumbs');
});

(function($){
    /* Additional Code for App Switcher BEGIN */
    $(".raul-header-app-switcher").click(function(){
        $(".app-switcher-menu").toggle();
    });
    /* Additional Code for App Switcher END */

    /* Additional Code for leftNavToggle BEGIN */
    if(localStorage.getItem('nav-theme') === 'dark'){
        $('.leftNavToggle i').removeClass('fa-toggle-off');
        $('.leftNavToggle i').addClass('fa-toggle-on');
    }

    $('.leftNavToggle i').on('click', function() {
        if($(this).hasClass('fa-toggle-off')){
            $(this).removeClass('fa-toggle-off');
            $(this).addClass('fa-toggle-on');
            RAUL.leftnav.$leftnav.removeClass('raul-left-navigation-light');
            RAUL.leftnav.$leftnav.addClass('raul-left-navigation-dark');
            localStorage.setItem('nav-theme', 'dark');
        } else {
            $(this).removeClass('fa-toggle-on');
            $(this).addClass('fa-toggle-off');
            RAUL.leftnav.$leftnav.removeClass('raul-left-navigation-dark');
            RAUL.leftnav.$leftnav.addClass('raul-left-navigation-light');
            localStorage.setItem('nav-theme', 'light');
        }
    });
    /* Additional Code for leftNavToggle BEGIN */

})(jQuery);

