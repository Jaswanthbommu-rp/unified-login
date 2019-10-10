//  Initialize Angular App Modules

(function(angular) {
    "use strict";

    angular.module("gbShared", []);

    angular
        .module("identity", ["rpApp", "gbShared"]);
})(angular);
