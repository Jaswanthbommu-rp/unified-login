//  portfolio Management Roles Grid Config Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, security) {
        var model = gridConfig();

        model.get = function () {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "id"
            }, {
                key: "name",
                type: "text"
            },
            {
                    key: "roleType",
                    type: "text"
            }];
        };

        model.getHeaders = function () {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: !security.isAllowed("viewUser")
                }, {
                    key: "name",
                    text: "Role",
                }, {
                    key: "roleType",
                    text: "Role Type",
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
                    placeholder: "Filter by Role Name"
                },
                {
                    key: "roleType",
                    type: "text",
                    placeholder: "Filter by Role Type"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("portfolioManagementRolesGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            factory
        ]);
})(angular);
