(function (angular) {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("login");

        bundle.set({

            login_username: "Username",
            login_password: "Password",
            login_remember_me: "Remember me",
            login_btn: "Log In",
            login_forgot: "Forgot Password?",

            login_username_req: "Username is required",
            login_password_req: "Password is required",

            login_change_pwd_success: "You have successfully changed your password. Please log in now.",
            login_profile_update_success: "Your profile has been updated please login.",

            login_alert_autologout_1: "You have been logged out of the system because your account has expired.",
            login_alert_autologout_2: "If you believe this was an error, please contact your system administrator.",
        });

        bundle.test();
    }

    angular
        .module("identity")
        .config(["appLangBundleProvider", config]);
})(angular);