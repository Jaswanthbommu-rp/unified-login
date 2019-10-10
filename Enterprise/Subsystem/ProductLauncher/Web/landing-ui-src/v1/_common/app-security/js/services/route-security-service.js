//  Route Security Service

(function (angular, undefined) {
    "use strict";

    function RouteSecuritySvc($q, $location, registry, resource) {
        var svc = this;

        svc.getRouteParams = function () {
            var data,
                url = $location.url();

            angular.forEach(registry, function (val) {
                var exp = new RegExp(val.exp, "i");

                if (!data && url.match(exp)) {
                    data = val;
                }
            });

            if (!data) {
                data = {};
                logw("RouteSecurity: No match found for route " + url);
            }

            return data;
        };

        svc.getRights = function (onSuccess, onError) {
            var reqData,
                params = svc.getRouteParams();

            svc.dataReq = $q.defer();

            if (svc.dataReqObj) {
                svc.dataReqObj.$cancelRequest();
            }

            if (params) {
                svc.reqParams = params;

                reqData = {
                    routeId: params.routeId
                };

                svc.callbacks = {
                    onError: onError,
                    onSuccess: onSuccess
                };

                svc.dataReqObj = resource.get(reqData, svc.onSuccess, svc.onError);
            }

            return svc.dataReq.promise;
        };

        svc.onSuccess = function (resp) {
            var data = {
                rights: resp.data.rights || [],
                routeData: svc.reqParams.routeData
            };

            if (svc.hasAccess(data)) {
                svc.callbacks.onSuccess(data);
                svc.dataReq.resolve();
            }
            else {
                logw("Access Denied!", data);
                $location.path("/error/access-denied");
            }

            svc.dataReq = undefined;
        };

        svc.onError = function () {
            svc.dataReq.reject();
            svc.callbacks.onError();
            svc.dataReq = undefined;
        };

        svc.hasAccess = function (data) {
            var hasAccess = false;

            if (data.rights.empty()) {
                return hasAccess;
            }

            angular.forEach(data.routeData, function (rights) {
                var allowed = true;

                rights.forEach(function (right) {
                    allowed = allowed && data.rights.contains(right);
                });

                hasAccess = hasAccess || allowed;
            });

            return hasAccess;
        };
    }

    angular
        .module("settings")
        .service("routeSecuritySvc", [
            "$q",
            "$location",
            "routeSecurityRegistry",
            "routeSecurityResource",
            RouteSecuritySvc
        ]);
})(angular);
