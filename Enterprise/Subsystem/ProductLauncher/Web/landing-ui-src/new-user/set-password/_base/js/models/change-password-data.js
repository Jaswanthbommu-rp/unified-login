//  Change Password Form Data Model
(function (angular) {
    "use strict";

    function factory() {
        return {
            "createPassword": "",
            "confirmPassword": ""
        };
    }

    angular
        .module("new-user")
        .factory("changePasswordData", [factory]);
})(angular);