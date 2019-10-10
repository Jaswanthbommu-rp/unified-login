// User Base Keys

(function() {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "text.btn.cancel",
            "text.btn.createUser",
            "text.btn.updateUser",

            "statusMsg.title.success",
            "statusMsg.title.error",

            "statusMsg.msg.addUser.success",
            "statusMsg.msg.addUser.error",

            "statusMsg.msg.editUser.success",
            "statusMsg.msg.editUser.error",

            "statusMsg.msg.cloneUser.success",
            "statusMsg.msg.cloneUser.error"
        ];

        appLangKeys.app("user.base").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
