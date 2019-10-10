//  Roles Config

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, actions, security, persona) {
        var model = gridConfig();

        model.get = function () {
            var data = [{
                    key: "name"
                }
            ];

             if (security.isAllowed("manageRoleRight") || security.isAllowed("viewRoleRight")  && !persona.hasViewOnlySupportToolAccess()) {

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
        .factory("spndmgmtRolesGridConfig", [
            "rpGridConfig",
            "spndmgmtRolesGridActions",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
