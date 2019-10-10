(function(angular, undefined) {
    "use strict";

    function factory() {
        var model = {};

        model.init = function() {
            model.ready = false;
            model.isImpersonated = false;
            return model;
        };

        model.setData = function(bool) {
            model.isImpersonated = bool;
            model.setReady(true);
        };

        model.isUserImpersonated = function() {
            return model.isImpersonated;
        };

        model.isReady = function () {           
            return model.ready;
        };

        model.setReady = function (bool) {            
            model.ready = bool ;            
        };

        model.reset = function() {
            model.isImpersonated = false;
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("userImpersonated", [
            factory
        ]);
})(angular);