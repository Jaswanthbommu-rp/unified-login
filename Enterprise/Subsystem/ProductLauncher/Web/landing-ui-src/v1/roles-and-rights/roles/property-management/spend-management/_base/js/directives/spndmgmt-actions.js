// 

(function(angular, undefined) {
    "use strict";

    function spndmgmtActions(assignRolesAside, tabsContext, tabsData) {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function() {
                scope.spndmgmtActions = dir;
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.showEditRolesbyRights = function(record) {
                dir.setActiveTab("01");
                tabsContext.set({ type: "edit", data: record });
                assignRolesAside.show();
            };

            dir.setActiveTab = function(id) {
                
                angular.forEach(tabsData.data, function(tab) {                    
                    tab.isActive = false;
                    if (tab.id === id) {                        
                        tab.isActive = true;
                    }
                });
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
        .directive("spndmgmtActions", [
            "spndmgmtAssignRoleAside",
            "spndmgmtAssignTabsContext",
            "spndMgmtAssignTabsData",
            spndmgmtActions
        ]);
})(angular);