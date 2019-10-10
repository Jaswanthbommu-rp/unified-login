(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.changePassword");

        bundle.set({
            "label.instructions": "Enter a new password for :username.",
            "label.rules": "Password must have at least:",
            "label.placeholder.currentPassword": "Current Password",
            "label.placeholder.newPassword": "Set Password",
            "label.placeholder.confirmPassword": "Confirm Password",
            "label.currentPassword": "Current Password",
            "label.newPassword": "New Password",
            "label.confirmPassword": "Re-enter new Password",
            "label.changePassword": "Change Password",
            "label.cancel": "Cancel",
            "label.charRange": "{{min}} - {{max}} characters",
            "label.upperCase": "1 upper-case letter",
            "label.lowerCase": "1 lower-case letter",
            "label.numerical": "1 number",
            "label.specialCharacter": "1 special character (such as !@#$%)",
            "label.historyRecord": "Must be different from your previous 5 passwords",

            "errorMsgs.currentPassword.required": "This should match your current password.",
            "errorMsgs.password.incompleteRequirements": "Your password does not meet the minimum requirements.",
            "errorMsgs.password.noSameUsername": "Your password cannot be the same as your username.",
            "errorMsgs.password.historyRecord": "This password has already been used. Please try again.",
            "errorMsgs.confirmPassword.mismatch": "The passwords you typed do not match. Please try again.",
            "errorMsgs.confirmPassword.required": "Please re-enter new password."
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();