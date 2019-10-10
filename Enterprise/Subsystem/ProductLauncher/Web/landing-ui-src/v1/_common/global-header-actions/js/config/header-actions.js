// Bind Header Actions

(function (angular) {
    "use strict";

    function config(headerActions) {
        headerActions.bind();
    }

    angular
        .module("rpGlobalHeader")
        .run(["rpGlobalHeaderActions", config]);
})(angular);
