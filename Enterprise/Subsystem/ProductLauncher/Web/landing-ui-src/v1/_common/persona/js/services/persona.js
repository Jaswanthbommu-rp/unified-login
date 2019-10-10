//  Persona Service

(function (angular) {
    "use strict";

    function personaSvc($resource, ENV) {
        return $resource(ENV.landingAPI + "api/persona");
    }

    angular
        .module("settings")
        .factory("personaSvc", ["$resource", "ENV", personaSvc]);
})(angular);
