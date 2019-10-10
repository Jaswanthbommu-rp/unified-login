//  Roles Config

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, actions, security, persona) {
        var model = gridConfig();

        model.get = function () {
            var data = [

                {
                    key: "centerName"
                },
                {
                    key: "right"
                },
                {
                    key: "actionLabel"
                },
                {
                    key: "rolesAssigned",
                    type: "custom",
                    templateUrl: "roles-and-rights/rights/property-management/realpage-accounting/base/templates/acct-rights.html"
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
                        key: "centerName",
                        text: "Area",
                    },
                    {
                        key: "right",
                        text: "Right",
                    },
                    {
                        key: "actionLabel",
                        text: "Action"
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
            var data = [{
                    key: "centerName"
                },
                {
                    key: "right",
                    type: "text",
                    placeholder: "Filter by right"
                },
                {
                    key: "actionLabel",
                    type: "text",
                    placeholder: "Filter by action"
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
        .factory("acctRightsGridConfig", [
            "rpGridConfig",
            "acctRightsGridActions",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
