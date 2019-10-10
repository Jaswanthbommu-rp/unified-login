//  Companies Profile Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("common/company-switch/templates/companies-modal.html");
    }

    angular
        .module("settings")
        .factory("compSwitchModal", ["rpModalModel", factory]);
})(angular);
