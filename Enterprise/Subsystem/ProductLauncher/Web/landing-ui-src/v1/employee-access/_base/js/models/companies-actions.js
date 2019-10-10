    //  Employee access Actions Config


    (function(angular) {
        "use strict";

        function factory(gridActions, actionsMenu, security, compActionModal) {
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

                if (security.isAllowed("viewUnifiedPlatform") &&
                    security.isAllowed("viewonlysupporttoolaccess")) {
                    actions.push({
                        text: "Update System Administrators",
                        data: record,
                        method: compActionModal.openModal
                    });
                } else if (security.isAllowed("viewUnifiedPlatform")) {
                    actions.push({
                        text: "Update System Administrators",
                        data: record,
                        method: compActionModal.openModal
                    });
                }


                return actions;
            };

            return model;
        }

        angular
            .module("settings")
            .factory("empAccessGridActions", [
                "rpGridActions",
                "rpActionsMenuModel",
                "routeSecurity",
                "empAccessCompActionModalModel",
                factory
            ]);
    })(angular);