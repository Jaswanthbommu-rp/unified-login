//  MCData Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function MCData() {
            var s = this;
            s.init();
        }

        var p = MCData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;
            s.data = {
                productId: 9, //TODO: Enum api for products instead of hard coded
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    isAssignedNewPropertyByDefault: false
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
        p.setNewPropertyState = function (bool) {
            var s = this;
            s.data.inputJson.isAssignedNewPropertyByDefault = bool;
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

        p.setAllProperties = function (propertiesData, val) {
            var s = this;
            propertiesData.forEach(function (item) {
                item["isAssigned"] = val;
            });
            //s.properties = propertiesData;
        };

        p.getData = function () {
            var s = this,
                hasRoles = false,
                hasProperties = false;

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

                s.properties.forEach(function (prop) {
                    if (prop.isAssigned) {
                        s.data.inputJson.propertyList.push(prop.id);
                    }
                });

                // if (s.properties[0] !== "all") {
                //     s.properties.forEach(function (prop) {
                //         if (prop.isAssigned) {
                //             s.data.inputJson.propertyList.push(prop.id);
                //         }
                //     });
                // }
                // else {
                //     s.data.inputJson.propertyList.push("all");
                // }

                hasProperties = s.data.inputJson.propertyList.length > 0;
            }

            if (hasRoles && hasProperties) {
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
            s.data = angular.copy(s._data);
        };

        return new MCData();
    }

    angular
        .module("settings")
        .factory("MarketingCenterDataModel", [factory]);
})(angular);
