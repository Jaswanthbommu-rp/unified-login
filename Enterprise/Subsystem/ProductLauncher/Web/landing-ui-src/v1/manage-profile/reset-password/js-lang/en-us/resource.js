(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("resetPassword");

        bundle.set({
            "errorMsgs.currentPassword.required": "Current password is required.",
            "errorMsgs.password.required": "New password is required.",
            "errorMsgs.password.incompleteRequirements": "New password does not meet requirements.",
            "errorMsgs.password.notSameAsCurrent": "New password should not match the current password.",
            "errorMsgs.confirmPassword.required": "You must re-enter your new password.",
            "errorMsgs.confirmPassword.mismatch": "The passwords you typed do not match. Please try again."
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();