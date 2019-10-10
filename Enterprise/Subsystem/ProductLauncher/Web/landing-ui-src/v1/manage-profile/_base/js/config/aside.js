//  Bind Manage Profile Aside

(function (angular) {
    "use strict";

    function config(pubsub, mpAside) {
        pubsub.subscribe("manageProfile.show", function () {
            mpAside.show();
        });
    }

    angular
        .module("settings")
        .run(["pubsub", "mpAside", config]);
})(angular);
