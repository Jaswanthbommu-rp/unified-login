    //  Roles Actions Config

    (function(angular) {
        "use strict";

        function factory(gridActions, actionsMenu) {
            var model = gridActions();

            model.get = function(record) {
                if (record.roletype.toLowerCase() === "custom") {
                    return actionsMenu({
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
                            {
                                text: "Delete",
                                method: model.getMethod("deleteRole"),
                                data: record
                            }
                        ],
                        menuOffsetLeft: -105,
                        menuClassNames: "onesite-roles-action-menu"
                    });
                } else {

                    return actionsMenu({
                        actions: [{
                            text: "Clone",
                            method: model.getMethod("cloneRole"),
                            data: record
                        }],
                        menuOffsetLeft: -105,
                        menuClassNames: "onesite-roles-action-menu"
                    });
                }

            };

            return model;
        }

        angular
            .module("settings")
            .factory("onesiteRolesGridActions", [
                "rpGridActions",
                "rpActionsMenuModel",
                factory
            ]);
    })(angular);
