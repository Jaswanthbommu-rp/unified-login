//  Security Question Options Service

(function (angular) {
    "use strict";

    function SecurityQuestionOptionsSvc($resource, userModel, ENV) {
        var svc = this,
            resource = $resource(ENV.landingAPI + "api/credential/userallsecurityquestions");

        svc.getList = function () {
            var params = {
                enterpriseUserName: userModel.getEnterpriseUserName()
            };

            return resource.get(params).$promise;
        };

    }

    angular
        .module("new-user")
        .service("securityQuestionOptionsSvc", [
            "$resource",
            "userModel",
            "ENV",
            SecurityQuestionOptionsSvc
        ]);
})(angular);
