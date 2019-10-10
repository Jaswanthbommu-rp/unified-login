// 

(function(angular, undefined) {
    "use strict";

    function spndmgmtRolesToRightsActions(assignRolesAside, tabsContext) {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function() {
                scope.spndmgmtRolesToRightsActions = dir;
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.showEditRolesbyRights = function(record) {
                tabsContext.set({ type: "readOnly", data: record });
                assignRolesAside.show();
            };

            dir.destroy = function() {
                dir.destWatch();
                dir = undefined;
                scope = undefined;
            };

            dir.init();
        }

        return {
            link: link,
            restrict: "A"
        };
    }

    angular
        .module("settings")
        .directive("spndmgmtRolesToRightsActions", [
            "assignRolesToRightsAside",
            "assignRolesToRightsContext",
            spndmgmtRolesToRightsActions
        ]);
})(angular);