//  change Order  Model

(function(angular) {
    "use strict";

    function factory(pubsub, user, svc, menuConfig, persona, $filter) {
        var model = {};

        model.init = function() {                        
            model.data = {};
            model.isReady = false;
            return model;
        };

        model.getCompData = function() {
            var params = { };

            svc.getData(params)
                .then(model.setData, model.setDataErr);
        };

        model.setData = function(resp) {            
            model.setAllCompanies(resp.data);
            // model.getAllCompanyOptions();
            model.setMenuOptions();
            model.setDefaultComp();
            model.isReady = true;
            pubsub.publish("compSwitch.setSelMenu");
        };

        model.setDataErr = function(data) {
            logc("Error: ", data);
        };

        model.setAllCompanies = function(data) {
            model.data = data;
        };

        model.getAllCompanies = function() {
            return model.data;
        };

        model.setSelCompId = function(data) {
            model.selval = data;
        };

        model.getSelCompId = function() {
            return model.selval;
        };

        model.getSelComp = function(compId) {            
            var records = $filter("filter")(model.getAllCompanies(), {
                            companyRealPageId: compId
                        }, true);

            return records;
        };

        model.getAllCompanyOptions = function() {
            var data = model.getAllCompanies();
            var dataOptions = [];

            if (data !== undefined) {
                data.forEach(function(item) {
                    var o = {
                        productName: item.companyName,
                        productVal: item.companyRealPageId
                    };
                    dataOptions.push(o);
                });
            }

            return dataOptions;
        };

        model.setMenuOptions = function() {
            var data = model.getAllCompanyOptions();
            menuConfig
                .setOptions("optionsData", data);
        };

        model.setDefaultComp = function() {            
            if(persona.isReady()){
                model.setSelCompId(persona.getOrgRealPageID());
            }
        };


        return model.init();
    }

    angular
        .module("settings")
        .factory("compSwitchSelectMenuModel", [
            "pubsub",
            "userSessionModel",
            "compSwitchSvc",
            "compMenuConfig",
            "personaDetails",
            "$filter",
            factory
        ]);
})(angular);