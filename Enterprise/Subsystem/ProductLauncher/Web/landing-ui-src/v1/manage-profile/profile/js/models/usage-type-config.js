//  Phone Label Config

(function (angular, undefined) {
    "use strict";

    function UsageTypeConfig(menuConfig, optionsSvc) {
        var index = 0,
            svc = this;

        svc.get = function (data) {
            index++;

            data = angular.extend({
                nameKey: "name",
                id: "usage-type-" + index,
                fieldName: "usage-type-" + index,
                valueKey: "contactMechanismUsageTypeId"
            }, data || {});

            var config = menuConfig(data);
            optionsSvc.get(config.setOptions.bind(config));
            return config;
        };
    }

    angular
        .module("settings")
        .service("usageTypeConfig", [
            "rpFormSelectMenuConfig",
            "usageTypeOptions",
            UsageTypeConfig
        ]);
})(angular);
