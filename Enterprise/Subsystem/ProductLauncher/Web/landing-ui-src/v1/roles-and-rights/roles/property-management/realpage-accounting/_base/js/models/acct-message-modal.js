//  acct Status Message Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("roles-and-rights/roles/property-management/realpage-accounting/base/templates/delete-status-message.html");
    }

    angular
        .module("settings")
        .factory("acctStatusMsgModal", ["rpModalModel", factory]);
})(angular);
