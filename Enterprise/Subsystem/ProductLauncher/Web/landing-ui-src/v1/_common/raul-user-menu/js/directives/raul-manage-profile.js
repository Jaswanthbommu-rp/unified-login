//  Raul Manage Profile Directive

(function (angular, undefined) {
    "use strict";

    function raulManageProfile(svc) {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function () {
                svc.setElemApi(dir);
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.show = function () {
                elem.parent().parent().removeClass("ng-hide");
            };

            dir.setLink = function (url) {
                elem.attr("href", url);
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
            restrict: "C"
        };
    }

    angular
        .module("settings")
        .directive("raulManageProfile", [
            "raulManageProfileSvc",
            raulManageProfile
        ]);
})(angular);
