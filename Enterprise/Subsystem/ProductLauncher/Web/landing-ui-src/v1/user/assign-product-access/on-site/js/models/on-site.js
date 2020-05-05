//  OnSiteData Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function OnSiteData() {
            var s = this;
            s.init();
        }

        var p = OnSiteData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;
            s.data = {
                productId: 23,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    regionList: [],
                    roleList: [],
                    propertyList: []
                }
            };

            s.propertyGroups = [];
            s.roles = [];
            s.properties = [];
            s._data = angular.copy(s.data);
        };

        p.getRoles = function () {
            var s = this;
            return s.roles;
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

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setPropertyGroups = function (propertyGroupData) {
            var s = this;
            s.propertyGroups = propertyGroupData;
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.setAllPropertiesData = function (propertiesData,val) {
            var s = this;
             propertiesData.forEach(function (item) {
               item["isAssigned"] = val;
            });
        };

        p.setAllPropertyGroupData = function (propertyGroupData,val) {
            var s = this;
            propertyGroupData.forEach(function (item) {
               item["isAssigned"] = val;
            });

        };

        p.getData = function () {
            var s = this,
                hasRoles = false;

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                s.roles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.id);
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;
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
                    s.data.inputJson.propertyList.push(-1);
                }
            }

            if (s.propertyGroups && s.propertyGroups.length) {
                s.data.inputJson.regionList = [];

                s.propertyGroups.forEach(function (group) {
                    if (group.isAssigned) {
                        s.data.inputJson.regionList.push(group.id);
                    }
                });
            }

            if (hasRoles) {
                return s.data;
            }

            return null;
        };

        p.reset = function () {
            var s = this;

            s.propertyGroups = [];
            s.roles = [];
            s.properties = [];
            s.active = false;
            s.changed = false;
            s.data = angular.copy(s._data);
        };

        return new OnSiteData();
    }

    angular
        .module("settings")
        .factory("onSiteDataModel", [factory]);
})(angular);
