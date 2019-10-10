// Users List Grid Actions

(function (angular) {
    "use strict";

    function actionsGridFactory($filter, rpGridActions, rpActionsModel, userModel, security, persona) {
        var gridActions = rpGridActions(),
            filter = $filter("userListText");

        gridActions.get = function (record) {
            var actionsModel = {
                actions: [],
                menuClassNames: "users-list-action-menu",
                menuOffsetLeft: -25
            };

            if (persona.hasViewOnlySupportToolAccess()) {
                if (security.isAllowed("viewUser")) {
                    actionsModel.actions.push({
                        text: filter("users_view"),
                        href: "#/user/" + record.realPageId + "/UserList" + "/edit"
                    });
                }
            }
            else {
                if (security.isAllowed("editUser")) {
                    actionsModel.actions.push({
                        text: filter("users_edit"),
                        href: "#/user/" + record.realPageId + "/UserList" + "/edit"
                    });
                }

                if (security.isAllowed("viewUser")) {
                    actionsModel.actions.push({
                        text: filter("users_view"),
                        href: "#/user/" + record.realPageId + "/UserList" + "/edit"
                    });
                }

                if (security.isAllowed("editUser") && security.isAllowed("cloneUser") && record.userType === "RegularUser") {
                    actionsModel.actions.push({
                        text: filter("users_clone"),
                        href: "#/user/" + record.realPageId + "/clone"
                    });
                }

                if (record.realPageId !== userModel.getRealPageId()) {
                    if ((record.accountStatus === "Expired" ||
                            record.accountStatus === "Pending" ||
                            record.accountStatus === "Active") &&
                        security.isAllowed("activatedeactivateUser")) {
                        actionsModel.actions.push({
                            text: filter("users_deactivate"),
                            data: record,
                            method: gridActions.getMethod("deactivateUser")
                        });
                    }

                    if (record.accountStatus === "Deactivated" && security.isAllowed("activatedeactivateUser")) {
                        actionsModel.actions.push({
                            text: filter("users_activate"),
                            data: record,
                            method: gridActions.getMethod("activateUser")
                        });
                    }

                    if (record.accountStatus === "Locked" && security.isAllowed("lockUnlockUser")) {
                        actionsModel.actions.push({
                            text: filter("users_unlock"),
                            data: record,
                            method: gridActions.getMethod("unlockUser")
                        });
                    }

                    if (record.accountStatus === "Active" && security.isAllowed("lockUnlockUser")) {
                        actionsModel.actions.push({
                            text: filter("users_lock"),
                            data: record,
                            method: gridActions.getMethod("lockUser")
                        });
                    }

                    /*if (((record.activeStatus || record.lockStatus) && !record.pendingStatus) && security.isAllowed("lockUnlockUser")) {
                        actionsModel.actions.push({
                            text: record.lockStatus ? filter("users_unlock") : filter("users_lock"),
                            data: record,
                            method: record.lockStatus ? gridActions.getMethod("unlockUser") : gridActions.getMethod("lockUser"),
                        });
                    }

                    if ((!record.lockStatus || !record.pendingStatus) && security.isAllowed("activatedeactivateUser")) {
                        actionsModel.actions.push({
                            text: record.activeStatus ? filter("users_deactivate") : filter("users_activate"),
                            data: record,
                            method: record.activeStatus ? gridActions.getMethod("deactivateUser") : gridActions.getMethod("activateUser")
                        });
                    }*/
                }
            }

            return rpActionsModel(actionsModel);
        };

        return gridActions;
    }

    angular
        .module("settings")
        .factory("userListGridActions", [
            "$filter",
            "rpGridActions",
            "rpActionsMenuModel",
            "userSessionModel",
            "routeSecurity",
            "personaDetails",
            actionsGridFactory
        ]);
})(angular);
