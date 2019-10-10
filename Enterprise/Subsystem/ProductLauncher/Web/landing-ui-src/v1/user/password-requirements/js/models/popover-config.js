//  Password Requirements Model

(function (angular, undefined) {
    "use strict";

    function factory(popoverConfig) {
        return popoverConfig({
            trigger: "manual",
            placement: "auto bottom",
            instName: "passReqPopover",
            templateUrl: "user/password-requirements/templates/password-requirements.html"
        });
    }

    angular
        .module("settings")
        .factory("passReqPopoverConfig", ["rpPopoverConfig", factory]);
})(angular);
