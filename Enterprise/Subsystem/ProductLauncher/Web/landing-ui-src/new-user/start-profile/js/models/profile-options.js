//  Start Profile Form Options Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {},
            defaultObj = {
                list: [],
                isReady: false
            };

        model.options = {
            industryJobTitle: angular.copy(defaultObj),
            phoneType: angular.copy(defaultObj)
        };

        model.isReady = function() {
            var isReady = true;

            angular.forEach(model.options, function(curr) {
                if(curr.isReady === false) {
                    isReady = false;
                }
            });

            return isReady;
        };

        model.initOptions = function(type, list) {
            if(!model.options[type]) {
                logc("ERROR: Invalid option type");
            } else {
                model.options[type].list = list;
                model.options[type].isReady = true;
            }
        };

        model.getOptions = function(type) {
            if(!model.options[type]) {
                logc("ERROR: Invalid option type");
                return [];
            } else {
                return model.options[type].list;
            }
        };

        return model;
    }

    angular
        .module("new-user")
        .factory("startProfileOptionsModel", [
        	factory
        ]);
})(angular);
