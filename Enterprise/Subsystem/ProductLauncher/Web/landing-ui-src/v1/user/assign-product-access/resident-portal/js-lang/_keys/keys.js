(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "notificationName.FrontDesk",
            "notificationName.Reservation",
            "notificationName.ServiceRequest"
        ];

        appLangKeys.app("residentPortals").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
