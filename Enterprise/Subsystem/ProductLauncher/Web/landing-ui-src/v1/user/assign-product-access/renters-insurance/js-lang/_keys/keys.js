(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "roleName.CorporateUser",
            "roleName.PropertyManager",
            "roleName.LeasingAgent"
        ];

        appLangKeys.app("rentersInsurance").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
