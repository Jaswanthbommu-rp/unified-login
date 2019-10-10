//  Dashboard User Service

(function (angular) {
    "use strict";

    function userSvc($q, $resource, ENV, links) {
        var svc = {};

        svc.resource = $resource(ENV.landingAPI + "api/dashboard");

        svc.getDashboardData = function () {
            svc.dataReq = $q.defer();
            svc.resource.get(svc.formatResponse);
            return svc.dataReq.promise;
        };

        //adapter for API response
        svc.formatResponse = function (response) {
            var newResponse = {};

            newResponse.dashboard = {};

            newResponse.dashboard.productSummary = response.dashboardElements.profileDetail.assignedProducts;
            newResponse.dashboard.resources = response.dashboardElements.resources;

            var profileJSON = response.dashboardElements.profileDetail,
                latestOrganization = svc.getLatestOrganization(profileJSON.organization);
            newResponse.dashboard.userSummary = profileJSON;
            newResponse.dashboard.userSummary.latestOrganization = latestOrganization.organization;
            newResponse.dashboard.userSummary.latestRole = latestOrganization.role;
            newResponse.dashboard.userSummary.latestEmail = svc.getLatestEmail(profileJSON.contactMechanism);
            newResponse.dashboard.userSummary.latestContactNum = svc.getLatestContactNum(profileJSON.contactMechanism);

            return newResponse;
        };

        svc.getLatestOrganization = function (arr) {
            var latest = svc.getLatest(arr);
            if (latest) {
                return {
                    organization: latest.name,
                    role: latest.partyRelationship.roleTypeFrom.name
                };
            }
            return null;
        };

        svc.getLatestEmail = function (arr) {
            if (arr) {
                for (var i = 0, max = arr.length; i < max; i++) {
                    var currContact = arr[i],
                        addressType = currContact.addressType || "";

                    addressType = addressType.toLowerCase();
                    if (addressType == "email" || addressType == "e-mail") {
                        return currContact.addressString;
                    }
                }
            }

            return null;
        };

        svc.getLatestContactNum = function (arr) {
            if (arr) {
                for (var i = 0, max = arr.length; i < max; i++) {
                    var currContact = arr[i],
                        addressType = currContact.addressType || "";

                    addressType = addressType.toLowerCase();
                    if (addressType == "telecommunications number") {
                        return currContact.addressString;
                    }
                }
            }

            return null;
        };

        svc.getLatest = function (arr) {
            if (angular.isUndefined(arr) || arr === null) {
                return null;
            }

            return arr[0]; //as per API, first index is sorted as latest
        };

        return svc;
    }

    angular
        .module("settings")
        .factory("userSummarySvc", [
            "$q",
            "$resource",
            "ENV",
            "externalLinks",
            userSvc
        ]);
})(angular);
