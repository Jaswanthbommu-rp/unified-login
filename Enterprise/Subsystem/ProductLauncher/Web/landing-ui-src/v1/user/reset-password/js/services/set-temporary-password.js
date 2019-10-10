(function (angular, undefined) {
    "use strict";

    function factory($resource, ENV, user) {
        var url, defaults, actions;

        defaults = {};

        url = ENV.landingAPI + "api/credential/settemporarypassword";

        actions = {
            method: "POST",
            cancellable: true
        };

        return $resource(url, defaults, actions);
    }

    angular
        .module("settings")
        .factory("setTempPasswordSvc", ["$resource", "ENV", "userSessionModel", factory]);
})(angular);
