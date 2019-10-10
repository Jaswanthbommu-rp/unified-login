(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "user_type_regular_user",
            "user_type_regular_no_email_user",
            "user_type_super_user",
            "user_type_sde",
            "user_type_external_user",
            "user_type_rp_employee",
        ];

        appLangKeys.app("people.manageUser").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
