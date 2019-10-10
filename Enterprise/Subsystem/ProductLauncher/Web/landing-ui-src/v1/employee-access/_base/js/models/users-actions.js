    //  Employee access Users Actions Config


    (function(angular) {
        "use strict";

        function factory(gridActions, actionsMenu, security) {
            var model = gridActions();

            model.get = function(record) {

                return actionsMenu({
                    actions: model.formatData(record),
                    menuOffsetLeft: -230
                });
            };

            model.formatData = function(record) {
                var actions = [];

                if (security.isAllowed("viewUnifiedPlatform") ||
                    security.isAllowed("viewonlysupporttoolaccess")) {
                    actions.push({
                        text: "Log in to the Unified Platform",

                        href: "../employee-access/" + record.userRealPageId
                    });
                }
               
                return actions;
            };

            return model;
        }

        angular
            .module("settings")
            .factory("empAccessUsersGridActions", [
                "rpGridActions",
                "rpActionsMenuModel",
                "routeSecurity",
                factory
            ]);
    })(angular);
