//  VCData Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function UMData() {
            var s = this;
            s.init();
        }

        var p = UMData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;
            s.data = {
                productId: 18, //TODO: Enum api for products instead of hard coded
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    propertyGroupList: [],
                    regionList: []
                }
            };

            s.roles = [];
            s.properties = [];
            s.propertyGroup = [];
            s.regions = [];
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

        p.setAllPropertiesData = function(propertiesData,val){
            var s = this;
            propertiesData.forEach(function (item) {
              item["isAssigned"] = val;
           });
        };

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setPortfolioProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setPropertyGroups = function (propertyGroupData) {
            var s = this;
            s.propertyGroup = propertyGroupData;
        };

        p.setRegions = function (regionsData) {
            var s = this;
            s.regions = regionsData;
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.setAllRoles = function(rolesData, val){
            var s = this;
            rolesData.forEach(function(item){
                item["isAssigned"] = val;
            });
            //s.roles = rolesData;
        };

        p.getData = function () {
            var s = this,
                hasRoles = false,
                hasProperties = false,
                hasPropertyGroups = false,
                hasRegions = false;

            if (s.regions && s.regions.length) {
                s.data.inputJson.regionList = [];

                s.regions.forEach(function (region) {
                    if (region.isAssigned) {
                        s.data.inputJson.regionList.push(region.id);
                    }
                });

                hasRegions = s.data.inputJson.regionList.length > 0;
            }

            if (s.propertyGroup && s.propertyGroup.length) {
                s.data.inputJson.propertyGroupList = [];

                s.propertyGroup.forEach(function (propertyGroup) {
                    if (propertyGroup.isAssigned) {
                        s.data.inputJson.propertyGroupList.push(propertyGroup.id);
                    }
                });

                hasPropertyGroups = s.data.inputJson.propertyGroupList.length > 0;
            }

            if (s.properties && s.properties.length) {
                s.data.inputJson.propertyList = [];

                if (s.properties[0] !== -1) {
                    s.properties.forEach(function (prop) {
                        if (prop.isAssigned) {
                            s.data.inputJson.propertyList.push(prop.id);
                        }
                    });
                }
                else {
                    s.data.inputJson.propertyList.push("all");
                }

                hasProperties = s.data.inputJson.propertyList.length > 0;
            }

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                s.roles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.roleName);
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;
            }

            // if (hasRoles && (hasProperties || hasPropertyGroups || hasRegions)) {
            if (hasProperties || hasPropertyGroups || hasRegions) {
                return s.data;
            }

            return null;
        };

        p.reset = function () {
            var s = this;

            s.changed = false;
            s.active = false;
            s.properties = [];
            s.propertyGroup = [];
            s.regions = [];
            s.roles = [];
            s.data = angular.copy(s._data);
        };

        return new UMData();
    }

    angular
        .module("settings")
        .factory("utilityManagementDataModel", [factory]);
})(angular);
