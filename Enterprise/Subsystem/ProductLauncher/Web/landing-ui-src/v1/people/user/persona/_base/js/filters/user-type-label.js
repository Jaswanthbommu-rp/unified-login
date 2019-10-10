(function (angular) {
    "use strict";

    function momentFilter() {
        return function (personaTypeValue) {

            //TODO persona types should be taken from back-end
            //TODO language bundles
            switch(personaTypeValue) {
                case 1: 
                    return "Production";
                case 2: 
                    return "UAT";
                default: 
                    return "--";
            }

        };
    }

    angular
        .module("settings")
        .filter("userTypeLabel", [
            momentFilter
        ]);
})(angular);