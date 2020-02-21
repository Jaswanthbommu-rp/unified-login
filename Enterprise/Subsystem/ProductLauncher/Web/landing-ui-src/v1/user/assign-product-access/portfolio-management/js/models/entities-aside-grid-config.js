//  portfolio-management Entity roles Grid Config Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function () {
            return [{
                        key: "isAssigned",
                        type: "select",
                        idKey: "id"
                    },
                    {
                        key: "name",
                        type: "text"
                    },
                    {
                        key: "propertyType",
                        type: "text"
                    }];
        };

        model.getHeaders = function () {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: true
                },
                {
                    key: "name",
                    text: "Entity",
                },
                {
                    key: "propertyType",
                    text: "Type"
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "isAssigned",
                    value: "",
                    type: "menu",
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
                    placeholder: "Filter by Entity"
                },
                {
                    key: "propertyType",
                    type: "text",
                    placeholder: "Filter by Type"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("portfolioManagementEntitiesAsideGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
