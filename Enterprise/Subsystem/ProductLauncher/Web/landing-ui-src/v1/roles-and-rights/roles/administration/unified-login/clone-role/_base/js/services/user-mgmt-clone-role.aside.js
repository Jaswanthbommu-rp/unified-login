//  Clone Role Aside Service


(function (angular, undefined) {
    "use strict";

   function factory(rpAsideModal) {
        return rpAsideModal("roles-and-rights/roles/administration/unified-login/clone-role/base/templates/index.html");
    }

    angular
        .module("settings")
        .factory("userMgmtCloneRoleAside", ["rpAsideModal", factory]);
})(angular);
