//  Unified Amenities Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function UnifiedAmenitiesProductAccessModel() {
            var s = this;
            s.init();
        }

        var p = UnifiedAmenitiesProductAccessModel.prototype;

        p.init = function () {
            var s = this;
            s.active = false;
            s.changed = false;
            s.allProperties = false;
            s.data = {
                productId: 26,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: []
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

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setAllProperties = function(allProperties) {
            var s = this;
            s.allProperties = allProperties;
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.getData = function () {
            var s = this,
                hasRoles = false,
                hasProperties = false;

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                s.roles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.getId());
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;
            }

            if (s.properties && s.properties.length) {
                s.data.inputJson.propertyList = [];

                if (!s.allProperties) {
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

            if (hasRoles && hasProperties) {
                return s.data;
            }

            return null;
        };


        p.reset = function () {
            var s = this;
            s.active = false;
            s.changed = false;
            s.properties = [];
            s.roles = [];
            s.data = angular.copy(s._data);
        };

        return new UnifiedAmenitiesProductAccessModel();
    }

    angular
        .module("settings")
        .factory("unifiedAmenitiesProductAccessModel", [factory]);
})(angular);