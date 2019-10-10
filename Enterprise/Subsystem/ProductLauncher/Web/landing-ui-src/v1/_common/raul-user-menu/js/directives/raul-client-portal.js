//  Raul Client Portal Directive

(function (angular, undefined) {
    "use strict";

    function raulClientPortal(pubsub, svc) {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function () {
                svc.setElemApi(dir);
                elem.on("click", dir.onClick);
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.onClick = function () {
                pubsub.publish("raulClientPortal.show");
            };

            dir.show = function () {
                elem.parent().parent().removeClass("ng-hide");
            };

            dir.setLink = function (url) {
                elem.attr("href", url);
            };

            dir.destroy = function () {
                elem.off("click");
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
            restrict: "C"
        };
    }

    angular
        .module("settings")
        .directive("raulClientPortal", [
            "pubsub",
            "raulClientPortalSvc",
            raulClientPortal
        ]);
})(angular);
