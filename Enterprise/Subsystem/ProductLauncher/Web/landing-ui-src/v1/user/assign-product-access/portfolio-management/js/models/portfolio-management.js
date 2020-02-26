//  Portfolio Management Data Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function PortfolioManagementData() {
            var s = this;
            s.init();
        }

        var p = PortfolioManagementData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;
            s.data = {
                "records": []
            };

            s.propertyRoleListData = {
                "PropertyId": "",
                "Roles": []
            };

            s.data = {
                productId: 44,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    propertyGroupList: [],
                    PropertyRoleList: [],
                    isAssigned: false,
                    companyId: ""
                }
            };


            s.roles = [];
            // s.roleList = [];
            s.properties = [];
            s.propertyGroup = [];
            s.propertyGroupList = [];
            s.PropertyRoleList = [];
            s.ready = false;
            s.entities = [];
            s.entityRoles = [];
            s._propertyRoleListData = angular.copy(s.propertyRoleListData);
            s._data = angular.copy(s.data);
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

        p.setActive = function (bool) {
            var s = this;
            s.active = bool;
            return s;
        };

        p.isActive = function () {
            var s = this;
            return s.active;
        };

        p.isReady = function () {
            var s = this;
            return s.ready;
        };


        p.isEntityDataExists = function () {
            var s = this;
            return s.entities.length > 0;
        };

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setPropertyGroups = function (propertyGroupData) {
            var s = this;
            s.propertyGroup = propertyGroupData;
        };

        p.setEntities = function (entitiesData) {
            var s = this;            
            s.ready = true;
            s.entities = entitiesData;
            s.multiCompany = s.entities.length > 1;

        };

        p.setEntityRoles = function (data) {
            var s = this;             
            s.ready = true;
            s.entityRoles = data;
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.getEntities = function () {
            var s = this;
            return s.entities;
        };

        p.getData = function () {
            var s = this,
                hasRoles = false,
                hasPropertyRoles = false;
                var propIds = [];
            s.data = angular.copy(s._data);

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                s.roles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.id);
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;
            }
           
            s.data.inputJson.PropertyRoleList = [];

            s.getEntities().forEach(function(role){
                if(role.isAssigned){
                    role.propertiesList.forEach(function(prop){
                        if(prop.isAssigned){
                            s.propertyRoleListData = angular.copy(s._propertyRoleListData);
                            s.propertyRoleListData.Roles = [];

                            s.propertyRoleListData.PropertyId = prop.id;
                            s.propertyRoleListData.Roles.push(role.id);

                            s.data.inputJson.PropertyRoleList.push(s.propertyRoleListData);
                        }
                    });
                }
            });

            hasPropertyRoles = s.data.inputJson.PropertyRoleList.length > 0;

            //if (s.entityRoles && s.entityRoles.length) {
                //s.data.inputJson.PropertyRoleList = [];

                // s.entityRoles.forEach(function (entity) {
                //     s.propertyRoleListData = angular.copy(s._propertyRoleListData);
                //     s.propertyRoleListData.Roles = [];

                //     if (entity.isAssigned) {
                //         entity.roleList.forEach(function (role) {
                //             if (role.isAssigned) {
                //                 s.propertyRoleListData.Roles.push(role.id);
                //             }
                //         });

                //         if (s.propertyRoleListData.Roles && s.propertyRoleListData.Roles.length) {
                //             s.propertyRoleListData.PropertyId = entity.id;
                //             s.data.inputJson.PropertyRoleList.push(s.propertyRoleListData);
                //             propIds.push(entity.id);
                //         }
                //     }
                // });
                
               
                //   s.getEntities().forEach(function (entity) {                  
                //         if(propIds.indexOf(entity.id, 0) === -1 ){
                //             var bAssigned = false;
                //             s.propertyRoleListData = angular.copy(s._propertyRoleListData);
                //             s.propertyRoleListData.Roles = [];
                //             entity.roleList.forEach(function (role) {
                //                 if(role.isAssigned === true){                                   
                //                     s.propertyRoleListData.Roles.push(role.id);  
                //                     bAssigned = true;                                    
                //                 }
                //             });
                //              if(bAssigned){
                //                 s.propertyRoleListData.PropertyId = entity.id;                                
                //                 s.data.inputJson.PropertyRoleList.push(s.propertyRoleListData);
                //             }
                //          }
                      
                // });

                 
                 //hasPropertyRoles = s.data.inputJson.PropertyRoleList.length > 0;
            //}

            if (hasPropertyRoles) {
                s.data.inputJson.isAssigned = true;
                return s.data;
            }

            return null;
        };

        p.reset = function () {
            var s = this;

            s.changed = false;
            s.active = false;
            s.ready = false;
            s.properties = [];
            s.propertyGroup = [];
            s.roleList = [];
            s.propertyGroupList = [];
            s.PropertyRoleList = [];
            s.entities = [];
            s.roles = [];
            s.entityRoles = [];
            s.pmdata = angular.copy(s._pmdata);
            s.data = angular.copy(s._data);
        };

        return new PortfolioManagementData();
    }

    angular
        .module("settings")
        .factory("portfolioManagementDataModel", [factory]);
})(angular);
