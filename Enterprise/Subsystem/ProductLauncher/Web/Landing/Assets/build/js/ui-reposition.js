'use strict';

(function ($) {
    "use strict";

    // Javascript for keeping the footer at the bottom of a scrolling Aside

    var repositionAsideActionFooter = function repositionAsideActionFooter() {
        var offset = $('.responsive-aside-scrolling').scrollTop();
        $('.responsive-aside-scrolling .stay-visible-bottom').css({
            bottom: -1 * offset + 'px'
        });
    };

    // Javascript for keeping the footer at the bottom of a scrolling content, but also above the footer content

    var repositionActionFooter = function repositionActionFooter() {
        console.log("running the reposition");
        var down = $(window).height() + 234;
        if ($(window).scrollTop() > $(document).height() - down) {
            $('.stay-visible-bottom').css('bottom', '-40');
            $('.stay-visible-bottom').css('position', 'absolute');
            $('.stay-visible-bottom').css('padding-left', '0');
            $('.stay-visible-bottom').css('width', '100%');
        } else {
            $('.stay-visible-bottom').css('bottom', '0');
            $('.stay-visible-bottom').css('position', 'fixed');
            $('.stay-visible-bottom').css('padding-left', $('.raul-page-container').css('margin-left'));
            $('.stay-visible-bottom').css('width', '100%');
        }
    };

    window.repositionActionFooter = repositionActionFooter;

    //window.addEventListener('scroll', repositionActionFooter);
    //window.addEventListener('resize', repositionActionFooter);

    $(window).scroll(function () {
        repositionActionFooter();
    });

    $(window).resize(function () {
        repositionActionFooter();
    });

    $(window).ready(function () {
        repositionActionFooter();
    });

    $('.responsive-aside-scrolling').scroll(function () {
        repositionAsideActionFooter();
    });
})(jQuery);