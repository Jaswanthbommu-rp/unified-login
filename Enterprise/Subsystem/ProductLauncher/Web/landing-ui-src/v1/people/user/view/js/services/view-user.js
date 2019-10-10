// Initialize data for View Users Service

(function (angular) {
    "use strict";

    function ViewUserSvc($resource, ENV) { 
        var svc = this;

        svc.getUserProfile = function(realpageID) {
            var url = ENV.landingAPI + "api/profiles/details?realPageId=" + realpageID;
            return $resource(url).get().$promise
                        .then(svc.formatResponse);
        };

        svc.getUserPersonas = function(realpageID) {
            var url = ENV.landingAPI + "api/person/personas/" + realpageID;
            var actions = {
                get: {
                    method: "GET",
                    params: {},
                    isArray: true
                }
            };
            return $resource(url, {}, actions).get().$promise;  
        };

        svc.formatResponse = function(response) {
            var newResponse = {};

            var profileJSON = response.data,
                latestOrganization = svc.getLatestOrganization(profileJSON.organization);
            newResponse.userSummary = profileJSON;
            newResponse.userSummary.latestOrganization = latestOrganization.organization;
            newResponse.userSummary.latestRole = latestOrganization.role;
            newResponse.userSummary.latestEmail = svc.getLatestEmail(profileJSON.contactMechanism);
            newResponse.userSummary.latestContactNum = svc.getLatestContactNum(profileJSON.contactMechanism);

            return newResponse;
        };

        svc.getLatestOrganization = function(arr) {
            var latest = svc.getLatest(arr);
            if(latest) {
                return {
                    organization: latest.name,
                    role: latest.partyRelationship.roleTypeFrom.name
                };
            }
            return null;
        };

         svc.getLatestEmail = function(arr) {
            if(arr) {
                for(var i=0, max=arr.length; i<max; i++) {
                    var currContact = arr[i],
                        addressType = currContact.addressType || "";

                    addressType = addressType.toLowerCase();
                    if(addressType == "email" || addressType == "e-mail") {
                        return currContact.addressString;
                    }
                }                
            }

            return null;
        };

        svc.getLatestContactNum = function(arr) {
            if(arr) {
                for(var i=0, max=arr.length; i<max; i++) {
                    var currContact = arr[i],
                        addressType = currContact.addressType || "";

                    addressType = addressType.toLowerCase();
                    if(addressType == "telecommunications number") {
                        return currContact.addressString;
                    }
                }                
            }

            return null;
        };

        svc.getLatest = function(arr) {
            if(angular.isUndefined(arr) || arr === null) {
                return null;
            }

            return arr[0]; //as per API, first index is sorted as latest
        };

    }

   angular
        .module("settings")
        .service("viewUserSvc", [
            "$resource",
            "ENV",
            ViewUserSvc
        ]);
})(angular);