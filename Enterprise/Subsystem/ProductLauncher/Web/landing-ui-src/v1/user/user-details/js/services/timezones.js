//  User Time Zone Service

(function (angular) {
    "use strict";

    function userTimeZones($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "api/usertimezones";

        params = {};

        actions = {
            get: {
                method: "GET",
                cancellable: true
            }
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("userTimeZoneSvc", ["$resource", "ENV", userTimeZones]);
})(angular);
