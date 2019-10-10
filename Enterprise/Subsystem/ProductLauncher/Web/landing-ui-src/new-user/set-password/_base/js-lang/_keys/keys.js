(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "label.passwordExpired",
            "label.instructions",
            "label.rules",
            "label.placeholder.newPassword",
            "label.placeholder.confirmPassword",
            "label.setPassword",
            "label.cancel",
            "label.charRange",
            "label.upperCase",
            "label.lowerCase",
            "label.numerical",
            "label.specialCharacter",
            "label.inputPassword",
            "label.confirmPassword",

            "errorMsgs.password.incompleteRequirements",
            "errorMsgs.password.noSameUsername",
            "errorMsgs.password.historyRecord",
            "errorMsgs.confirmPassword.mismatch",
            "errorMsgs.confirmPassword.required"
        ];

        appLangKeys.app("setPassword").set(keys);
    }

    angular
        .module("new-user")
        .config(["appLangKeysProvider", config]);
})();
