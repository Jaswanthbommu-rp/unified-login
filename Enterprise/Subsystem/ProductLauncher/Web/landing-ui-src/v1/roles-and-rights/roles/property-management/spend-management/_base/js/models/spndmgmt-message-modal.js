//  spndmgmt Status Message Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("roles-and-rights/roles/property-management/spend-management/base/templates/delete-status-message.html");
    }

    angular
        .module("settings")
        .factory("spndmgmtStatusMsgModal", ["rpModalModel", factory]);
})(angular);
