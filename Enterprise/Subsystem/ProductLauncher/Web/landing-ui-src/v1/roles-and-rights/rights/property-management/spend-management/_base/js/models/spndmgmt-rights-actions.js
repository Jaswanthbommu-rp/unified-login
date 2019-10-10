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
                    menuOffsetLeft: -100,
                    menuClassNames: "spnd-mgt-rights-action-menu"
                });

            };

            return model;
        }

        angular
            .module("settings")
            .factory("spndmgmtightsGridActions", [
                "rpGridActions",
                "rpActionsMenuModel",
                factory
            ]);
    })(angular);
