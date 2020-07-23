(function (angular) {
    "use strict";

    function factory(user, personaSvc, eventStream) {
        var model = {
            data: {}
        };

        model.events = {
            update: eventStream()
        };

        model.init = function () {
            if (user.isReady()) {
                model.loadData();
            }
            else {
                user.subscribe(model.loadData);
            }
        };

        // Setters

        model.setData = function (data) {
            model.ready = true;
            model.data = data || {};
            model.events.update.publish(data);
        };

        // Getters

        model.getId = function () {
            return model.data.personaId;
        };

        model.getOrgRealPageID = function () {
            return model.data.organization.realPageId;
        };

        model.getPersonaRealPageID = function () {
            return model.data.realPageId;
        };

        model.getCompanyName = function () {
            return model.data.organization.name;
        };

        model.getBooksMasterId = function () {
            return model.data.organization.booksMasterId;
        };

        model.getUserRole = function () {
            return model.data.name;
        };

        // Assertions
		model.isPersonaIsRegularUser = function () {
            return model.data.userTypeId != "402";
        };
		
        model.hasResidentPortalUserAccess = function () {
            return model.data.hasResidentPortalUserAccess;
        };

        model.hasViewOnlySupportToolAccess = function () {
            return model.data.hasViewOnlySupportToolAccess;
        };

        model.hasManageDepositAlternativeProductAccess = function () {
            return model.data.hasManageDepositAlternativeProductAccess;
        };

        model.hasManageClickPayProductAccess = function () {
            return model.data.hasManageClickPayProductAccess;
        };

        model.hasImportUsersAccess = function () {
            return model.data.hasImportUsersAccess;
        };

        model.hasViewOnlySettingsAccess = function () {
            return model.data.hasViewOnlySettingsAccess;
        };

        model.hasManagePlatFormSecurity = function () {
            return model.data.hasManagePlatFormSecurity;
        };
        
        model.hasManageSettingsTemplates = function () {
            return model.data.hasManageSettingsTemplates;
        };

        model.hasManageCustomFields = function () {
            return model.data.hasManageCustomFields;
        };

        model.hasManageUnifiedSettings = function () {
            return model.data.hasManageUnifiedSettings;
        };

        model.hasAccessSettingsAdmin = function () {
            return model.data.hasAccessSettingsAdmin;
        };

        model.hasnotificationsAccess = function () {
            return model.data.hasnotificationsAccess;
        };

        model.hasPlatformAlertsAccess = function(){
            return model.data.hasPlatformAlertsAccess;
        };

        model.isReady = function () {
            return model.ready;
        };

        // Actions

        model.loadData = function () {
            personaSvc.get(model.setData);
        };

        model.subscribe = function (callback) {
            return model.events.update.subscribe(callback);
        };

        return model;
    }

    angular
        .module("settings")
        .factory("personaDetails", [
            "userSessionModel",
            "personaSvc",
            "eventStream",
            factory
        ]);
})(angular);
