//  Assign Role Aside Service

(function(angular, undefined) {
    "use strict";
    
    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "roles-and-rights/roles/property-management/realpage-accounting/assign-role/base/templates/index.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("acctAssignRoleAside", ["rpAsideModal", factory]);
})(angular);