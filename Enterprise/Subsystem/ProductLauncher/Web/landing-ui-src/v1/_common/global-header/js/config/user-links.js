//  Init Global Header User Links

(function (angular) {
    "use strict";

    function config(userLinks) {
        userLinks.init();
    }

    angular
        .module("settings")
        .run(["globalHeaderUserLinks", config]);
})(angular);
