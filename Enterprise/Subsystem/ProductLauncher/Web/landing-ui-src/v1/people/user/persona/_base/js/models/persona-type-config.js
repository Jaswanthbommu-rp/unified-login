//  Persona Type Config

(function (angular, undefined) {
    "use strict";

    function PersonaTypeMenuConfig($filter, menuConfig, optionsSvc) {
        var errorMsgs,
            index = 0,
            svc = this;

        errorMsgs = [
            {
                name: "required",
                text: $filter("manageUserPersonaText")("err_req_persona_type")
            }
        ];

        svc.get = function (data) {
            index++;

            data = angular.extend({
                required: true,
                nameKey: "name",
                valueKey: "personaEnvironmentTypeId",
                errorMsgs: errorMsgs,
                id: "persona-type-" + index,
                fieldName: "persona-type-" + index
            }, data || {});

            var config = menuConfig(data);
            optionsSvc.get(config.setOptions.bind(config));
            return config;
        };
    }

    angular
        .module("settings")
        .service("personaTypeMenuConfig", [
            "$filter",
            "rpFormSelectMenuConfig",
            "personaTypeOptions",
            PersonaTypeMenuConfig
        ]);
})(angular);
