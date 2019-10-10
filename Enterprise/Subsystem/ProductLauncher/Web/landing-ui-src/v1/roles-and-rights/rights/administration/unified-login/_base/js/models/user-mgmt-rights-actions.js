    //  Roles Actions Config

    (function(angular) {
        "use strict";

        function factory(gridActions, actionsMenu) {
            var model = gridActions();

            model.get = function(record) {

                return actionsMenu({
                    actions: [{
                        text: "Assign Roles",
                        method: model.getMethod("assignRoles"),
                        data: record
                    }],
                    menuOffsetLeft: -105,
                    menuClassNames: "usr-mgmt-rights-action-menu"
                });
            };

            return model;
        }

        angular
            .module("settings")
            .factory("userMgmtRightsGridActions", [
                "rpGridActions",
                "rpActionsMenuModel",
                factory
            ]);
    })(angular);
