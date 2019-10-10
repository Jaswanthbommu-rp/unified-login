//  Security Question Options Service

(function (angular, undefined) {
    "use strict";

    function SecurityQuestionOptions($filter, ENV, $resource, eventStream, user) {
        var svc = this,
            url = ENV.landingAPI + "api/credential/userallsecurityquestions";

        svc.isBusy = false;
        svc.hasData = false;
        svc.ready = eventStream();

        svc.options = [
            {
                question: $filter("securityQuestionsText")("option.default"),
                questionId: ""
            }
        ];

        svc.get = function (callback) {
            if (svc.hasData) {
                callback(svc.options);
            }
            else {
                if (!svc.isBusy) {
                    svc.getData();
                    svc.isBusy = true;
                }
                svc.ready.subscribe(callback);
            }
        };

        svc.getData = function () {
            var data = {
                enterpriseUserName: user.getEnterpriseUserName()
            };

            $resource(url).get(data, svc.setData);
        };

        svc.setData = function (resp) {
            svc.isBusy = false;
            svc.hasData = true;
            svc.options = svc.options.concat(resp.securityQuestions);

            svc.ready.publish(svc.options);
            svc.ready.destroy();
            svc.ready = undefined;
        };
    }

    angular
        .module("settings")
        .service("securityQuestionOptions", [
            "$filter",
            "ENV",
            "$resource",
            "eventStream",
            "userModel",
            SecurityQuestionOptions
        ]);
})(angular);
