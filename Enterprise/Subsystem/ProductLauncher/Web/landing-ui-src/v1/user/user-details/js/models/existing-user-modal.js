//  Existing User Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/user-details/templates/existing-user-modal.html");
    }

    angular
        .module("settings")
        .factory("existingUserModal", ["rpModalModel", factory]);
})(angular);
