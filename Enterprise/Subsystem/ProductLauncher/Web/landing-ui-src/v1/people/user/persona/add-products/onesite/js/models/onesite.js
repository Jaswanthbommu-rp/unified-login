//  OSData Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function OSData() {
            var s = this;
            s.init();
        }

        var p = OSData.prototype;

        p.init = function () {
            var s = this;
            s.data = {
                productId: 1, //TODO: Enum api for products instead of hard coded
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

        p.setProperties = function (propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setRoles = function (rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.getData = function () {
            var s = this,
                hasData = false;

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

                if (s.properties[0] !== "all") {
                    s.properties.forEach(function (prop) {
                        if (prop.isAssigned) {
                            s.data.inputJson.propertyList.push(prop.id);
                        }
                    });
                }
                else {
                    s.data.inputJson.propertyList.push("all");
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
            s.data = angular.copy(s._data);
        };

        return new OSData();
    }

    angular
        .module("settings")
        .factory("OnesiteDataModel", [factory]);
})(angular);
