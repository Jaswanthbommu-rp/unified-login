//  User Resource Model

(function (angular, undefined) {
    "use strict";

    function factory($resource, ENV) {
        var url, defaults, actions;

        defaults = {};

        url = ENV.landingAPI + "api/user/:realPageId";

        actions = {
            get: {
                method: "GET",
                cancellable: true
            },

            save: {
                method: "POST",
                cancellable: true
            },

            update: {
                method: "PUT",
                cancellable: true
            }
        };

        return $resource(url, defaults, actions);
    }

    angular
        .module("settings")
        .factory("userSvc", ["$resource", "ENV", factory]);
})(angular);
