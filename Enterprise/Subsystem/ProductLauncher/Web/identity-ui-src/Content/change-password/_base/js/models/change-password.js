(function(angular) {
	"use strict";

    var changePasswordModel = function(baseForm) {
        var model = baseForm();
        return model;
    };

    angular
        .module("identity")
        .factory("changePasswordModel", [
            "baseForm",
            changePasswordModel
        ]);

})(angular);