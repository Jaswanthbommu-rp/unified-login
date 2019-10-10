//  Deposit Alt Model

(function (angular, undefined) {
    "use strict";

    function factory(tabs, pubsub) {
        function DepositAlternativeProductAccessModel() {
            var s = this;
            s.init();
        }

        var p = DepositAlternativeProductAccessModel.prototype;

        p.init = function () {
            var s = this;
            s.active = false;
            s.changed = false;
            s.allProperties = false;
            s.data = {
                productId: 47,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {                   
                    propertyGroupList: [],
                    propertyList: [],
                    roleList: [],
                    canReceiveMonthlyReport: true
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

        p.setChanged = function () {
            var s = this;
            s.changed = true;
            return s;
        };

        p.hasChanged = function () {
            var s = this;
            return s.changed;
        };

        p.clearProperties = function () {
            var s = this;
            if(s.properties !== undefined){
                s.properties.forEach(function (item) {
                    item.isAssigned = false;
                });
            }

            pubsub.publish("DA.regions");             
            pubsub.publish("DA.areas");    
        };

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.clearAreas = function () {
            var s = this;
            if(s.areas !== undefined){
                s.areas.forEach(function (item) {
                    item.isAssigned = false;
                });
            }

            pubsub.publish("DA.regions"); 
            pubsub.publish("DA.properties");               
        };

        p.setAreas = function (areasData) {
            var s = this;
            s.areas = areasData;
        };

        p.clearRegions = function () {
            var s = this;
            if(s.regions !== undefined){
                s.regions.forEach(function (item) {
                    item.isAssigned = false;
                });
            }

            pubsub.publish("DA.properties");   
            pubsub.publish("DA.areas");    
        };

        p.setRegions = function (regionsData) {            
            var s = this;
            s.regions = regionsData;
        };

        p.setCanReceiveMonthlyReport = function (val) {
            var s = this;
            s.data.inputJson.canReceiveMonthlyReport = val;
        };

        p.setRoles = function (rolesData) {            
            var s = this;
            s.roles = rolesData;
        };

        p.getData = function () {
            var s = this,
                hasRoles = false,
                hasProperties = false,
                roleWithoutPropTabs = false;
               
            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                s.roles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.getId());
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;

                if(s.data.inputJson.roleList[0] === 'collections_agent' || s.data.inputJson.roleList[0] === 'company'  || s.data.inputJson.roleList[0] === 'insurance_provider' || s.data.inputJson.roleList[0] === 'admin'){
                    roleWithoutPropTabs = true;
                }
            }

            tabs.tabsList.forEach(function (tab) {
                
            if (s.properties && s.properties.length && tab.id === 'properties'  ) {
                s.data.inputJson.propertyList = [];
                s.data.inputJson.propertyGroupList = [];
            
                s.properties.forEach(function (prop) {
                    if (prop.isAssigned) {
                        s.data.inputJson.propertyList.push(prop.id);
                    }
                });               

                hasProperties = s.data.inputJson.propertyList.length > 0;
            }

            if (s.areas && s.areas.length && tab.id === 'areas'  ) {
                s.data.inputJson.propertyGroupList = [];
                s.data.inputJson.propertyList = [];
                
                s.areas.forEach(function (area) {
                    if (area.isAssigned) {
                        s.data.inputJson.propertyGroupList.push(area.id);
                    }
                });                

                hasProperties = s.data.inputJson.propertyGroupList.length > 0;
            }

            if (s.regions && s.regions.length && tab.id === 'regions'  ) {
                s.data.inputJson.propertyGroupList = [];
                s.data.inputJson.propertyList = [];
                
                s.regions.forEach(function (reg) {
                    if (reg.isAssigned) {
                        s.data.inputJson.propertyGroupList.push(reg.id);
                    }
                });
                
                hasProperties = s.data.inputJson.propertyGroupList.length > 0;
            }


            });

           
            if (hasRoles && (hasProperties || roleWithoutPropTabs) ) {
                return s.data;
            }

            return null;
        };


        p.reset = function () {
            var s = this;
            s.active = false;
            s.changed = false;
            s.properties = [];
            s.areas = [];
            s.roles = [];
            s.regions = [];
            s.data = angular.copy(s._data);
        };

        return new DepositAlternativeProductAccessModel();
    }

    angular
        .module("settings")
        .factory("depositAlternativeProductAccessModel", ["DepositAlternativeTabsModel","pubsub",factory]);
})(angular);