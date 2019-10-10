//  Init Global Nav

(function (angular) {
    "use strict";

    function config(svc) {
        svc.init();
    }

    angular
        .module("settings")
        .run(["globalNavSvc", config]);
})(angular);
