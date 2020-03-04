//Client Portal Data model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function ProductPanelData() {
            var s = this;
            s.init();
        }

        var p = ProductPanelData.prototype;

        p.init = function () {
            var s = this;
            s.active = false;
            s.roleGridActive = false;
            s.propertyGridActive = false;
            s.changed = false;
            s.data = {
                productId: 14,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: []
                }
            };

            s.roles = [];
            s.properties = [];
            s.isAllProperties = false;
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

        p.setRoleGridActive = function (bool) {
            var s = this;
            s.roleGridActive = bool;
            return s;
        };

        p.isRoleGridActive = function () {
            var s = this;
            return s.roleGridActive;
        };

        p.setPropertyGridActive = function (bool) {
            var s = this;
            s.propertyGridActive = bool;
            return s;
        };

        p.isPropertyGridActive = function () {
            var s = this;
            return s.propertyGridActive;
        };

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };


        p.setAllProperty = function (val) {
            var s = this;
            s.isAllProperties = val;

        };

        p.getRoles = function () {
            var s = this;
            return s.roles;
        };

        p.getProperties = function () {
            var s = this;
            return s.properties;
        };

        p.getData = function () {
            var s = this,
                hasRoleSelected = false,
                hasPropertySelected = false;

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];
                s.roles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.id);
                    }
                });

                hasRoleSelected = s.data.inputJson.roleList.length > 0;
            }

            if (s.properties && s.properties.length) {
                s.data.inputJson.propertyList = [];

                if (s.isAllProperties) {
                    s.data.inputJson.propertyList.push("-1");
                }
                else {
                    s.properties.forEach(function (prop) {
                        if (prop.isAssigned) {
                            s.data.inputJson.propertyList.push(prop.id);
                        }
                    });
                }

                hasPropertySelected = s.data.inputJson.propertyList.length > 0;
            }

            if (hasRoleSelected && hasPropertySelected) {
                return s.data;
            }

            return null;
        };

        p.reset = function () {
            var s = this;

            s.roles = [];
            s.properties = [];
            s.isAllProperties = false;
            s.roleGridActive = false;
            s.propertyGridActive = false;
            s.active = false;
            s.changed = false;
            s.data = angular.copy(s._data);
        };

        return new ProductPanelData();
    }

    angular
        .module("settings")
        .factory("productPanelDataModel", [factory]);
})(angular);
