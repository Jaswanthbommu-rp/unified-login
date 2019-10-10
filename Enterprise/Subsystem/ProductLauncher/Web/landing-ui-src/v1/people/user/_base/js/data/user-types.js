// User Types

(function (angular) {
    "use strict";

    function userTypesFactory($filter) {
        return {
            REGULAR: {
                id: 401,
                label: $filter("manageUserText")("user_type_regular_user")
            },
            RP_SYS_ADMIN: {
                id: 402,
                label: $filter("manageUserText")("user_type_super_user")
            },
            REGULAR_NO_EMAIL: {
                id: 404,
                label: $filter("manageUserText")("user_type_regular_no_email_user")
            },

            //TODO continue after MVP
            SDE: { 
                id: 406,
                label: $filter("manageUserText")("user_type_sde")
            },
            EXTERNAL_USER: { 
                id: 405,
                label: $filter("manageUserText")("user_type_external_user")
            },
            RP_EMPLOYEE: {
                id: 403,
                label: $filter("manageUserText")("user_type_rp_employee")
            }
        };
    }

    angular
        .module("settings")
        .factory("userTypes", [
            "$filter",
            userTypesFactory
        ]);

})(angular);
