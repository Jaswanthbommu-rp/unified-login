//  Accounting Switch Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        model.init = function() {
           
            return  model.refresh();
        };

        model.refresh = function () {
            model.accessToSiteSpendMgmtOnly = false;
            model.allowAccessToCurrentFutureProp = false;
            model.accountingAdmin = false;
            model.siteSpendManagementAssignedToCompany = false;
            model.companies = [];
            model.properties = [];
            model.roles = [];
            return model;
        };


        model.setHasAccessToSiteSpendMgmtOnly = function (bool) {            
            model.accessToSiteSpendMgmtOnly = bool;        
        };

        model.setHasAccessToCurrentFutureProp = function (bool) {            
            model.allowAccessToCurrentFutureProp = bool;
        };

        model.setIsAccountingAdmin = function (bool) {            
            model.accountingAdmin = bool;            
        };

        model.setIsSiteSpendManagementAssignedToCompany = function (bool) {            
            model.siteSpendManagementAssignedToCompany = bool;            
        };

        model.getIsSiteSpendManagementAssignedToCompany = function () {                    
            return model.siteSpendManagementAssignedToCompany;            
        };

        model.getIsAccessToSiteSpendMgmtOnly = function () {                    
            return model.accessToSiteSpendMgmtOnly;            
        };

        model.getAllProperties = function () {            
            return model.allowAccessToCurrentFutureProp;
        };

        model.getIsAccountingAdmin = function () {
            return model.accountingAdmin;
        };

        model.getAllRoles = function () {            
            return model.accountingAdmin;
        };

        model.setCompanies = function (companiesData) {            
            model.companies = companiesData;
        };

        model.setProperties = function (propertiesData) {            
            return model.properties ;
        };

        model.getCompanies = function (companiesData) {            
            return model.companies ;
        };

        model.getProperties = function (propertiesData) {            
            model.properties = propertiesData;
        };

        model.setRoles = function (roles) {            
             model.roles = roles ;
        };

        model.getRoles = function (roles) {            
            return model.roles ;
        };

        model.reset = function () {
        	model = {};
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("ASwitchModel", [
        	factory
        ]);
})(angular);
