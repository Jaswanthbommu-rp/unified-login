(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "errorMsgs.currentPassword.required",
            "errorMsgs.password.required",
            "errorMsgs.password.incompleteRequirements",
            "errorMsgs.password.notSameAsCurrent",
            "errorMsgs.confirmPassword.required",
            "errorMsgs.confirmPassword.mismatch"
        ];

        appLangKeys.app("resetPassword").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();