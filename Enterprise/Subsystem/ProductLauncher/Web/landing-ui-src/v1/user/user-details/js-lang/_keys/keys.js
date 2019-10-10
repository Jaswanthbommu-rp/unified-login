// User Details Keys

(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "text.accessDetails.title",

            "text.profileInfo.title",

            "label.firstName",
            "label.middleName",
            "label.lastName",
            "label.userTypeId",
            "label.loginName",
            "label.password",
            "label.passwordCopy",
            "label.notificationEmail",
            "label.fromDate",
            "label.thruDate",
            "label.isActive",
            "label.is3rdPartyIDP",
           // "label.timeZoneOffset",

            "label.phone",
            "label.phoneType",
            "label.contactMethod",
            "label.industryJobTitle",
            "label.companyJobTitle",

            "options.userTypeId.401",
            "options.userTypeId.402",
            "options.userTypeId.403",
            "options.userTypeId.404",
            "options.userTypeId.405",

            "errorMsgs.firstName.required",
            "errorMsgs.fromDate.required",
            "errorMsgs.lastName.required",
            "errorMsgs.loginName.invalidLoginName",
            "errorMsgs.loginName.required",
            "errorMsgs.notificationEmail.pattern",
            "errorMsgs.password.required",
            "errorMsgs.passwordCopy.passwordsMatch",
            "errorMsgs.passwordCopy.required",
            "errorMsgs.userTypeId.required",
           // "errorMsgs.TimeZoneOffset.required",

            "User.CreateUser.1",
            "User.CreateUser.2",
            "User.CreateUser.3",
            "User.CreateUser.4",
            "User.CreateUser.5",
            "User.CreateUser.6",
            "User.CreateUser.7",
            "User.CreateUser.8",
            "User.CreateUser.9",
            "User.CreateUser.10",
            "User.CreateUser.11",
            "User.CreateUser.12",
            "User.CreateUser.13",
            "User.CreateUser.14",
			"User.CreateUser.15",
            "User.CreateUser.16",
            "User.CreateUser.17",
            "User.CreateUser.18",
            "User.CreateUser.19",
            "User.CreateUser.20",
			"User.CreateUser.21",
			"User.CreateUser.22",
			"User.CreateUser.23",
			"User.CreateUser.24",
			"User.CreateUser.25",

            "User.UpdateUser.1",
            "User.UpdateUser.2",
            "User.UpdateUser.3",
            "User.UpdateUser.4",
            "User.UpdateUser.5",
            "User.UpdateUser.6",
            "User.UpdateUser.7",
            "User.UpdateUser.8",
            "User.UpdateUser.9",
            "User.UpdateUser.10",
            "User.UpdateUser.11",
            "User.UpdateUser.12",
            "User.UpdateUser.13",
            "User.UpdateUser.14",
            "User.UpdateUser.15",
            "User.UpdateUser.16"
        ];

        appLangKeys.app("user.userDetails").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
