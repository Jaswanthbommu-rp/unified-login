//  Security Question Options Service

(function (angular, undefined) {
    "use strict";

    function SecurityQuestionOptions(ENV, $resource, eventStream, user) {
        var svc = this,
            url = ENV.landingAPI + "api/credential/userallsecurityquestions";

        svc.isBusy = false;
        svc.hasData = false;
        svc.ready = eventStream();

        svc.options = [
            {
                question: "Select Security Question",
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
                enterpriseUserName: user.getUsername()
            };

            $resource(url).get(data, svc.setData);
        };

        svc.setData = function (resp) {
            svc.isBusy = false;
            svc.hasData = true;
            svc.options = svc.options.concat(resp.securityQuestions);

            // svc.options.push({
            //     questionId: -1,
            //     question: "Create Your Own Question"
            // });

            svc.ready.publish(svc.options);
            svc.ready.destroy();
            svc.ready = undefined;
        };
    }

    angular
        .module("settings")
        .service("securityQuestionOptions", [
            "ENV",
            "$resource",
            "eventStream",
            "userSessionModel",
            SecurityQuestionOptions
        ]);
})(angular);
