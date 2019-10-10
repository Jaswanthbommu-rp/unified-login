// Enabled Config

(function (angular, undefined) {
    "use strict";

    function EnabledConfig($filter, inputTextConfig) {
        var index = 0,
            svc = this;

        svc.get = function (data) {
            index++;

            data = angular.extend({
                id: "enableUser",
                label: $filter("userDetailsText")("user_detail_enable_user") 
            }, data || {});

            return data;
        };
    }

    angular
        .module("settings")
        .service("enableSwitchConfig", [
            "$filter",
            "rpFormInputTextConfig",
            EnabledConfig
        ]);
})(angular);
