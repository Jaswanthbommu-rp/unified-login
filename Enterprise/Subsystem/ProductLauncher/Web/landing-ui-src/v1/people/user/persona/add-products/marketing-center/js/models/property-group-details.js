//  Marketing Center Property Group Detail Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        model.setName = function (name) {
        	model.name = name;
        };

        model.getName = function() {
        	return model.name;
        };

        model.reset = function () {
        	model = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("MCPropertyGroupDetailsModel", [
        	factory
        ]);
})(angular);
