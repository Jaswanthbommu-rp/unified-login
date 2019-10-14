// User Details Resource

(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("user.userDetails");

        function usernameLabel(data) {
            var bool = data == 404;
            return bool ? "Username" : "Email (Username)";
        }

        function notificationLabel(data) {
            if (!data) {
                return "Notification Email (optional)";
            }
            return "Notification Email";
        }

        function usernameErrorMsg(data) {
            var bool = data == 404;
            return bool ? "Username is required" : "Email (Username) is required";
        }

        function notificationEmailErrorMsg(data) {
            return "Notification Email is required because " + data + " selected";
        }

        bundle.set({
            "text.accessDetails.title": "Access Details",
            "text.profileInfo.title": "Profile Information (optional)",

            "label.firstName": "First Name",
            "label.middleName": "Middle Initial",
            "label.lastName": "Last Name",
            "label.userTypeId": "User Type",
            "label.loginName": usernameLabel,
            "label.password": "Password",
            "label.passwordCopy": "Re-enter Password",
            "label.notificationEmail": notificationLabel,
            "label.fromDate": "User Effective",
            "label.thruDate": "User Expires (optional)",
            "label.isActive": "Enable access for this user?",
            "label.is3rdPartyIDP": "Use third party identity service provider?",
            "label.timeZoneOffset": "Time Zone",

            "label.phone": "Phone",
            "label.phoneType": "Phone Type",
            "label.contactMethod": "Preferred Contact Method",
            "label.industryJobTitle": "Industry Standard Job Title",
            "label.companyJobTitle": "Company Job Title",

            "options.userTypeId.401": "Regular User",
            "options.userTypeId.402": "RealPage System Administrator",
            "options.userTypeId.404": "Regular User (No Email)",
            "options.userTypeId.403": "RealPage Employee",
            "options.userTypeId.405": "External User",

            "errorMsgs.firstName.required": "First name is required",
            "errorMsgs.fromDate.required": "Effective date is required",
            "errorMsgs.lastName.required": "Last name is required",
            "errorMsgs.loginName.invalidLoginName": "Email is not valid",
            "errorMsgs.loginName.required": usernameErrorMsg,
            "errorMsgs.notificationEmail.pattern": "Notification email is not a valid email address",
            "errorMsgs.notificationEmail.required": notificationEmailErrorMsg,
            "errorMsgs.password.required": "Password is required",
            "errorMsgs.password.validPassword": "Password does not meet requirements",
            "errorMsgs.passwordCopy.passwordsMatch": "The passwords you typed do not match. Please try again",
            "errorMsgs.passwordCopy.required": "Please re-enter password",
            "errorMsgs.userTypeId.required": "User type name is required",
            "errorMsgs.TimeZoneOffset.required": "Time Zone is required",

			"User.CreateUser.1": "Username already exists in this company.",
			"User.CreateUser.2": "This user type already exists for this username.",
            "User.CreateUser.3": "Unable to Create New Person",
            "User.CreateUser.4": "Username already exists!",
            "User.CreateUser.5": "Unable to Create User Login",
			"User.CreateUser.6": "There was an error unassociating the user to a user role.",
            "User.CreateUser.7": "Error creating the user login status.",
			"User.CreateUser.8": "User has no persona.",
			"User.CreateUser.9": "Persona was not created.",
			"User.CreateUser.10": "There was an error associating the persona to a user role.",
			"User.CreateUser.11": "User Custom Fields was not created.",
			"User.CreateUser.12": "Persona name was not associated to the Persona.",
			"User.CreateUser.13": "Employer role is missing.",
			"User.CreateUser.14": "User Type role is missing.",
			"User.CreateUser.15": "User role(s) missing.",
			"User.CreateUser.16": "There was an error associating the user to a user role.",
			"User.CreateUser.17": "There was an error while new user profile update.",
			"User.CreateUser.18": "User Custom Fields was not created.",
			"User.CreateUser.19": "An error was encountered when creating a contact mechanism.",
			"User.CreateUser.20": "An error was encountered while linking user contact mechanism.",
			"User.CreateUser.21": "An error was encountered when assigning a usage type to the contact mechanism.",
			"User.CreateUser.22": "An error was encountered when creating an email address.",
			"User.CreateUser.23": "Link Identity Provider to UserLogin failed.",
            "User.CreateUser.24": "Unable to create new user.",
			"User.CreateUser.25": "No value(s) were provided for enabled and required custom field(s).",
			"User.CreateUser.26": "An error was encountered when updating a multi-company user First, Middle, and/or Last name.",

            "User.UpdateUser.1": "Profile is required.",
            "User.UpdateUser.2": "User already exists",
            "User.UpdateUser.3": "User roles are missing.",
            "User.UpdateUser.4": "Username is required.",
            "User.UpdateUser.5": "Invalid user type.",
            "User.UpdateUser.6": "Notification email is not a valid email address.",
            "User.UpdateUser.7": "Username is not a valid email address.",
            "User.UpdateUser.8": "Effective date is required.",
            "User.UpdateUser.9": "Unable to save user.",
            "User.UpdateUser.10": "First name is required.",
            "User.UpdateUser.11": "Last name is required.",
            "User.UpdateUser.12": "User exists in a different organization.",
            "User.UpdateUser.13": "Resident Portal User Access: You do not have the permissions to edit this user's role.",
            "User.UpdateUser.14": "Ability to update RealPage Access user profile is not allowed.",
            "User.UpdateUser.15": "Editor doesn't have rights to assign products.",
			"User.UpdateUser.16": "No value(s) were provided for enabled and required custom field(s)."
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
