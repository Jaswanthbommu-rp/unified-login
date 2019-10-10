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
                templateUrl: "people/user/persona/add-products/spend-management/templates/property-radio.html"
            }, {
                key: "name",
                type: "text",
            }];
        };

        model.getHeaders = function() {
            return [
                [{ }, {
                    key: "name",
                    text: "Property Group",
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
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("SMPropertyGroupGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
