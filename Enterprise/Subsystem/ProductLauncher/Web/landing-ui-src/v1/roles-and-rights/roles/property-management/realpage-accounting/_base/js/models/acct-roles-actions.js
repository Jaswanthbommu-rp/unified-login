    //  Roles Actions Config

    (function (angular) {
        "use strict";

        function factory(gridActions, actionsMenu) {
            var model = gridActions();

            model.get = function (record) {
                if (record.roletype.toLowerCase() === "custom") {
                    return actionsMenu({
                        actions: [{
                                text: "Assign Rights",
                                method: model.getMethod("assignRights"),
                                data: record
                            },
                            // {
                            //     text: "Edit",
                            //     method: model.getMethod("editRole"),
                            //     data: record
                            // },
                            {
                                text: "Clone",
                                method: model.getMethod("cloneRole"),
                                data: record
                            },
                            {
                                text: "Delete",
                                method: model.getMethod("deleteRole"),
                                data: record
                            }
                        ],
                        menuOffsetLeft: -100,
                        menuClassNames: "acct-roles-action-menu"
                    });
                }
                else {
                    return actionsMenu({
                        actions: [{
                            text: "Clone",
                            method: model.getMethod("cloneRole"),
                            data: record
                        }],
                        menuOffsetLeft: -100,
                        menuClassNames: "acct-roles-action-menu"
                    });
                }

            };

            return model;
        }

        angular
            .module("settings")
            .factory("acctRolesGridActions", [
                "rpGridActions",
                "rpActionsMenuModel",
                factory
            ]);
    })(angular);
