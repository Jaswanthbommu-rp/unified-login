//  New Role Aside Service


(function(angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "roles-and-rights/roles/property-management/spend-management/new-role/base/templates/index.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("spndmgmtNewRoleAside", ["rpAsideModal", factory]);
})(angular);