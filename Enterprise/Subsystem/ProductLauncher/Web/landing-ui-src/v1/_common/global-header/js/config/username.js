//  Init Username

(function (angular) {
    "use strict";

    function config(username) {
        username.bind();
    }

    angular
        .module("settings")
        .run(["globalHeaderUsername", config]);
})(angular);
