//  Properties Aside Grid Config Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function () {
            return [
                {
                    key: "propertyName",
                    type: "text"
                }
                ];
        };

        model.getHeaders = function () {
            return [
                [{
                        key: "propertyName",
                        text: "Property",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "propertyName",
                    type: "text",
                    placeholder: "Find by Property"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("paPropertiesAsideGrigConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
