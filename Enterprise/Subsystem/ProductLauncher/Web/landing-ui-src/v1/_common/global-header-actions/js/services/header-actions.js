//  Header Actions Service

(function (angular) {
    "use strict";

    function GlobalHeaderActions($location, $window, $timeout, $rootScope, pubsub, userModel) {
        var svc = this;

        svc.bound = false;

        svc.bind = function () {
            if (!svc.bound) {
                svc.bound = true;
                $timeout(svc.bindEvents, 100);
                pubsub.subscribe("signout.rpGlobalHeader", svc.signoutHandler);
            }
        };

        svc.bindEvents = function () {
            $rootScope.$on("rpAppStateChange", svc.onStateChange);
        };

        svc.onStateChange = function (event, eventData) {
            if (!event.defaultPrevented && eventData.triggerID.match("GlobalHeaderActions")) {
                eventData.onContinue();
            }
        };

        svc.signoutHandler = function () {
            $rootScope.$emit("rpAppStateChange", {
                onContinue: svc.signout,
                triggerID: "GlobalHeaderActions.signout"
            });
        };

        svc.signout = function (params) {
            var queryStr = "";

            if (params) {
                angular.forEach(params, function (value, key) {
                    if (queryStr.length > 0) {
                        queryStr += "&";
                    }
                    else {
                        queryStr = "?";
                    }
                    queryStr += key + "=" + value;
                });
            }

            $window.location.href = "/home/signout" + queryStr;
        };
    }

    angular
        .module("rpGlobalHeader")
        .service("rpGlobalHeaderActions", [
            "$location",
            "$window",
            "$timeout",
            "$rootScope",
            "pubsub",
            "userSessionModel",
            GlobalHeaderActions
        ]);
})(angular);
