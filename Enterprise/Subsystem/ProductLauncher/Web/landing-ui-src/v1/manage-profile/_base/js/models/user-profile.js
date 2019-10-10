//  User Model

(function (angular) {
    "use strict";

    function factory(eventStream) {
        var model = {
            user: {}
        };

        model.events = {
            update: eventStream()
        };

        // Setters

        model.setData = function (userData) {
            model.user = userData;
        };

        model.setProfileCardCaller = function (bool) {
            model.ProfileCardCaller = bool;
        };

        // Getters

        model.subscribe = function(method) {
            return model.events.update.subscribe(method);
        };

        model.publish = function(data) {
            model.events.update.publish(data);
        };

        model.getFullName = function () {
            if (model.user.firstName && model.user.lastName) {
                return model.user.firstName + " " + model.user.lastName;
            }

            return null;
        };

        model.getRealPageId = function () {
            if (model.user.realPageId) {
                return model.user.realPageId;
            }

            return null;
        };

        model.getUsername = function () {
            if (model.user.userLogin) {
                return model.user.userLogin.loginName;
            }

            return null;
        };

        // Assertions

        model.isProfileCardCaller = function () {
            if (model.ProfileCardCaller) {
                return true;
            }
            return false;
        };
        
        model.reset = function () {
            model.user = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("userProfileModel", [
            "eventStream",
            factory]);
})(angular);
