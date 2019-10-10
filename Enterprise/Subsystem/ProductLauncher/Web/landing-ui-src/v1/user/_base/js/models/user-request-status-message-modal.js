//  User Request Status Message Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/base/templates/user-request-status-message.html");
    }

    angular
        .module("settings")
        .factory("userReqStatusMsgModal", ["rpModalModel", factory]);
})(angular);
