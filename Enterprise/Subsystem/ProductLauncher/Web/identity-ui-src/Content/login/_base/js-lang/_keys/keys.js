(function (angular) {
    "use strict";

    function config(appLangKeys) {
        var keys = [

	        "login_username",
            "login_password",
            "login_remember_me",
            "login_btn",
            "login_forgot",

            "login_username_req",
            "login_password_req",

            "login_change_pwd_success",
            "login_profile_update_success",

            "login_alert_autologout_1",
            "login_alert_autologout_2"

        ];

        appLangKeys.app("login").set(keys);
    }

    angular
        .module("identity")
        .config(["appLangKeysProvider", config]);
})(angular);
