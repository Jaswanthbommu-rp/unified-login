// Notification Email Config

(function (angular, undefined) {
    "use strict";

    function NotificationEmailConfig($filter, inputTextConfig) {
        var errorMsgs = [],
            index = 0,
            svc = this;

        errorMsgs = [
            {
                "name": "email",
                "text": $filter("userDetailsText")("err_notif_email_invalid")
            }
        ];

        svc.get = function (data) {
            index++;

            data = angular.extend({
                required: false,
                id: "email-" + index,
                fieldName: "email-" + index,
                dataType: "email",
                errorMsgs: errorMsgs,

                isVisible: false,
                label: $filter("userDetailsText")("notification_email")
            }, data || {});

            return inputTextConfig(data);
        };
    }

    angular
        .module("settings")
        .service("notificationEmailConfig", [
            "$filter",
            "rpFormInputTextConfig",
            NotificationEmailConfig
        ]);
})(angular);
