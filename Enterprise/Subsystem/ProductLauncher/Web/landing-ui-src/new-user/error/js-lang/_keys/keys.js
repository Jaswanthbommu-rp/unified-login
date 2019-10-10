(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
           "system_err_contact_admin"
        ];

        appLangKeys.app("newUserError").set(keys);
    }

    angular
        .module("new-user")
        .config(["appLangKeysProvider", config]);
})();
