//  Roles Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "isAssigned",
                type: "custom",
                idKey: "id",
                templateUrl: "user/assign-product-access/unified-login/templates/role-radio.html"
            }, {
                key: "name",
                type: "text",
            }, {
                key: "roletype"
            }, {
                key: "infoIcon",
                type: "custom",
                templateUrl: "user/assign-product-access/unified-login/templates/rights-info-icon.html"
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
                }, {
                    key: "infoIcon",
                    text: "",
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
                            value: "Custom",
                            name: "Custom"
                        },
                        {
                            value: "System",
                            name: "System"
                        }
                    ]
                },
                {
                    key: "infoIcon",
                    type: "",
                    placeholder: ""
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("userMgmtRolesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);