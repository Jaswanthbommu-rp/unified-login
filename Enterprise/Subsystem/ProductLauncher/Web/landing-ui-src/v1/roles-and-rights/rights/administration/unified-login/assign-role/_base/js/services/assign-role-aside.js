//  Assign Role Aside Service

(function(angular, undefined) {
    "use strict";


    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "roles-and-rights/rights/administration/unified-login/assign-role/base/templates/index.html"
        };

        return asideModal().setData(modalData);

    }

    angular
        .module("settings")
        .factory("umAssignRolesToRightsAside", ["rpAsideModal", factory]);
})(angular);