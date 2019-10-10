(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            if(guid == "default_persona_name") {
                return "Persona";
            }
            return appLangTranslate("people.user.add").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("addUserText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 