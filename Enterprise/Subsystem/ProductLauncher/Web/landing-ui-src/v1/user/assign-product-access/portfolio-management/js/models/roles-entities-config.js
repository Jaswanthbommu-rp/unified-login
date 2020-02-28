//  portfolio-management Entity roles Grid Config Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function () {
            return [
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
                [
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
                    key: "name",
                    type: "text",
                    placeholder: "Filter by Role Name"
                },
                {
                    key: "assignedProperties",
                    type: "",
                    placeholder: ""
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
