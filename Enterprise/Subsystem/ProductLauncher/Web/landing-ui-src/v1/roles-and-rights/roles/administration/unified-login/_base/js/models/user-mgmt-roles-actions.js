    //  Roles Actions Config

    (function(angular) {
        "use strict";

        function factory(gridActions, actionsMenu) {
            var model = gridActions();

            model.get = function(record) {                
                if (record.roletype.toLowerCase() === "custom") {
                    return actionsMenu(model.setCustomRole({
                        actions: [{
                                text: "Assign Rights",
                                method: model.getMethod("assignRights"),
                                data: record
                            },
                            {
                                text: "Edit",
                                method: model.getMethod("editRole"),
                                data: record
                            },
                            {
                                text: "Clone",
                                method: model.getMethod("cloneRole"),
                                data: record
                            },
                            // {
                            //     text: "Set as User Default",
                            //     method: model.getMethod("setUserDefault"),
                            //     data: record
                            // },
                            {
                                text: "Delete",
                                method: model.getMethod("deleteRole"),
                                data: record
                            }
                        ],
                        menuOffsetLeft: -105,
                        menuClassNames: "usr-mgmt-roles-action-menu"
                    }, record));
                } else {
                    return actionsMenu(model.setDefaultRole({
                        actions: [{
                            text: "Clone",
                            method: model.getMethod("cloneRole"),
                            data: record
                        }],
                        menuOffsetLeft: -105,
                        menuClassNames: "usr-mgmt-roles-action-menu"
                    }, record));
                }
            };

            model.setDefaultRole = function(acMenu, record) {                
                if (record.name.toLowerCase() !== "user administrator" &&  record.defaultRole.toLowerCase() !== "user default") {
                    acMenu.actions.push({
                        text: "Set as User Default",
                        method: model.getMethod("setUserDefault"),
                        data: record
                    });
                }
                return acMenu;
            };

            model.setCustomRole = function(acMenu, record) {                
                if (record.defaultRole.toLowerCase() !== "user default") {
                    acMenu.actions.push({
                        text: "Set as User Default",
                        method: model.getMethod("setUserDefault"),
                        data: record
                    });
                }
                return acMenu;
            };

            return model;
        }

        angular
            .module("settings")
            .factory("userMgmtRolesGridActions", [
                "rpGridActions",
                "rpActionsMenuModel",
                factory
            ]);
    })(angular);