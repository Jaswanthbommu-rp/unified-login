//  Detect Product Change Directive

(function (angular, undefined) {
    "use strict";

    function detectProductChange() {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function () {
                dir.config = scope.$eval(attr.detectProductChange) || {};

                dir.bindEvents();
                dir.changed = false;
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.bindEvents = function () {
                elem.on("change", dir.onChange);
            };

            dir.onChange = function () {
                if (dir.config.onChange && !dir.changed) {
                    dir.changed = true;
                    dir.config.onChange();
                }
            };

            dir.hasChanged = function () {
                return dir.changed;
            };

            dir.destroy = function () {
                dir.destWatch();
                dir = undefined;
                attr = undefined;
                elem = undefined;
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
        .directive("detectProductChange", [detectProductChange]);
})(angular);
