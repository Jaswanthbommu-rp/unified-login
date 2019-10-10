//  User Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        model.setData = function (data) {
        	model.data = data;
        };

        model.setError = function(data) {
            model.isError = true;

            if(data && data.errorReason) {
                model.errorReason = data.errorReason;
            }
        };

        model.reset = function () {
        	model = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("viewUserSummaryModel", [
        	factory
        ]);
})(angular);
