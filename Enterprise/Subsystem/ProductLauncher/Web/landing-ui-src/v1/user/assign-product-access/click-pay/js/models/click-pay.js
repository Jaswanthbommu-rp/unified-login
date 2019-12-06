//  Click Pay Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function ClickPayProductAccessModel() {
            var s = this;
            s.init();
        }

        var p = ClickPayProductAccessModel.prototype;

        p.init = function () {
            var s = this;
            s.active = false;
            s.changed = false;            
            s.data = {
                productId: 48,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {                   
                    organizationRoleList: []
                }
            };
            s._data = angular.copy(s.data);
        };

        p.setActive = function (bool) {
            var s = this;
            s.active = bool;
            return s;
        };

        p.isActive = function () {
            var s = this;
            return s.active;
        };

        p.setNewUser = function (val) {
            var s = this;
            s.newuser = val;
        };

        p.setChanged = function () {
            var s = this;
            s.changed = true;
            return s;
        };

        p.hasChanged = function () {
            var s = this;
            return s.changed;
        };

        p.setOrgProperties = function (propertiesData) {
            var s = this;
            s.orgproperties = propertiesData;
        };

        p.getOrgProperties = function () {
            var s = this;
            return s.orgproperties ;
        };

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.getProperties = function () {
            var s = this;
            return s.properties ;
        };
       
        p.setCompanies = function (compData) {
            var s = this;
            s.comps = compData;
        };

        p.getCompanies = function (compData) {
            var s = this;
            return s.comps ;
        };

        p.setOrigCompanies = function (compData) {
            var s = this;
            s.orgcomps = compData;
        };

        p.getOrigCompanies = function (compData) {
            var s = this;
            return s.orgcomps ;
        };

        p.setLlc = function (llcData) {
            var s = this;
            s.llc = llcData;
        };

        p.getLlc = function (llcData) {
            var s = this;
            return s.llc ;
        };

        p.setOrigLlc = function (llcData) {
            var s = this;
            s.orgllc = llcData;
        };

        p.getOrgiLlc = function (llcData) {
            var s = this;
            return s.orgllc ;
        };

         p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.getRoles = function () {
            var s = this;
            return s.roles ;
        };

        p.getData = function () {
            var s = this,
                hasComps = false,
                hasProperties = false,
                hasLlcList = false,
                hasOrgnizationRoleList = false,
                noRoleAssigned = true;

            s.data.inputJson.organizationRoleList = [];  

            if (s.comps && s.comps.length) {                
                s.comps.forEach(function (comp) {                    
                    s.data.inputJson.organizationRoleList.push(comp);                                            
                });
            }

            if (s.properties && s.properties.length) {                               
                s.properties.forEach(function (prop) {
                    s.data.inputJson.organizationRoleList.push(prop);                        
                });                               
            }

            if (s.llc && s.llc.length) {
                s.llc.forEach(function (llc) {
                    s.data.inputJson.organizationRoleList.push(llc);                   
                });                
            }

            hasOrgnizationRoleList = s.data.inputJson.organizationRoleList.length > 0;

            
             s.roles.forEach(function (item) {                
                if(item.orgsAssigned > 0){
                    noRoleAssigned = false;
                }
            });
           

            if ( ((!hasOrgnizationRoleList && !s.newuser) || (hasOrgnizationRoleList && s.newuser) || (hasOrgnizationRoleList && !s.newuser) )  && !noRoleAssigned) {  
                return s.data;
            }

            return null;
        };


        p.reset = function () {
            var s = this;
            s.active = false;
            s.changed = false;            
            s.data = angular.copy(s._data);
        };

        return new ClickPayProductAccessModel();
    }

    angular
        .module("settings")
        .factory("clickPayProductAccessModel", [factory]);
})(angular);