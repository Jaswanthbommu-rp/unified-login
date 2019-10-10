//  User Types Service

(function(angular) {
    "use strict";

    function factory($resource, $window, ENV) {

        var svc = {};

        svc.url = {
            urlParm: ENV.landingAPI + "api/userlogins/loginnameexists"
        };

        svc.requests = {
            getReq: null
        };

        svc.getDataReq = function(paramData) {

            var url = svc.url.urlParm,
                actions = {
                    get: {
                        method: "GET",
                        cancellable: true,
                        // isArray: true
                    }
                };

            return $resource(url, paramData, actions).get();
        };

        svc.getData = function(paramData) {
            svc.requests.getReq = svc.getDataReq(paramData);
            return svc.requests.getReq.$promise;
        };

        return svc;
    }

    angular
        .module("settings")
        .factory("externalUserSvc", ["$resource", "$window", "ENV", factory]);
})(angular);
