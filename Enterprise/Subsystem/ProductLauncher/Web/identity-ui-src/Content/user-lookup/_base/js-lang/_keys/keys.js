(function() {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            "lookup_username_info",
            "lookup_enter_username",

            "lookup_required",
            "lookup_continue",
            "lookup_back_to_login",
            "lookup_system_err_contact_admin"

        ];

        appLangKeys.app("userLookup").set(keys);
    }

    angular
        .module("identity")
        .config(["appLangKeysProvider", config]);
})();
