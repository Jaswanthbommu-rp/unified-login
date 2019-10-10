//  User Session Resource

(function (angular, undefined) {
    "use strict";

    function factory($resource, ENV) {
        return $resource(ENV.landingAPI + "api/profiles/details");
    }

    angular
        .module("settings")
        .factory("userSessionSvc", ["$resource", "ENV", factory]);
})(angular);
