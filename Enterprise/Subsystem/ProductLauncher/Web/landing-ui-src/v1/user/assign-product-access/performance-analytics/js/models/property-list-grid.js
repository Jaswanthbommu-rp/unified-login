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
                },
                {
                    key: "state",
                    type: "text"
                }
                ];
        };

        model.getHeaders = function () {
            return [
                [{
                        key: "propertyName",
                        text: "Property",
                },
                    {
                        key: "state",
                        text: "State",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "propertyName",
                    type: "text",
                    placeholder: "Find by Property"
                },
                {
                    key: "state",
                    type: "text",
                    placeholder: "Find by State"
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
