//  User Lookup Model

(function (angular) {
    "use strict";

    function userLookupModel() {
        var model = {},
            formDefault,
            error;

        model.form = {};
        model.error = {};

        formDefault = {
            username: "",
            submitBtnDisabled: false
        };

        error = {
            state: false,
            message: ""
        };

        model.init = function () {
            model.reset();

            return model;
        };

        model.setFormUsername = function (username) {
            if(model.form){
                model.form.username = username;
            }
        };

        model.setError = function (errorMsg) {
            model.error.state = true;
            model.error.message = errorMsg;

            return model;
        };

        model.clearError = function () {
            model.error = angular.extend({}, error);

            return model;
        };

        model.setSubmitBtnDisabled = function (state) {
            model.form.submitBtnDisabled = state === undefined || state === true ? true : false;

            return model;
        };

        model.reset = function () {
            model.form = angular.extend({}, formDefault);
            model.clearError();

            return model;
        };

        return model.init();
    }

    angular
        .module("identity")
        .factory("userLookupModel", userLookupModel);
})(angular);