(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "label.phone",
            "label.phoneType",
            "label.contactMethod",
            "label.industryJobTitle",
            "label.companyJobTitle",
            "statusMsg.title.success",
            "statusMsg.title.error",
            "statusMsg.msg.editUser.success",
            "statusMsg.msg.editUser.error",

            "cancelBtn",
            "updateBtn"
        ];

        appLangKeys.app("userEditProfile").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
