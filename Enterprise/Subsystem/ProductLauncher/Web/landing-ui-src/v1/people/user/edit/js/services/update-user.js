//  Update User Service

(function (angular) {
    "use strict";

    function UpdateUserSvc($resource, ENV) { 
        var svc = this;

        svc.save = function(userData) {
            var url = ENV.landingAPI + "api/user/profiledetail",
                actions = {
                    save: {
                        method: "PUT"
                    }
                },
                params = {
                    userType: userData.userType
                },
                payload = {
                    realPageId: userData.realPageId,
                    userLogin: {
                        loginName: userData.username,
                        isActive: userData.isEnabled
                    }
                };

            if(userData.startDate && userData.startDate.isValid()) {
                payload.userLogin.fromDate = userData.startDate.utc();
            } else {
                payload.userLogin.fromDate = null;
            }

            if(userData.endDate && userData.endDate.isValid()) {
                payload.userLogin.thruDate = userData.endDate.utc();
            } else {
                payload.userLogin.thruDate = null;
            }

            if(userData.personas) {
                var personaPayload = svc.getPersonaPayload(userData.personas);

                payload.persona = personaPayload.activePersonas;
                payload.inactivePersona = personaPayload.inactivePersonas;
            }

            return $resource(url, params, actions).save(payload).$promise;
        };


        svc.getPersonaPayload = function(personas) {
            var payload = {
                activePersonas: [],
                inactivePersonas: []
            };

            angular.forEach(personas, function(persona) {
                if(persona.isEdited === false) {
                    return;
                }

                var newPersonaJson = {};
                if(!persona.isActive) {
                    payload.inactivePersonas.push({
                        personaId: persona.data.personaId
                    });
                } else {
                    newPersonaJson = {
                        personaId: persona.data.personaId,
                        personaTypeId: persona.data.personaTypeId,
                        personaEnvironmentTypeId: persona.data.type,
                        name: persona.data.name
                    };

                    if(persona.data.startDate && persona.data.startDate.isValid()) {
                        newPersonaJson.fromDate = persona.data.startDate.utc();                    
                    } else {
                        newPersonaJson.fromDate = null;
                    }

                    if(persona.data.endDate && persona.data.endDate.isValid()) {
                        newPersonaJson.thruDate = persona.data.endDate.utc();                    
                    } else {
                        newPersonaJson.thruDate = null;
                    }

                    if(persona.data.notificationEmail) {
                        newPersonaJson.contactMechanism = [{
                            contactMechanismId: persona.data.notificationEmailId,
                            addressString: persona.data.notificationEmail,

                            //notification email
                            contactMechanismUsageType: {
                                contactMechanismUsageTypeId: 301
                            }
                        }];
                    }

                    payload.activePersonas.push(newPersonaJson);
                }                
            });

            return payload;
        };

    }

   angular
        .module("settings")
        .service("updateUserSvc", [
            "$resource",
            "ENV",
            UpdateUserSvc
        ]);
})(angular);