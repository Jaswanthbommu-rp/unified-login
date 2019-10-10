//  Raul User Menu Directive

(function (angular, undefined) {
    "use strict";

    function raulUserMenu($cache, $compile, userLinks, pubsub) {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function () {
                dir.userLinksWatch = pubsub.subscribe("raul.userlinks", dir.setUserLinks);
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.assembleContent = function () {
                var html = $cache.get("common/raul-user-menu/templates/raul-user-menu.html");

                dir.contentScope = scope.$new();
                dir.content = angular.element(html);
                dir.contentScope.menuItems = dir.userLinks;

                $compile(dir.content)(dir.contentScope);

                // elem.html(dir.content);
            };

            dir.setUserLinks = function (links) {
                dir.userLinks = links;
                dir.assembleContent();
            };

            dir.destroy = function () {
                dir.contentScope.$destroy();
                dir.content.remove();

                dir.destWatch();
                dir.content = undefined;
                dir.contentScope = undefined;
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
        .directive("raulUserMenu", [
            "$templateCache",
            "$compile",
            "globalHeaderUserLinks",
            "pubsub",
            raulUserMenu
        ]);
})(angular);
