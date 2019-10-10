// userMgmt Data Model

(function(angular, undefined) {
    "use strict";

    function factory() {
        function OCData() {
            var s = this;
            s.init();
        }

        var p = OCData.prototype;

        p.init = function() {
            var s = this;
            s.changed = false;
            s.active = false;
            s.data = {
                productId: 3, //TODO: Enum api for products instead of hard coded
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: []
                }
            };

            s.roles = [];
            s._data = angular.copy(s.data);
        };

        p.setChanged = function() {
            var s = this;
            s.changed = true;
            return s;
        };

        p.hasChanged = function() {
            var s = this;
            return s.changed;
        };

        p.setActive = function(bool) {
            var s = this;
            s.active = bool;
            return s;
        };

        p.isActive = function() {
            var s = this;
            return s.active;
        };


        p.setRoles = function(rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.getData = function() {
            var s = this,
                hasRoles = false;

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                s.roles.forEach(function(role) {
                    if (role.isAssigned) {
                        s.data.inputJson.roleList.push(role.id);
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;
            }

            if (hasRoles) {
                return s.data;
            }

            return null;
        };

        p.reset = function() {
            var s = this;

            s.roles = [];
            s.changed = false;
            s.active = false;
            s.data = angular.copy(s._data);
        };

        return new OCData();
    }

    angular
        .module("settings")
        .factory("userMgmtDataModel", [factory]);
})(angular);