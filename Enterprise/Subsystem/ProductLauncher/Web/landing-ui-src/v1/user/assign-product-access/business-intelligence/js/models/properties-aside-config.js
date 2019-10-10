//  BusinessIntelligence Properties Aside Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "propertyName",
                type: "text"
            }, {
                key: "statecode",
                type: "text"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "propertyName",
                    text: "Property",
                }, {
                    key: "statecode",
                    text: "State"
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "propertyName",
                    type: "text",
                    placeholder: "Filter by Property Name"
                },
                {
                    key: "statecode",
                    type: "text",
                    placeholder: "Filter by State"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("biPropertiesAsideGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
