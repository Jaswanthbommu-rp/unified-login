(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("setPassword");

        bundle.set({
            "label.passwordExpired" : "Your temporary password has expired.",
            "label.instructions": "Before continuing please set a password.",
            "label.rules": "Password must have at least:",
            "label.placeholder.newPassword": "Set Password",
            "label.placeholder.confirmPassword": "Confirm Password",
            "label.setPassword": "Save and Continue",
            "label.cancel": "Cancel",
            "label.charRange": "{{min}} - {{max}} characters",
            "label.upperCase": "1 upper-case letter",
            "label.lowerCase": "1 lower-case letter",
            "label.numerical": "1 number",
            "label.specialCharacter": "1 special character (such as !@#$%)",
            "label.inputPassword": "Password",
            "label.confirmPassword": "Re-enter Password",

            "errorMsgs.password.incompleteRequirements": "Your password does not meet the minimum requirements.",
            "errorMsgs.password.noSameUsername": "Your password cannot be the same as your username.",
            "errorMsgs.password.historyRecord": "This password has already been used. Please try again.",
            "errorMsgs.confirmPassword.mismatch": "The passwords you typed do not match. Please try again.",
            "errorMsgs.confirmPassword.required": "Please re-enter new password."
        });

        bundle.test();
    }

    angular
        .module("new-user")
        .config(["appLangBundleProvider", config]);
})();
