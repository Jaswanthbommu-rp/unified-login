(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
           "system_err_contact_admin"
        ];

        appLangKeys.app("newUserValidation").set(keys);
    }

    angular
        .module("new-user")
        .config(["appLangKeysProvider", config]);
})();
