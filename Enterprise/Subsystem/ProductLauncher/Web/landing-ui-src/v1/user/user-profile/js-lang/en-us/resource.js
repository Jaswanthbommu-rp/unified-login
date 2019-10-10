(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("userEditProfile");

        bundle.set({
            "label.phone": "Phone",
            "label.phoneType": "Phone Type",
            "label.contactMethod": "Preferred Contact Method",
            "label.industryJobTitle": "Industry Standard Job Title",
            "label.companyJobTitle": "Company Job Title",
            "statusMsg.title.error": "Error",
            "statusMsg.title.success": "Success",
            "statusMsg.msg.editUser.success": "User was updated successfully.",
            "statusMsg.msg.editUser.error": "User update failed.",
            "cancelBtn": "Cancel",
            "updateBtn": "Save"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
