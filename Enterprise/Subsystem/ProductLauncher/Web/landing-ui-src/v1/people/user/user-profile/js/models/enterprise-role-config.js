// Username Config

(function (angular, undefined) {
    "use strict";

    function UsernameConfig($filter, inputTextConfig) {
        var //errorMsgs,
            index = 0,
            svc = this;

        // errorMsgs = [
        //     {
        //         "name": "required",
        //         "text": $filter("userDetailsText")("err_username_required")
        //     }
        // ];

        svc.get = function (data) {
            index++;

            data = angular.extend({
                id: "enterprise-role-" + index,
                fieldName: "enterprise-role-" + index,
                iconClass: "rp-icon-search2",

                isVisible: true,
                label: $filter("userDetailsText")("enterprise_role")
            }, data || {});

            return inputTextConfig(data);
        };
    }

    angular
        .module("settings")
        .service("enterpriseRoleConfig", [
            "$filter",
            "rpFormInputTextConfig",
            UsernameConfig
        ]);
})(angular);
