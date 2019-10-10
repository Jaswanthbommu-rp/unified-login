(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "label.instructions",
            "label.rules", 
            "label.placeholder.currentPassword",
            "label.placeholder.newPassword", 
            "label.placeholder.confirmPassword", 
            "label.currentPassword",
            "label.newPassword",
            "label.confirmPassword",
            "label.changePassword", 
            "label.cancel", 
            "label.charRange", 
            "label.upperCase", 
            "label.lowerCase", 
            "label.numerical", 
            "label.specialCharacter",
            "label.historyRecord",
            
            "errorMsgs.currentPassword.required",
            "errorMsgs.password.incompleteRequirements", 
            "errorMsgs.password.noSameUsername",
            "errorMsgs.password.historyRecord",
            "errorMsgs.confirmPassword.mismatch",
            "errorMsgs.confirmPassword.required"
        ];

        appLangKeys.app("people.changePassword").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();