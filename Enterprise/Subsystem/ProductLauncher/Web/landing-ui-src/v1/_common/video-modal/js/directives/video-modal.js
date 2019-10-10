//  Video Modal Directive

(function (angular, undefined) {
    "use strict";

    function videoModal() {
        function link(scope, elem, attr) {
            var dir = {};

            dir.init = function () {
                dir.destWatch = scope.$on("$destroy", dir.destroy);
                dir.idWatch = scope.$watch("realpageId", dir.installVideoModal);
            };

            dir.installVideoModal = function (id) {
                var html = "<script>";
                html += "var uuid = 'rpId';";
                html += "new VideoPlayer(uuid);";
                html += "</script>";

                window.SIMON_MOCK_DATA = {
                title: 'Meet Simon',
                description: 'Find out how Simon makes it easy to learn how to navigate, use, and stay up to date with the RealPage Unified Platform.',
                videoLength: 1487146670,
                videoUrl: 'https://learning.realpage.com/downloads/Simon/Simon_UnifiedLogin.mp4',
                };
                window.SIMON_MOCK_SETTINGS = {
                    settingId: 0,
                    showVideoForNewUser: true,
                    allowSkipVideos: true,
                    skipVideoAfterPercentage: 30,
                    markVideoCompleteAfterPercentage: 30,
                    companyId: '123456789',
                };

                if (id) {
                    var vElem = angular.element(html.replace(/rpId/, id));
                    dir.idWatch();
                    elem.append(vElem);
                }
            };

            dir.destroy = function () {
                elem.html("");
                dir.destWatch();
                dir = undefined;
                attr = undefined;
                elem = undefined;
                scope = undefined;
            };

            dir.init();
        }

        return {
            scope: {
                realpageId: "="
            },
            link: link,
            restrict: "C"
        };
    }

    angular
        .module("settings")
        .directive("videoModal", [videoModal]);
})(angular);
