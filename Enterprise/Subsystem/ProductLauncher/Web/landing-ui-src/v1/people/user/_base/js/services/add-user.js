//  Manage User Service

(function (angular) {
    "use strict";

    function addUserSvc($resource, moment, ENV) {
        var svc = {};        

        svc.createNewUser = function (userData) {
            var url = ENV.landingAPI + "api/newuser",

                actions = {
                    createNewUser: {
                        method: "POST"
                    }
                },

                params = {
                    userType: userData.userType
                },

                payload = {
                    userLogin: {
                        loginName: userData.username,
                        fromDate: null,
                        thruDate: null,                            
                        isActive: userData.isEnabled,
                    },

                    firstName: userData.firstName,
                    middleName: userData.middleInitial,
                    lastName: userData.lastName,
                };

            if(userData.startDate) {
                payload.userLogin.fromDate = moment(userData.startDate, "MM/DD/YYYY").utc();
            }
            if(userData.endDate) {
                payload.userLogin.thruDate = moment(userData.endDate, "MM/DD/YYYY").utc();
            }

            return $resource(url, params, actions).createNewUser(payload).$promise;
        };

        return svc;
    }

    angular
        .module("settings")
        .factory("manageUserSvc", [
            "$resource",
            "moment",
            "ENV",
            addUserSvc
        ]);
})(angular);