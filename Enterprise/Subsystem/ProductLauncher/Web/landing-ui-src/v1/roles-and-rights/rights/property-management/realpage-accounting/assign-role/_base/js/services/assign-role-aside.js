//  Assign Role Aside Service

(function(angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "roles-and-rights/rights/property-management/realpage-accounting/assign-role/base/templates/index.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("acctAssignRolesToRightsAside", ["rpAsideModal", factory]);
})(angular);