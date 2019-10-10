(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            "pwd_notice_change_pwd",
                
            "pwd_notice_change_pwd_title",
            "pwd_notice_change_pwd_msg",
            "pwd_notice_change_pwd_last_msg",

            "pwd_notice_new_pwd_title",
            "pwd_notice_new_pwd_msg",

            "pwd_notice_pwd_expired_title",
            "pwd_notice_pwd_expired_msg"


        ];

        appLangKeys.app("common.notification-lang").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
