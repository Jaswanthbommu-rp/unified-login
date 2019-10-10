// Username Config

(function (angular, undefined) {
    "use strict";

    function UsernameConfig($filter, inputTextConfig) {
        var errorMsgs,
            index = 0,
            svc = this;

        errorMsgs = [
            {
                "name": "required",
                "text": $filter("userDetailsText")("err_useremail_required")
            }
        ];

        svc.get = function (data) {
            index++;

            data = angular.extend({
                required: true,
                id: "username-" + index,
                fieldName: "username-" + index,
                errorMsgs: errorMsgs,
                label: $filter("userDetailsText")("username_email")
            }, data || {});

            return inputTextConfig(data);
        };
    }

    angular
        .module("settings")
        .service("usernameConfig", [
            "$filter",
            "rpFormInputTextConfig",
            UsernameConfig
        ]);
})(angular);
