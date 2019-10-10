//  Manage User Service

(function (angular) {
    "use strict";

    function activityLogSvc($resource, ENV) {
        var svc = {};

        svc.getActivitiesForUser = function (payload) {
            // var url = "https://myactivitydev.corp.realpage.com/api/listactivitylog",
            var url = ENV.loggingAPI + "api/listactivitylog",

                actions = {
                    getActivities: {
                        method: "POST"
                    }
                },

                params = {
                };

            return $resource(url, params, actions).getActivities(payload).$promise;
        };

        return svc;
    }

    angular
        .module("settings")
        .factory("activityLogSvc", [
            "$resource",
            "ENV",
            activityLogSvc
        ]);
})(angular);
