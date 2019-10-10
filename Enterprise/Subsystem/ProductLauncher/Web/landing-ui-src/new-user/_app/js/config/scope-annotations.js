//  Scope Annotation Config

(function (angular) {
    "use strict";

    function config($compileProvider) {
        $compileProvider.debugInfoEnabled(false);
    }

    angular
        .module("new-user")
        .config(["$compileProvider", config]);
})(angular);
