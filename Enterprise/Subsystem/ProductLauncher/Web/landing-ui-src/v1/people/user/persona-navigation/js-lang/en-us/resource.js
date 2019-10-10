(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.manageUser.personaNavigation");

        bundle.set({

            initial_persona_name: "Primary",
            default_persona_name: "Persona"

        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();