//  User Tabs Data Constant

(function (angular) {
    "use strict";

    var userTabsData = {
        userDetails: {
            isActive: true,
            id: "userDetails",
            text: "User Details",
            templateUrl: "user/user-details/templates/user-details.html"
        },

        productAccess: {
            isActive: false,
            id: "productAccess",
            text: "Product Access",
            templateUrl: "user/product-access/templates/product-access.html"
        },

        securityQuestions: {
            isActive: false,
            id: "securityQuestions",
            text: "Security Questions",
            templateUrl: "user/security-questions/templates/security-questions.html"
        },

        resetPassword: {
            isActive: false,
            id: "resetPassword",
            text: "Password",
            templateUrl: "user/reset-password/templates/reset-password.html"
        },

        activityLog: {
            isActive: false,
            id: "activityLog",
            text: "Activity",
            templateUrl: "common/activity-log/templates/index.html"
        },

        userPreference: {
            isActive: false,
            id: "userPreference",
            text: "User Preference",
            templateUrl: "user/user-preference/index.html"
        }
    };

    angular
        .module("settings")
        .value("userTabsData", userTabsData);
})(angular);
