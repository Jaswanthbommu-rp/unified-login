//  Product Learning Portal Access Service

(function (angular) {
    "use strict";

    function accessLearningPortalSvc($resource, ENV) {
        var params, svc = {};
        var url = ENV.landingAPI + "api/products/learningportal";

        svc.getPLPUrl = function () {
            params = {};
            return $resource(url).get().$promise
                .then(svc.formatResponse);
        };

        svc.createPLPLogin = function () {
            var params = {
                createUser: true
            };
            return $resource(url, params).get().$promise
                .then(svc.formatResponse);
        };

        svc.verifyPLPLogin = function (parameters) {
            params = parameters;
            return $resource(url, params).get().$promise
                .then(svc.formatResponse);
        };

        //adapter for API response
        svc.formatResponse = function (response) {
            var newResponse = {};

            if (response.status.success === false) {
                newResponse.isError = true;
                newResponse.errorReason = response.status.errorMsg;
                newResponse.errorCode = response.status.errorCode;
            }
            else {
                newResponse = response;
            }

            return newResponse;
        };

        return svc;
    }

    angular
        .module("settings")
        .factory("accessLearningPortalSvc", [
            "$resource",
            "ENV",
            accessLearningPortalSvc
        ]);
})(angular);