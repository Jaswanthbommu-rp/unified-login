//  Assign Product Family Directive

(function (angular, undefined) {
    "use strict";

    function assignProductFamily(panels) {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function () {
                dir.registerPanel();
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.getPanelData = function () {
                return scope.$eval(attr.rpAccordionPanel);
            };

            dir.registerPanel = function () {
                var data = dir.getPanelData();
                panels.register({
                    name: data.instName,
                    panel: scope[data.instName]
                });
            };

            dir.removePanel = function () {
                var data = dir.getPanelData();
                panels.remove({
                    name: data.instName
                });
            };

            dir.destroy = function () {
                dir.destWatch();
                dir.removePanel();
                dir = undefined;
                attr = undefined;
                elem = undefined;
                scope = undefined;
            };

            dir.init();
        }

        return {
            link: link,
            restrict: "C"
        };
    }

    angular
        .module("settings")
        .directive("assignProductFamily", [
            "familyAccordionPanels",
            assignProductFamily
        ]);
})(angular);
