(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "label.instructions",
            "label.rules",
            "label.placeholder.newPassword",
            "label.placeholder.confirmPassword",
            "label.changePassword",
            "label.charRange",
            "label.upperCase",
            "label.lowerCase",
            "label.numerical",
            "label.specialCharacter",

            "errorMsgs.password.incompleteRequirements",
            "errorMsgs.password.noSameUsername",
            "errorMsgs.password.historyRecord",
            "errorMsgs.confirmPassword.mismatch",
            "errorMsgs.confirmPassword.required"
        ];

        appLangKeys.app("changePassword").set(keys);
    }

    angular
        .module("identity")
        .config(["appLangKeysProvider", config]);
})();