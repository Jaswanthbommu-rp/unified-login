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
        .module("identity")
        .factory("changePasswordData", [factory]);
})(angular);