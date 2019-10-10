//  Manage User Form Data Model

(function (angular) {
    "use strict";

    function factory(userTypes, moment) {
        return {
            realPageId: null,
            firstName: null,
            middleInitial: null,
            lastName: null,

            username: null,
            password: null,
            confirmPassword: null,
            notificationEmail: null,

            startDate: moment(),
            endDate: null,

            userType: userTypes.REGULAR.id,
            enterpriseRole: null,
            isEnabled: true,

            personas: {},
            inactivePersona: []
        };
    }

    angular
        .module("settings")
        .factory("manageUserFormData", [
            "userTypes",
            "moment",
            factory
        ]);
})(angular);
