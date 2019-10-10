//  User Type Select Menu Config

(function (angular, undefined) {
    "use strict";

    function UserTypeMenuConfig($filter, menuConfig, userTypes) {
        var errorMsgs,
            index = 0,
            svc = this;

        errorMsgs = [
            {
                name: "required",
                text: $filter("userDetailsText")("err_user_type_required")
            }
        ];

        svc.get = function (data) {
            index++;

            data = angular.extend({
                required: true,
                nameKey: "label",
                valueKey: "value",
                errorMsgs: errorMsgs,
                id: "user-type-" + index,
                fieldName: "user-type"
            }, data || {});

            var config = menuConfig(data);
            // optionsSvc.get(config.setOptions.bind(config));
            config.setOptions(userTypes.getOptions());
            return config;
        };
    }

    angular
        .module("settings")
        .service("userTypeMenuConfig", [
            "$filter",
            "rpFormSelectMenuConfig",
            "userTypes",
            UserTypeMenuConfig
        ]);
})(angular);
