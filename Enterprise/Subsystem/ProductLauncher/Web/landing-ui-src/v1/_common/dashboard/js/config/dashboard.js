//  Init Dashboard Model

(function (angular) {
    "use strict";

    var bound = false;

    function config(dashboard, pubsub) {
        dashboard.load();

        if (!bound) {
            bound = true;
            pubsub.subscribe("dashboard.reload", dashboard.reload.bind(dashboard));
        }
    }

    angular
        .module("settings")
        .run(["dashboardModel", "pubsub", config]);
})(angular);
