(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("profileSetup");

        bundle.set({

            profile_welcome: "Welcome to My RealPage",
            profile_instructions: "Please tell us a little bit about yourself.",

            profile_username: "Username",
            profile_industry_job_title: "Industry Standard Job Title",
            profile_company_job_title: "Company Job Title",
            profile_phone_number: "Phone",
            profile_phone_type: "Type",

            profile_save: "Save and Continue",
            profile_cancel: "Skip for now",

            profile_system_err_contact_admin: "A system error has occurred. Please contact your system administrator.",

            // profile_req_err_username: "Username is required",
            // profile_req_err_industry_job_title: "Industry Standard Job Title is required",
            // profile_req_err_company_job_title: "Company Job Title is required",
            // profile_req_err_phone_number: "Phone Number is required",
            // profile_req_err_phone_type: "Phone Type is required",

            profile_req_err: "Required"

        });

        bundle.test();
    }

    angular
        .module("new-user")
        .config(["appLangBundleProvider", config]);
})();