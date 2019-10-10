//  Property Group Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "isAssigned",
                type: "custom",
                idKey: "id",
                templateUrl: "people/user/persona/add-products/vendor-compliance/templates/property-group-radio.html"
            }, {
                key: "name",
                type: "text",
            }, {
                key: "type",
                type: "text"
            }];
        };

        model.getHeaders = function() {
            return [
                [{}, {
                    key: "name",
                    text: "Property Group",
                }, {
                    key: "type",
                    text: "Group Type"
                }]
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
                    placeholder: "Filter by Property Group Name"
                },
                {
                    key: "type",
                    type: "text",
                    placeholder: "Filter by Group Type"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("VendCompPropertyGroupGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
