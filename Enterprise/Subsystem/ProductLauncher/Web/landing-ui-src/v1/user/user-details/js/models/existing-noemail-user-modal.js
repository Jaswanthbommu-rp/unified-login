//  External User Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/user-details/templates/existing-user-noemail-modal.html");
    }

    angular
        .module("settings")
        .factory("existingNoEmailUserModal", ["rpModalModel", factory]);
})(angular);
