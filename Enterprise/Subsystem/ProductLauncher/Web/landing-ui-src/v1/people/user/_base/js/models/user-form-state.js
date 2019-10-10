//  User Form model

(function (angular) {
    "use strict";

    function factory() {
        var model = {},
            defaultState = {
                isLoading: false,
                userDetailsForm: null,
                hasAddedPersona: false,

                showProductsLinking: true,
                isRolesReady: false //DELETEME
            };

        model.init = function() {
            model.state = angular.copy(defaultState);            
            return model;
        };

        //setters

        model.setIsLoading = function (state) {
            model.state.isLoading = state === undefined || state === true ? true : false;
        };

        model.setHasAddedPersona = function(flag) {
            model.state.hasAddedPersona = flag;
        };

        model.showProductsLinking = function() {
            model.state.showProductsLinking = true;
        };

        //assertions

        model.isLoading = function() {
            return model.state.isLoading;
        };

        model.hasAddedPersona = function() {
            return model.state.hasAddedPersona;
        };

        model.isShowProductsLinking = function() {
            return model.state.showProductsLinking;
        };

        //actions

        model.reset = function() {
            model.state = angular.copy(defaultState);
        };


        return model.init();
    }

    angular
        .module("settings")
        .factory("manageUserFormState", [
        	factory
        ]);
})(angular);
