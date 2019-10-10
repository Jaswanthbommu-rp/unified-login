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
                    key: "propertyType",
                    type: "text"
            },
                {
                    key: "role",
                    type: "custom",
                    idKey: "id",
                    templateUrl: "user/assign-product-access/portfolio-management/templates/entity-roles.html"
            }];
        };

        model.getHeaders = function () {
            return [
                [{
                    key: "name",
                    text: "Entity",
                }, {
                    key: "propertyType",
                    text: "Type",
                },{
                    key: "role",
                    text: "Role"
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "name",
                    type: "text",
                    placeholder: "Filter by Entity Name"
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
