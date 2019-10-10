//  Roles Config

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, actions, security, persona) {
        var model = gridConfig();

        model.get = function () {
            var data = [{
                    key: "name"
                },
                {
                    key: "rightsAssigned",
                    type: "custom",
                    templateUrl: "roles-and-rights/roles/administration/unified-login/base/templates/user-mgmt-rights.html"
                },
                {
                    key: "roletype"
                },
                {
                    key: "defaultRole",
                    type: "custom",
                    templateUrl: "roles-and-rights/roles/administration/unified-login/base/templates/user-mgmt-default-role.html"

                }
            ];

            if (security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess()) {
                data.push({
                    key: "more",
                    type: "actionsMenu",
                    getActions: actions.get
                });
            }

            return data;
        };

        model.getHeaders = function () {

            var headerData = [];
            var data = [{
                    key: "name",
                    text: "Role",
                            },
                {
                    key: "rightsAssigned",
                    text: "Rights",
                            },
                {
                    key: "roletype",
                    text: "Type",
                            },
                {
                    key: "defaultRole",
                    text: "",
                    }
                        ];

            if (security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess()) {
                data.push({
                    key: "more"
                });
            }

            headerData.push(data);
            return headerData;
        };

        model.getFilters = function () {
            var data = [{
                    key: "name",
                    type: "text",
                    placeholder: "Filter by name"
                },
                {
                    key: "rightsAssigned"
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
                    key: "defaultRole"
                }
            ];

            if (security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess()) {
                data.push({
                    key: "more"
                });
            }

            return data;
        };

        model.getTrackSelectionConfig = function () {
            var config = {},
                columns = model.get();

            columns.forEach(function (column) {
                if (column.type == "select") {
                    config.idKey = column.id;
                    config.selectKey = column.key;
                }
            });

            return config;
        };

        return model;
    }
    angular
        .module("settings")
        .factory("usMgmtRolesGridConfig", [
            "rpGridConfig",
            "userMgmtRolesGridActions",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
