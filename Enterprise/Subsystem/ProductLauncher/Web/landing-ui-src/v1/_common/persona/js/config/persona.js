// Google Analytics

(function (angular) {
    "use strict";

    var initComplete = false;

    function setConfig(persona) {
        if (!initComplete) {
            initComplete = true;
            persona.init();
        }
    }

    angular
        .module("settings")
        .run(["personaDetails", setConfig]);
})(angular);
