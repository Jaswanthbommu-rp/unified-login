(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            "profile_welcome",
            "profile_instructions",

            "profile_username",
            "profile_industry_job_title",
            "profile_company_job_title",
            "profile_phone_number",
            "profile_phone_type",

            "profile_save",
            "profile_cancel",

            "profile_system_err_contact_admin",

            "profile_req_err"

        ];

        appLangKeys.app("profileSetup").set(keys);
    }

    angular
        .module("new-user")
        .config(["appLangKeysProvider", config]);
})();
