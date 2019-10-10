//  New Role Aside Service


(function(angular, undefined) {
    "use strict";

    function factory(rpAsideModal) {
        return rpAsideModal("roles-and-rights/roles/property-management/realpage-accounting/clone-role/base/templates/index.html");
    }

    angular
        .module("settings")
        .factory("acctCloneRoleAside", ["rpAsideModal", factory]);
})(angular);