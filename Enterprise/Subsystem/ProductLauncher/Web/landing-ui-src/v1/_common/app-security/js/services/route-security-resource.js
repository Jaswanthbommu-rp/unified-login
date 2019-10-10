//  Route Security Resource

(function (angular) {
    "use strict";

    function routeSecurityResource($resource, ENV) {
        var url, defaults, actions;

        actions = {
            get: {
                method: "GET",
                cancellable: true
            }
        };

        defaults = {};

        url = ENV.landingAPI + "api/:routeId/rights";

        return $resource(url, defaults, actions);
    }

    angular
        .module("settings")
        .factory("routeSecurityResource", ["$resource", "ENV", routeSecurityResource]);
})(angular);
