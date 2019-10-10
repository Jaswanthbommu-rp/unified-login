//  ProspectContactCenter Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function PCCData() {
            var s = this;
            s.init();
        }

        var p = PCCData.prototype;

        p.init = function () {
            var s = this;
            s.changed = false;
            s.active = false;
            s.data = {
                productId: 10,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    propertyList: []
                }
            };

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

        p.getData = function () {
            var s = this,
                hasProperties = false;

            if (s.properties && s.properties.length) {
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

                hasProperties = s.data.inputJson.propertyList.length > 0;
            }

            if (hasProperties) {
                return s.data;
            }

            return null;
        };

        p.reset = function () {
            var s = this;

            s.active = false;
            s.changed = false;
            s.properties = [];
            s.data = angular.copy(s._data);
        };

        return new PCCData();
    }

    angular
        .module("settings")
        .factory("prospectContactCenterDataModel", [factory]);
})(angular);
