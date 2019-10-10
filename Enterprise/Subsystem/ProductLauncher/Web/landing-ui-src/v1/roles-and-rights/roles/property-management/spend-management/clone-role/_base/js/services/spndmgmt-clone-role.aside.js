//  New Role Aside Service


(function(angular, undefined) {
    "use strict";

    function factory(rpAsideModal) {
        return rpAsideModal("roles-and-rights/roles/property-management/spend-management/clone-role/base/templates/index.html");
    }

    angular
        .module("settings")
        .factory("spndmgmtCloneRoleAside", ["rpAsideModal", factory]);
})(angular);