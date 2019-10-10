//  User Status Model

(function (angular, undefined) {
    "use strict";

    function factory(eventStream) {
        function AOStatusModel() {
            var s = this;
            s.init();
        }

        var p = AOStatusModel.prototype;

        p.init = function () {
            var s = this;
            s.BICompanyId = 0;
            s.IACompanyId = 0;
            s.PACompanyId = 0;
            s.POCompanyId = 0;
            s.AOBenchmarkProductAccess = false;
            s.updateBI = eventStream();
            s.updateIA = eventStream();
            s.updatePO = eventStream();
            s.updatePA = eventStream();
            s.updateBM = eventStream();
        };

        // Setters

        p.setBICompanyId = function (id) {
            var s = this;
            s.BICompanyId = id;
            s.updateBI.publish(id);
            return s;
        };

         p.setBenchmarkProductAccess = function (bool) {
            var s = this;
            s.AOBenchmarkProductAccess = bool;
            s.updateBM.publish(s.AOBenchmarkProductAccess);
            return s;
        };


        p.setIACompanyId = function (id) {
            var s = this;
            s.IACompanyId = id;
            s.updateIA.publish(id);
            return s;
        };

        p.setPACompanyId = function (id) {
            var s = this;
            s.PACompanyId = id;
            s.updatePA.publish(id);
            return s;
        };

        p.setPOCompanyId = function (id) {
            var s = this;
            s.POCompanyId = id;
            s.updatePO.publish(id);
            return s;
        };
        // Actions

        p.subscribeBI = function (callback) {
            var s = this;
            return s.updateBI.subscribe(callback);
        };

        p.subscribeBM = function (callback) {
            var s = this;
            return s.updateBM.subscribe(callback);
        };

         p.subscribeIA = function (callback) {
            var s = this;
            return s.updateIA.subscribe(callback);
        };

         p.subscribePA = function (callback) {
            var s = this;
            return s.updatePA.subscribe(callback);
        };

         p.subscribePO = function (callback) {
            var s = this;
            return s.updatePO.subscribe(callback);
        };
        // Assertions

        p.isBIReady = function () {
            var s = this;
            return s.BICompanyId !== 0;
        };

        p.isIAReady = function () {
            var s = this;
            return s.IACompanyId !== 0;
        };

        p.isPAReady = function () {
            var s = this;
            return s.PACompanyId !== 0;
        };

        p.isPOReady = function () {
            var s = this;
            return s.POCompanyId !== 0;
        };

        // Reset

        p.reset = function () {
            var s = this;
            s.BICompanyId = 0;
            s.IACompanyId = 0;
            s.PACompanyId = 0;
            s.POCompanyId = 0;
            s.AOBenchmarkProductAccess = false;
            return s;
        };

        return new AOStatusModel();
    }

    angular
        .module("settings")
        .factory("aOStatusModel", ["eventStream", factory]);
})(angular);
