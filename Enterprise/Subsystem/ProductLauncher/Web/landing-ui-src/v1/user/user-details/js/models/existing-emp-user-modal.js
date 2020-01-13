//  Existing Employee User Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/user-details/templates/existing-emp-user-modal.html");
    }

    angular
        .module("settings")
        .factory("existingEmpUserModal", ["rpModalModel", factory]);
})(angular);
