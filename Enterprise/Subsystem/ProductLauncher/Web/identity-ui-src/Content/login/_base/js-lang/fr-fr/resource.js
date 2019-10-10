//  English Resource Bundle

(function (angular) {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("fr-fr").app("login");

        bundle.set({
            login_username: "Username FR",
            login_password: "Password FR",
            login_remember_me: "souviens-toi de moi",
            login_btn: "Login FR",
            login_forgot: "Forgot Password? FR",

            login_change_pwd_success: "You have successfully changed your password. Please log in now. FR",
            login_profile_update_success: "Your profile has been updated please login. FR",

            login_alert_autologout_1: "You have been logged out of the system because your account has expired. FR",
            login_alert_autologout_2: "If you believe this was an error, please contact your system administrator. FR",
        });

        bundle.test();
    }

    angular
        .module("identity")
        .config(["appLangBundleProvider", config]);
})(angular);
