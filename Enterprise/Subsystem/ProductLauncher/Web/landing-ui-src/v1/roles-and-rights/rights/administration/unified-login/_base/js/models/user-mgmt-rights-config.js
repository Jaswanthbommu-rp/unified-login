//  Roles Config

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, actions, security, persona) {
        var model = gridConfig();

        model.get = function () {
            var data = [
                {
                    key: "description"
                },
                {
                    key: "rolesAssigned",
                    type: "custom",
                    templateUrl: "roles-and-rights/rights/administration/unified-login/base/templates/user-mgmt-rights.html"
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
            var data = [
                    {
                        key: "description",
                        text: "Right",
                    },
                    {
                        key: "rolesAssigned",
                        text: "Roles",
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
            var data = [

                {
                    key: "description",
                    type: "text",
                    placeholder: "Filter by right"
                },
                {
                    key: "rolesAssigned"
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
        .factory("userMgmtRightsGridConfig", [
            "rpGridConfig",
            "userMgmtRightsGridActions",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
