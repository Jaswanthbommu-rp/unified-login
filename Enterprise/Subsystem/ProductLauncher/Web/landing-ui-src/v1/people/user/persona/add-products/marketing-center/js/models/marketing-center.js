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
            s.data = {
            	productId: 9, //TODO: Enum api for products instead of hard coded
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

	            s.properties.forEach(function (prop) {
	                if (prop.isAssigned) {
	                    s.data.inputJson.propertyList.push(prop.id);
	                }
	            });
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

        return new MCData();
    }

    angular
        .module("settings")
        .factory("MarketingCenterDataModel", [factory]);
})(angular);
