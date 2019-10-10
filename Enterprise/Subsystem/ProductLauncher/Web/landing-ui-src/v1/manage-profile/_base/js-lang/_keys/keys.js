(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "cancelBtn",
            "updateBtn",
            "modalTitle",
            "profileTab",
            "resetPasswordTab",
            "securityQuestionsTab"
        ];

        appLangKeys.app("manageProfile").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
