(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("rentersInsurance");

        bundle.set({
            "roleName.CorporateUser": "Front desk instructions",
            "roleName.PropertyManager": "New amenity reservation",
            "roleName.LeasingAgent": "Service request submission & updates"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
