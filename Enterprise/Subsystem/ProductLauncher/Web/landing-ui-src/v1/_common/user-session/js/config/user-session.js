//  Init User Session

(function (angular) {
    "use strict";

    function config(userSession) {
        userSession.load();
    }

    angular
        .module("settings")
        .run(["userSessionModel", config]);
})(angular);
