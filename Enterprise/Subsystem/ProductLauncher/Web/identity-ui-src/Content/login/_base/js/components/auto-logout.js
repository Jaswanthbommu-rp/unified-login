// Auto-logout Modal

(function (angular, $) {
    "use strict";

    angular
        .module("identity")
        .component("autoLogoutModal", {
            templateUrl: "login/base/templates/auto-logout-modal.html",
            bindings: {
                messages: "<"
            }
        });
        
})(angular, jQuery);
