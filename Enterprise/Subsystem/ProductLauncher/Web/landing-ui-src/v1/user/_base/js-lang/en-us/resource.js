// User Base Text Resource

(function() {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("user.base");

        bundle.set({
            "text.btn.cancel": "Cancel",
            "text.btn.createUser": "Create User",
            "text.btn.updateUser": "Update User",

            "statusMsg.title.error": "Error",
            "statusMsg.title.success": "Success",

            "statusMsg.msg.addUser.success": "User was created successfully.",
            "statusMsg.msg.addUser.error": "User creation failed.",

            "statusMsg.msg.editUser.success": "User was updated successfully.",
            "statusMsg.msg.editUser.error": "User update failed.",

            "statusMsg.msg.cloneUser.success": "User was created successfully.",
            "statusMsg.msg.cloneUser.error": "User creation failed.",

            "User.GetProfile.3" : "User exists in a different organization."
            // "User.GetProfile.4" : "Resident Portal User Access: You do not have the permissions to edit this user's role."
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
