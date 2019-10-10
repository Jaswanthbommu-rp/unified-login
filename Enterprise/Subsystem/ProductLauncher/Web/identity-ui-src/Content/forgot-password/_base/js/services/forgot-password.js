//  Forgot Password Service

(function (angular) {
    "use strict";

    function forgotPasswordSvc($resource, userModel, ENV) {
        var svc = {};        

        svc.submitAnswers = function (answers) {
            var request,
                actions,
                postData;

            request = {
                url: ENV.landingAPI + "api/credential/VerifySecurityAnswers"
            };

            actions = {
                submitAnswers: {
                    method: "POST"
                }
            };

            postData = {
                enterpriseUserName: userModel.getEnterpriseLoginName(),
                activityToken: userModel.getActivityToken(),
                securityQuestionAnswers: answers
            };

            return $resource(request.url, {}, actions).submitAnswers({}, postData).$promise;
        };

        return svc;
    }

    angular
        .module("identity")
        .factory("forgotPasswordSvc", [
            "$resource",
            "userModel",
            "ENV",
            forgotPasswordSvc
        ]);
})(angular);