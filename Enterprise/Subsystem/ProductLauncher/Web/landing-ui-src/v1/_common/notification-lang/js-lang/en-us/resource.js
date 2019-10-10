(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("common.notification-lang");

        bundle.set({

            pwd_notice_change_pwd: "Change Password",

            pwd_notice_change_pwd_title: "Password Expiration Notice",
            pwd_notice_change_pwd_msg: "Your password will expire in <strong>###</strong> days.",
            pwd_notice_change_pwd_last_msg: "Your password will expire <strong>tomorrow</strong>.",

            pwd_notice_new_pwd_title: "Password Successfully Changed",
            pwd_notice_new_pwd_msg: "You have successfully changed your password.",

            pwd_notice_pwd_expired_title: "Password Expired",
            pwd_notice_pwd_expired_msg: "Password Expired. Change password now."

        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
