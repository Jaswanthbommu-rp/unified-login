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
                    templateUrl: "roles-and-rights/roles/property-management/onesite/base/templates/onesite-rights.html"
                },
                {
                    key: "roletype"
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
                    key: "rightsAssigned",
                    // type: "text",
                    // placeholder: "Filter by rights"
                },
                {
                    key: "roletype",
                    // type: "text",
                    // placeholder: "Filter by type",
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
                            value: "Default",
                            name: "Default"
                        }
                    ]
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
        .factory("onesiteRolesGridConfig", [
            "rpGridConfig",
            "onesiteRolesGridActions",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
