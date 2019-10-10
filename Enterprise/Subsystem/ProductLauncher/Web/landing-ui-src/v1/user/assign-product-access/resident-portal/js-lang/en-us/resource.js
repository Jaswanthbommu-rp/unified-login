(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("residentPortals");

        bundle.set({
            "notificationName.FrontDesk": "Front desk instructions",
            "notificationName.Reservation": "New amenity reservation",
            "notificationName.ServiceRequest": "Service request submission & updates"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
