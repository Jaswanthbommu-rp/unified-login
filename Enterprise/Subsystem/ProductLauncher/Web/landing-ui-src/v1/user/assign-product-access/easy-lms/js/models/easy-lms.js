//  Coming Soon Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function EasyLMSProductAccessModel() {
            var s = this;
            s.init();
        }

        var p = EasyLMSProductAccessModel.prototype;

        p.init = function () {
            var s = this;
            s.active = false;
            s.changed = false;
            s.data = {
                productId: 36,
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                }
            };
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
        };

        return new EasyLMSProductAccessModel();
    }

    angular
        .module("settings")
        .factory("easyLMSProductAccessModel", [factory]);
})(angular);
