(function (angular) {
    "use strict";

    var idpRedirectSvc = function ($resource) {
        var svc = {};

        svc.requestState = null;
        svc.url = "../Home/IdpRedirect";

        svc.checkIDProviderReq = function (username, signin) {
            var actions = {
                    post: {
                        method: "POST",
                        responseType: "json",
                        cancellable: true
                    }
                },
                paramData = {
                    username: username,
					signin : signin
                };

            return $resource(svc.url, {}, actions).post({}, paramData);
        };
        svc.checkIDProvider = function (username, signin) {
            svc.requestState = svc.checkIDProviderReq(username, signin);
            return svc.requestState.$promise;
        };

        svc.clearRequests = function () {
            svc.requestState = null;
        };

        svc.cancelRequests = function () {
            if (svc.requestState !== null) {
                svc.requestState.$cancelRequest();
                svc.requestState = null;
            }
        };

        return svc;
    };

    angular
        .module("identity")
        .factory("idpRedirectSvc", [
            "$resource",
            idpRedirectSvc
        ]);


})(angular);
