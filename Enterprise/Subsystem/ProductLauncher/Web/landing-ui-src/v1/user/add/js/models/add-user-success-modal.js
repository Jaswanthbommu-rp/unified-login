//  Add User Success Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/add/templates/add-user-success.html");
    }

    angular
        .module("settings")
        .factory("addUserSuccessModal", ["rpModalModel", factory]);
})(angular);
