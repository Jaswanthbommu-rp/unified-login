//  Initialize Angular App Modules

(function () {
    "use strict";

    angular.module("gbShared", []);

    angular
        .module("new-user", ["rpApp", "gbShared"]);
})();
