//  Onesite Status Message Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("roles-and-rights/roles/property-management/onesite/base/templates/delete-status-message.html");
    }

    angular
        .module("settings")
        .factory("onesiteStatusMsgModal", ["rpModalModel", factory]);
})(angular);
