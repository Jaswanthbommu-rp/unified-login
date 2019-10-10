//  User Model

(function (angular) {
    "use strict";

    function factory($location, $timeout) {
        var model = {},
        	userDefault;

        userDefault = {
        	enterpriseUserName: "",
        	activityToken: "",
        	validateUserToken: "",
        	isValidated: false,
        	isError: false,
            errorReason: "",
        };

        model.init = function () {
        	model.user = angular.copy(userDefault);

        	return model;
        };

        model.setTokenData = function (data) {
            model.user.validateUserToken = data.validateUserToken;
            model.user.isError = data.isError;
            model.user.errorReason = data.errorReason;
        };

        model.getEnterpriseUserName = function () {
        	return model.user.enterpriseUserName;
        };

        model.setEnterpriseUserName = function (username) {
        	model.user.enterpriseUserName = username;

        	return model;
        };

        model.getActivityToken = function () {
            return model.user.activityToken;
        };

        model.setActivityToken = function (activityToken) {
            model.user.activityToken = activityToken;

            return model;
        };

        model.getIsValidated = function () {
            return model.user.isValidated;
        };

        model.setIsValidated = function(isValid) {
            if (isValid === true) {
                model.user.isValidated = true;
            } else {
                model.user.isValidated = false;
            }

            return model;
        };

        model.checkUserValidated = function () {            
            if (model.getIsValidated()) {
                return true;
            } else {
                $timeout(function () {
                    $location.path("/error").replace();
                }, 100);
                
                return false;
            }
        };        

        model.checkUserToken = function () {
            return model.user.validateUserToken && model.user.isError === false;
        };

        model.isDataError = function () {
            return model.user.isError === true && model.user.errorReason;
        };

        model.getErrorReason = function () {
            return model.user.errorReason;
        };

        model.reset = function () {
        	model.user = angular.copy(userDefault);

        	return model;
        };

        return model.init();
    }

    angular
        .module("new-user")
        .factory("userModel", [
            "$location",
            "$timeout",
        	factory
        ]);
})(angular);
