//  Clone User Resource Model

(function (angular, undefined) {
    "use strict";

    function factory($resource, ENV) {
        var url, defaults, actions;

        defaults = {};

        url = ENV.landingAPI + "api/userclone/:realPageId";

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
        .factory("cloneUserSvc", ["$resource", "ENV", factory]);
})(angular);
