// Enabled Config

(function (angular, undefined) {
    "use strict";

    function EnabledConfig($filter, inputTextConfig) {
        var index = 0,
            svc = this;

        svc.get = function (data) {
            index++;

            data = angular.extend({
                isVisible: true,
                label: $filter("userDetailsText")("add_persona") 
            }, data || {});

            return data;
        };
    }

    angular
        .module("settings")
        .service("addPersonaBtnConfig", [
            "$filter",
            EnabledConfig
        ]);
})(angular);
