// Date Picker Config

(function (angular, undefined) {
    "use strict";

    function DatePickerConfig($filter, dateTimePickerConfig) {
        var index = 0,
            svc = this;

        svc.get = function (type, data) {
            index++;

            data = angular.extend({
                id: type + "-date-" + index,
                fieldName: type + "-date-" + index,
                iconClass: "rp-icon-calendar",
                format: "MM/DD/YYYY"                
            }, data || {});

            return dateTimePickerConfig(data);
        };
    }

    angular
        .module("settings")
        .service("userDatePickerConfig", [
            "$filter",
            "rpDatetimepickerConfig",
            DatePickerConfig
        ]);
})(angular);
