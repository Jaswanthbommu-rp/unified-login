//  New Role Aside Service


(function (angular, undefined) {
    "use strict";

    function factory(asideModal) {
        var modalData = {
            keyboard: false,
            backdrop: "static",
            templateUrl: "roles-and-rights/roles/administration/unified-login/new-role/base/templates/index.html"
        };

        return asideModal().setData(modalData);
    }

    angular
        .module("settings")
        .factory("userMgmtNewRoleAside", ["rpAsideModal", factory]);
})(angular);
