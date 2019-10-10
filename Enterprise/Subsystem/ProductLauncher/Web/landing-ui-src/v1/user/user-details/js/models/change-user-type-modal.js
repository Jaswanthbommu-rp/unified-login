//  Change User Type Modal

(function (angular, undefined) {
    "use strict";

    function factory(modal) {
        return modal("user/user-details/templates/change-user-type-modal.html");
    }

    angular
        .module("settings")
        .factory("chgUserTypeModal", ["rpModalModel", factory]);
})(angular);
