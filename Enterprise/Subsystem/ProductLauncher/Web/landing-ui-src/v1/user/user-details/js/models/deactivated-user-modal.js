//  External User Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/user-details/templates/deactivated-user-modal.html");
    }

    angular
        .module("settings")
        .factory("deactivatedUserModal", ["rpModalModel", factory]);
})(angular);
