(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("changePassword");

        bundle.set({
            "label.instructions": "Enter a new password for {{email}}.",
            "label.rules": "Password must have at least:",
            "label.placeholder.newPassword": "Create Password",
            "label.placeholder.confirmPassword": "Confirm Password",
            "label.changePassword": "Change Password",
            "label.charRange": "{{min}} - {{max}} characters",
            "label.upperCase": "1 upper-case letter",
            "label.lowerCase": "1 lower-case letter",
            "label.numerical": "1 number",
            "label.specialCharacter": "1 special character (such as !@#$%)",

            "errorMsgs.password.incompleteRequirements": "Your password does not meet the minimum requirements.",
            "errorMsgs.password.noSameUsername": "Your password cannot be the same as your username.",
            "errorMsgs.password.historyRecord": "This password has already been used. Please try again.",
            "errorMsgs.confirmPassword.mismatch": "The passwords you typed do not match. Please try again.",
            "errorMsgs.confirmPassword.required": "Please re-enter new password."
        });

        bundle.test();
    }

    angular
        .module("identity")
        .config(["appLangBundleProvider", config]);
})();