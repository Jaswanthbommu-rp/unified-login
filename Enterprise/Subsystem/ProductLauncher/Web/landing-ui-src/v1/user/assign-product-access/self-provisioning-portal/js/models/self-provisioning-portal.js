//  Self-Provisioning Portal Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function SelfProvisioningPortalProductAccessModel() {
            var s = this;
            s.init();
        }

        var p = SelfProvisioningPortalProductAccessModel.prototype;

        p.init = function () {
            var s = this;
            s.active = false;
            s.changed = false;
            s.data = {
                productId: 25,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
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

        p.hasChanged = function () {
            var s = this;
            return false;
        };

        p.getData = function () {
            var s = this;
            return s.data;
        };

        p.reset = function () {
            var s = this;
            s.active = false;
            s.changed = false;
            s.data = angular.copy(s._data);
        };

        return new SelfProvisioningPortalProductAccessModel();
    }

    angular
        .module("settings")
        .factory("selfProvisioningPortalProductAccessModel", [factory]);
})(angular);
