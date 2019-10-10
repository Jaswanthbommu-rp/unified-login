//  Unified Login Status Message Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("roles-and-rights/roles/administration/unified-login/base/templates/delete-status-message.html");
    }

    angular
        .module("settings")
        .factory("ulStatusMsgModal", ["rpModalModel", factory]);
})(angular);
