//  Properties Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "isAssigned",
                type: "custom",
                idKey: "id",
                templateUrl: "user/assign-product-access/client-portal/templates/property-radio.html"
            }, {
                key: "name",
                type: "text",
            }, {
                key: "state",
                type: "text"
            }];
        };

        model.getHeaders = function() {
            return [
                [
                    { key: "isAssigned" },
                    {
                        key: "name",
                        text: "Property",
                    },
                    {
                        key: "state",
                        text: "State"
                    }
                ]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "isAssigned",
                    type: "menu",
                    value: "",
                    options: [
                        {
                            value: "",
                            name: "All"
                        },
                        {
                            value: true,
                            name: "Selected"
                        },
                        {
                            value: false,
                            name: "Not Selected"
                        }
                    ]
                },
                {
                    key: "name",
                    type: "text",
                    placeholder: "Filter by Property Name"
                },
                {
                    key: "state",
                    type: "text",
                    placeholder: "Filter by State"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("clientPortalPropertiesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
