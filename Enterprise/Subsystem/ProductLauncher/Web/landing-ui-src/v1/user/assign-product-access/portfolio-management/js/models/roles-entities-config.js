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
                    key: "assignedProperties",
                    type: "custom",
                    templateUrl: "user/assign-product-access/portfolio-management/templates/entities-assigned.html"
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
                    text: "Role",
                },
                {
                    key: "assignedProperties",
                    text: "Assigned Entities"
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
                            name: "Selected"
                        },
                        {
                            value: false,
                            name: "Not Selected"
                        }
                    ]
                },
                {
                    key: "role",
                    type: "text",
                    placeholder: "Filter by Role Name"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("portfolioManagementEntitiesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
