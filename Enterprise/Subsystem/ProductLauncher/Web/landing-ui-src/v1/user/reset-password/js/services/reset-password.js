//  Reset Password Service
(function (angular, undefined) {
    "use strict";

    function factory($resource, ENV) {
        var url, defaults, actions;

        defaults = {};

        url = ENV.landingAPI + "api/credential/resetpassword";

        actions = {
            save: {
                method: "POST",
                cancellable: true
            }
        };

        return $resource(url, defaults, actions);
    }

    angular
        .module("settings")
        .factory("resetPasswordSvc", ["$resource", "ENV", factory]);
})(angular);
