//  Manage User Service

(function (angular) {
    "use strict";

    function AddUserSvc($resource, moment, ENV) {
        var svc = this;

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
                    productBatch: userData.productBatch,
                    persona: []
                };

            if (userData.password) {
                payload.password = userData.password;
            }

            if (userData.notificationEmail) {
                payload.notificationEmail = userData.notificationEmail;
            }

            if (userData.startDate && userData.startDate.isValid()) {
                payload.userLogin.fromDate = userData.startDate.utc();
            }
            if (userData.endDate && userData.endDate.isValid()) {
                payload.userLogin.thruDate = userData.endDate.utc();
            }

            return $resource(url, params, actions).createNewUser(payload).$promise;
        };

        svc.getPersonaPayload = function (personas) {
            var json = [];

            angular.forEach(personas, function (persona) {
                if (persona.isEdited === false) {
                    return;
                }

                var newPersonaJson = {
                    personaEnvironmentTypeId: persona.data.type,
                    name: persona.data.name
                };

                if (persona.data.startDate && persona.data.startDate.isValid()) {
                    newPersonaJson.fromDate = persona.data.startDate.utc();
                }
                if (persona.data.endDate && persona.data.endDate.isValid()) {
                    newPersonaJson.thruDate = persona.data.endDate.utc();
                }

                json.push(newPersonaJson);
            });

            return json;
        };

        return svc;
    }

    angular
        .module("settings")
        .factory("addUserSvc", [
            "$resource",
            "moment",
            "ENV",
            AddUserSvc
        ]);
})(angular);
