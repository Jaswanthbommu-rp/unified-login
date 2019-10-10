//  Initialize Angular App Modules

(function(angular) {
    "use strict";

    angular.module("gbShared", []);

    angular
        .module("settings", ["rpApp", "gbShared"]);
})(angular);
