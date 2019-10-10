    //  Roles Actions Config

    (function(angular) {
        "use strict";

        function factory(gridActions, actionsMenu, security, persona) {
            var model = gridActions();

            model.get = function(record) {

                return actionsMenu(model.checkRight({}, record));
            };

            model.checkRight = function(acMenu, record) {
                if (security.isAllowed("viewRoleRight")) {

                    acMenu = {
                        actions: [{
                            text: "View",
                            method: model.getMethod("viewRights"),
                            data: record
                        }],
                        menuOffsetLeft: -105,
                        menuClassNames: "spnd-mgt-roles-action-menu"
                    };
                }
                
                if (security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess()) {
                    // is_marketplace_admin Marketplace Administrator Role is like System role, cannot be edited
                    if (record.is_marketplace_admin === "1") {
                        acMenu = {
                            actions: [{
                                text: "Clone",
                                method: model.getMethod("cloneRole"),
                                data: record
                            }],
                            menuOffsetLeft: -105,
                            menuClassNames: "spnd-mgt-roles-action-menu"
                        };

                    } else {


                        acMenu = {
                            actions: [

                                {
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
                                //     text: "Delete",
                                //     method: model.getMethod("deleteRole"),
                                //     data: record
                                // }

                            ],
                            menuOffsetLeft: -105,
                            menuClassNames: "spnd-mgt-roles-action-menu"
                        };
                    }
                }

                return acMenu;
            };

            return model;
        }

        angular
            .module("settings")
            .factory("spndmgmtRolesGridActions", [
                "rpGridActions",
                "rpActionsMenuModel",
                "routeSecurity",
                "personaDetails",
                factory
            ]);
    })(angular);