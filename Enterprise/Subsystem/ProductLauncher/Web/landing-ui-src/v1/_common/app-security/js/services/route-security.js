//  Route Security Service

(function (angular, undefined) {
    "use strict";

    function RouteSecurity(service, eventStream) {
        var svc = this;

        svc.ready = false;
        svc.readyEvent = eventStream();

        svc.data = {
            rights: [],
            routeData: {}
        };

        // Getters

        svc.getData = function () {
            return svc.data;
        };

        // Actions

        svc.load = function () {
            svc.ready = false;
            return service.getRights(svc.onSuccess, svc.onError);
        };

        svc.onError = function () {
            svc.ready = false;
            logw("RouteSecurity: Data call failed!");
            return svc;
        };

        svc.onSuccess = function (data) {
            svc.ready = true;
            svc.data = data || {};
            svc.readyEvent.publish(svc.data);
            return svc;
        };

        svc.subscribe = function (callback) {
            return svc.readyEvent.subscribe(callback);
        };

        // Assertions

        svc.isAllowed = function (activityName) {
            var allowed = true,
                rights = svc.data.routeData[activityName];

            if (rights) {
                rights.forEach(function (right) {
                    allowed = allowed && svc.data.rights.contains(right);
                });
            }
            else {
                allowed = false;
            }

            return allowed;
        };

        svc.isReady = function () {
            return svc.ready;
        };
    }

    angular
        .module("settings")
        .service("routeSecurity", [
            "routeSecuritySvc",
            "eventStream",
            RouteSecurity
        ]);
})(angular);
