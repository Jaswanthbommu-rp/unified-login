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
            s.data = {
                productId: 16, //TODO: Enum api for products instead of hard coded
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    propertyGroup: [],
                    isInsuranceExpired: false,
                    IsVendorRecommendationChanges: false
                }
            };

            s.roles = [];
            s.properties = [];
            s.propertyGroup = [];
            s._data = angular.copy(s.data);
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

        p.getData = function () {
            var s = this,
                hasData = false;

            if (s.propertyGroup.length) {
                hasData = true;

                s.data.inputJson.propertyGroup = [];

                s.propertyGroup.forEach(function (propertyGroup) {
                    if (propertyGroup.isAssigned) {
                        var newGroup = {
                            id: propertyGroup.id,
                            type: propertyGroup.type
                        };
                        s.data.inputJson.propertyGroup.push(newGroup);
                    }
                });
            }

            if (s.roles.length) {
                hasData = true;

                s.data.inputJson.roleList = [];

                s.roles.forEach(function (role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.id);
                    }
                });
            }

            if (s.properties.length) {
                hasData = true;

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

            if (hasData) {
                return s.data;
            }

            return null;
        };

        p.reset = function () {
            var s = this;

            s.roles = [];
            s.properties = [];
            s.propertyGroup = [];
            s.data = angular.copy(s._data);
        };

        return new VCData();
    }

    angular
        .module("settings")
        .factory("VendorComplianceDataModel", [factory]);
})(angular);
