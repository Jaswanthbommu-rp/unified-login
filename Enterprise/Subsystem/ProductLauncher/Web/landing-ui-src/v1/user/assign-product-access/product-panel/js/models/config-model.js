//  Config Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

                      
        model.setGridConfig = function (config) {            
             model.gridconfig = config ;
        };

        model.getGridConfig = function (config) {            
            return model.gridconfig ;
        };

        model.setRadioConfig = function (config) {            
             model.radioconfig = config ;
        };

        model.getRadioConfig = function (config) {            
            return model.radioconfig ;
        };

        model.setSwitchConfig = function (config) {            
             model.switchconfig = config ;
        };

        model.getSwitchConfig = function (config) {            
            return model.switchconfig ;
        };

        model.reset = function () {
        	model = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("ConfigModel", [
        	factory
        ]);
})(angular);
