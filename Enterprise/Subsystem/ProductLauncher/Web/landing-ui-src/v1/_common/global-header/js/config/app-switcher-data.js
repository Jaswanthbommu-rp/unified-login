//  Bind App Switcher

(function (angular) {
    "use strict";

    function config(appSwitcherData) {
        appSwitcherData.bind();
    }

    angular
        .module("settings")
        .run(["appSwitcherData", config]);
})(angular);
