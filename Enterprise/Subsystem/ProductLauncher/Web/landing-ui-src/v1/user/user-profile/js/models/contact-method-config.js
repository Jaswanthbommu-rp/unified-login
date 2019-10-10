//  Contact Method Config

(function (angular, undefined) {
    "use strict";

    function ContactMethodMenuConfig(menuConfig, optionsSvc) {
        var errorMsgs,
            index = 0,
            svc = this;

        errorMsgs = [
            {
                name: "required",
                text: "Preferred contact method is required"
            }
        ];

        svc.get = function (data) {
            index++;

            data = angular.extend({
                required: false,
                nameKey: "name",
                errorMsgs: errorMsgs,
                id: "contact-method-" + index,
                fieldName: "contact-method-" + index,
                valueKey: "preferredContactMethodId"
            }, data || {});

            var config = menuConfig(data);
            optionsSvc.get(config.setOptions.bind(config));
            return config;
        };
    }

    angular
        .module("settings")
        .service("contactMethodMenuConfig", [
            "rpFormSelectMenuConfig",
            "contactMethodOptions",
            ContactMethodMenuConfig
        ]);
})(angular);
