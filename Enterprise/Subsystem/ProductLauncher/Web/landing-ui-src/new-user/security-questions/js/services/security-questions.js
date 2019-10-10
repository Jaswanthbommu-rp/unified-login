//  Security Questions Service

(function (angular) {
    "use strict";

    function SecurityQuestionsSvc($resource, userModel, ENV) {
        var svc = this,
            resource = $resource(ENV.landingAPI + "api/credential/setusersecurityquestions");

        svc.save = function (listItem) {
            var params = {
                enterpriseUserName: userModel.getEnterpriseUserName(),
                activityToken: userModel.getActivityToken(),
                securityQuestionAnswers: []
            };

            angular.forEach(listItem, function (val) {
                params.securityQuestionAnswers.push({
                    questionId: val.data.questionId,
                    answer: val.data.answer,
                });
            });

            return resource.save(params).$promise;
        };
    }

    angular
        .module("new-user")
        .service("securityQuestionsSvc", [
            "$resource",
            "userModel",
            "ENV",
            SecurityQuestionsSvc
        ]);
})(angular);