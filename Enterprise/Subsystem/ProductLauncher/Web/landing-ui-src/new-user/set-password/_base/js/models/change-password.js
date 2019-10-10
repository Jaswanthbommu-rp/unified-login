(function(angular) {
	"use strict";

	var changePasswordModel = function (baseForm) {
	    var model = baseForm();

        model.getCreatePassword = function() {
            return model.form.createPassword;
        };

        model.getConfirmPassword = function() {
            return model.form.confirmPassword;
        };

	    return model;
	};

    angular
        .module("new-user")
        .factory("changePasswordModel", [
            "baseForm",
            changePasswordModel
        ]);

})(angular);