//  External User Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/user-details/templates/internal-user-modal.html");
    }

    angular
        .module("settings")
        .factory("internalUserModal", ["rpModalModel", factory]);
})(angular);