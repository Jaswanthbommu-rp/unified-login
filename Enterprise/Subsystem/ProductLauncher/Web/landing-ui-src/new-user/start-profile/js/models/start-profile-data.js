//  Start Profile Form Data Model

(function(angular) {
    "use strict";

    function factory() {

        return {
            "username": "",
            "industryJobTitle": "",
            "companyJobTitle": "",
            "phoneNumber": "",
            "telecomNumbers": [{
                "partyContactMechanismId": 0,
                "contactMechanismId": 0,
                "countryCode": "",
                "areaCode": "",
                "phoneNumber": "",
                "isDeleted": false,
                "contactMechanismUsageType": {
                    "contactMechanismUsageTypeId": null,
                    "parentContactMechanismUsageTypeId": 200,
                    "name": ""
                }
            }],
            "electronicEmails": [{
                "partyContactMechanismId": 0,
                "contactMechanismId": 0,
                "addressString": null,
                "addressType": "Email",
                "contactMechanismUsageType": {
                    "contactMechanismUsageTypeId": 302,
                    "parentContactMechanismUsageTypeId": 300,
                    "name": "Email"
                }
            }],
            "phoneType": "",
            "submitBtnDisabled": false
        };
    }

    angular
        .module("new-user")
        .factory("startProfileFormData", [factory]);
})(angular);