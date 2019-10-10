(function (angular, undefined) {
    "use strict";

    function rmpropertyActions(assignRolesAside, tabsContext, tabsData) {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function () {
                scope.rmpropertyActions = dir;
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.showEditPropertiesbyCompany = function (record) {
                tabsContext.set({
                    type: "edit",
                    data: record
                });
                assignRolesAside.show();
            };

            dir.destroy = function () {
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
        .directive("rmpropertyActions", [
            "rmAssignPropertyAside",
            "rmAssignPropertyContext",
             rmpropertyActions
        ]);
})(angular);
