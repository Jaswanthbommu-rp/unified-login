//  Rights Grid Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "id"
            }, {
                key: "name"
            }, {
                key: "state"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: true
                }, {
                    key: "name",
                    text: "Property",
                }, {
                    key: "state",
                    text: "State",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "isAssigned",
                    value: "",
                    type: "menu",
                    size: "small",
                    options: [
                        {
                            value: "",
                            name: "All"
                        },
                        {
                            value: true,
                            name: "Assigned"
                        },
                        {
                            value: false,
                            name: "Unassigned"
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
        .factory("osPropertiesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
