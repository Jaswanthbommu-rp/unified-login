//  Companies admin refresh Message Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("employee-access/base/templates/companies-admin-refresh-status-message.html");
    }

    angular
        .module("settings")
        .factory("compAdminRefreshMsgModal", ["rpModalModel", factory]);
})(angular);
