//  Renters Insurnace Data Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function RentersInsuranceData() {
            var s = this;
            s.init();
        }

        var p = RentersInsuranceData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;
            s.tabsReady = false;
            s.data = {
                productId: 15,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: []
                }
            };

            s.roles = [];
            s.properties = [];
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

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };
        p.setAllPropertiesData = function (filteredPropertiesData, val) {
            var s = this;
            filteredPropertiesData.forEach(function (item) {
               item["isAssigned"] = val;
            });
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.getData = function () {
            var s = this,
                hasRoles = false,
                hasProperties = false;

            if (s.properties && s.properties.length) {
                s.data.inputJson.propertyList = [];

                s.properties.forEach(function (prop) {
                    if (prop.isAssigned) {
                        s.data.inputJson.propertyList.push(prop.id);
                    }
                });

                hasProperties = s.data.inputJson.propertyList.length > 0;
            }

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                s.roles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.id);
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;
            }

            if (hasRoles && hasProperties) {
                return s.data;
            }

            return null;
        };

        p.reset = function () {
            var s = this;
            s.roles = [];
            s.active = false;
            s.changed = false;
            s.tabsReady = false;
            s.properties = [];
            s.data = angular.copy(s._data);
        };

        return new RentersInsuranceData();
    }

    angular
        .module("settings")
        .factory("rentersInsuranceDataModel", [factory]);
})(angular);
