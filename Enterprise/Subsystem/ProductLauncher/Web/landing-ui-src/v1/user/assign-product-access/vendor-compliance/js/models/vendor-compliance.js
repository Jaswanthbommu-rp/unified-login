//  VCData Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function VCData() {
            var s = this;
            s.init();
        }

        var p = VCData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;
            s.data = {
                productId: 16, //TODO: Enum api for products instead of hard coded
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    propertyGroup: [],
                    isInsuranceExpired: false,
                    IsVendorRecommendationChanges: false,
                    isVendorNotLinkedToAnyProperty: false
                }
            };

            s.roles = [];
            s.properties = [];
            s.propertyGroup = [];
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

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.setPropertyGroups = function (propertyGroupData) {
            var s = this;
            s.propertyGroup = propertyGroupData;
        };

        p.setInsuranceExpired = function (val) {
            var s = this;
            s.data.inputJson.isInsuranceExpired = val;
        };

        p.setIsVendorChangeRec = function (val) {
            var s = this;
            s.data.inputJson.IsVendorRecommendationChanges = val;
        };

        p.setIsVendorNotLinkedToAnyProperty = function (val) {
            var s = this;
            s.data.inputJson.isVendorNotLinkedToAnyProperty = val;
        };

        p.getData = function () {
            var s = this,
                hasRoles = false,
                hasProperties = false,
                hasPropertyGroups = false;

            if (s.propertyGroup && s.propertyGroup.length) {
                s.data.inputJson.propertyGroup = [];

                s.propertyGroup.forEach(function (propertyGroup) {
                    if (propertyGroup.isAssigned) {
                        var newGroup = {
                            Id: propertyGroup.propertyGroupId,
                            Type: propertyGroup.accessLevel
                        };
                        s.data.inputJson.propertyGroup.push(newGroup);
                    }
                });

                hasPropertyGroups = s.data.inputJson.propertyGroup.length > 0;
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

                hasProperties = s.data.inputJson.propertyList.length > 0;
            }

            if (hasRoles && (hasProperties || hasPropertyGroups)) {
                return s.data;
            }

            return null;
        };

        p.reset = function () {
            var s = this;

            s.roles = [];
            s.changed = false;
            s.active = false;
            s.properties = [];
            s.propertyGroup = [];
            s.data = angular.copy(s._data);
        };

        return new VCData();
    }

    angular
        .module("settings")
        .factory("vendorComplianceDataModel", [factory]);
})(angular);
