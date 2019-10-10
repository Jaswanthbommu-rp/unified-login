//  External User Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/user-details/templates/external-user-modal.html");
    }

    angular
        .module("settings")
        .factory("externalUserModal", ["rpModalModel", factory]);
})(angular);
