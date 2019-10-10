//  Third Party IDP Setting Service

(function (angular) {
    "use strict";

    function thirdPartyIDPSvc($resource, ENV) {
        var url, defaults, actions;

        actions = {
            get: {
                method: "GET",
                cancellable: true
            }
        };

        defaults = {};

        url = ENV.landingAPI + "api/organization/providertype";

        return $resource(url, defaults, actions);
    }

    angular
        .module("settings")
        .factory("thirdPartyIDPSvc", ["$resource", "ENV", thirdPartyIDPSvc]);
})(angular);
