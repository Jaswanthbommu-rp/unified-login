(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            "initial_persona_name",
            "default_persona_name"
        ];

        appLangKeys.app("people.manageUser.personaNavigation").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
