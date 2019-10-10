//  Persona Type Options

(function (angular) {
    "use strict";

    function personaTypeOptions(ENV, optionsCache) {
        var defaultOptions,
            model = optionsCache(ENV.landingAPI + "api/persona/environment");

        return model;
    }

    angular
        .module("settings")
        .factory("personaTypeOptions", [
            "ENV",
            "optionsCache",
            personaTypeOptions
        ]);
})(angular);
