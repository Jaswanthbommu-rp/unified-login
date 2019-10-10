//  Manage User Form Model

(function (angular) {
    "use strict";

    function factory(baseForm) {
        var model = baseForm();

        //setters

        model.setRealPageId = function(rpId) {
            model.form.realPageId = rpId;
            return model;
        };

        model.setNotificationEmail = function(email, emailId) {
            model.form.notificationEmail = email;
            model.form.notificationEmailId = emailId;
            return model;
        };

        model.setUserType = function(userType) {
            model.form.userType = userType;
            return model;
        };

        //getters 

        model.getStartDate = function() {
            return model.form.startDate;
        };

        model.getPersona = function(id) {
            return model.form.personas[id];
        };

        model.getUserType = function() {
            return model.form.userType;
        };

        //assertions

        model.hasAddedPersona = function() {
            return model.form.personas && model.form.personas.length > 1;
        };

        //actions

        model.addPersona = function(newPersona) {
            model.form.personas[newPersona.data.tabID] = newPersona;
        };

        model.updateData = function(data) {
            angular.extend(model.form, data);
        };

        return model;
    }

    angular
        .module("settings")
        .factory("manageUserFormModel", [
        	"baseForm",
        	factory
        ]);
})(angular);
