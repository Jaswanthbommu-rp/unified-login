//  Change Password Form Data Model
(function (angular) {
    "use strict";

    function factory() {
        return {
            "createPassword": "",
            "confirmPassword": "", 
            "oldPassword": ""
        };
    }

    angular
        .module("settings")
        .factory("changePasswordData", [factory]);
})(angular);
