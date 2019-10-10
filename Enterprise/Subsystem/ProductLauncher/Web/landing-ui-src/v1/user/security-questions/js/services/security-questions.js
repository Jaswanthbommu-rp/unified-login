//  Security Questions Resource Model
//  Security Questions List Service

(function (angular, undefined) {
    "use strict";

    function SecurityQuestionsList(ENV, $resource) {
        var svc = this,
            resource = $resource(ENV.landingAPI + "api/credential/userselectedsecurityquestions");

        svc.get = function (realpageId, callback) {
            if (realpageId !== null) {
                return $resource(ENV.landingAPI + "api/credential/userselectedsecurityquestions?realPageId=" + realpageId).get(callback);
            }
            else {
                return resource.get(callback);
            }
        };

        svc.save = function (realpageId, data, callback) {
            if (realpageId !== null) {
                return $resource(ENV.landingAPI + "api/credential/userselectedsecurityquestions?realPageId=" + realpageId).save.apply(resource, arguments);
            }
            else {
                return resource.save.apply(resource, arguments);
            }
        };
    }

    angular
        .module("settings")
        .service("securityQuestionsList", [
            "ENV",
            "$resource",
            SecurityQuestionsList
        ]);
})(angular);
