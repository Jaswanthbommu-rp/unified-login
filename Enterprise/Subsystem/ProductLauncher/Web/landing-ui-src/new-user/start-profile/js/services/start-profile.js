//  Start Profile Service

(function(angular) {
    "use strict";

    function StartProfileSvc(ENV, $resource, userModel) {
        var svc = this,
            resource = $resource(ENV.landingAPI + "api/newuser/profile");

        svc.save = function(formData, phoneNumbers, emails) {
            var params = {
                    activityToken: userModel.getActivityToken(),
                    companyJobTitle: formData.companyJobTitle,
                    userLogin: formData.username,
                },

                payload = {
                    telecommunicationNumber: phoneNumbers,
                    emailContacts: emails,
                    partyRole: {
                        roleTypeId: formData.industryJobTitle
                    }
                };

            return resource.save(params, payload).$promise;
        };

    }

    angular
        .module("new-user")
        .service("startProfileSvc", [
            "ENV",
            "$resource",
            "userModel",
            StartProfileSvc
        ]);
})(angular);