//  Launch Simon Video Directive

(function (angular, window, undefined) {
    "use strict";

    function launchSimonVideo($window) {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function () {
                dir.click = "click.launchSimonVideo";
                elem.on(dir.click, dir.showVideo);
                dir.destWatch = scope.$on("$destroy", dir.destroy);
            };

            dir.showVideo = function ($event) {
                $event.preventDefault();

                var player = new $window.StaticVideoPlayer({
                    title: 'RealPage Unified Platform',
                    description: '',
                    src: "https://learning.realpage.com/downloads/Simon/Simon_UnifiedLogin.mp4"
                });

                player.show();
            };

            dir.destroy = function () {
                elem.off(dir.click);
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
        .module("identity")
        .directive("launchSimonVideo", ["$window", launchSimonVideo]);
})(angular, window);
