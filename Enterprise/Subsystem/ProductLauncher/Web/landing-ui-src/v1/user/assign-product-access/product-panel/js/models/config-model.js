//  Config Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        
        model.setConfig = function (config) {            
             model.config = config ;
        };

        model.getConfig = function (name) {     
            var retObj;       
            model.config.forEach(function (item) {
                if(item[name]){
                    retObj = item[name] ;
                }
            });
            return retObj;
        };

        model.setGridConfig = function (config) {            
             model.gridconfig = config ;
        };

        model.getGridConfig = function () {            
            return model.gridconfig ;
        };

        model.setRadioConfig = function (config) {            
             model.radioconfig = config ;
        };

        model.getRadioConfig = function () {            
            return model.radioconfig ;
        };

        model.setSwitchConfig = function (config) {            
             model.switchconfig = config ;
        };

        model.getSwitchConfig = function () {            
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
