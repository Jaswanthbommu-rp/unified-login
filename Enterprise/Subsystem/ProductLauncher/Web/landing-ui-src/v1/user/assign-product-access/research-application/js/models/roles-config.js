//  Roles Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [
            {
                key: "isAssigned",
                type: "custom",
                idKey: "id",
                templateUrl: "user/assign-product-access/research-application/templates/role-radio.html"
            },
            {
                key: "name",
                type: "text",
            }, {
                key: "roletype"
            }];
        };

        model.getHeaders = function() {
            return [
                [{ key: "isAssigned"},
                {
                    key: "name",
                    text: "Role",
                }, {
                    key: "roletype",
                    text: "Role Type",
                }]
            ];
        };

        model.getFilters = function() {
            return [{
                    key: "isAssigned",
                    type: "menu",
                    value: "",
                    options: [{
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
                    key: "roletype",
                    value: "",
                    type: "menu",
                    options: [{
                            value: "",
                            name: "All"
                        },
                        {
                            value: "System",
                            name: "System"
                        },
                    ]
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("resAppRolesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
