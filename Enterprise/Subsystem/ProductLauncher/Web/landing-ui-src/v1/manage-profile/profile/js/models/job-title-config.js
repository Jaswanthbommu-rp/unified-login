//  Job Title Config

(function (angular, undefined) {
    "use strict";

    function JobTitleMenuConfig(menuConfig, optionsSvc) {
        var errorMsgs,
            index = 0,
            svc = this;

        errorMsgs = [
            {
                name: "required",
                text: "Job title is required"
            }
        ];

        svc.get = function (data) {
            index++;

            data = angular.extend({
                required: true,
                nameKey: "name",
                errorMsgs: errorMsgs,
                id: "job-title-" + index,
                valueKey: "partyRoleTypeId",
                fieldName: "job-title-" + index
            }, data || {});

            var config = menuConfig(data);
            optionsSvc.get(config.setOptions.bind(config));
            return config;
        };
    }

    angular
        .module("settings")
        .service("jobTitleMenuConfig", [
            "rpFormSelectMenuConfig",
            "jobTitleOptions",
            JobTitleMenuConfig
        ]);
})(angular);
