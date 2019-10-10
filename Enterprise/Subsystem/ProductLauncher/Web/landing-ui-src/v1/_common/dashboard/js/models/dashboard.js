//  Dashboard Model

(function (angular, undefined) {
    "use strict";

    function factory(eventStream, dashboardSvc) {
        function DashboardModel() {
            var s = this;
            s.init();
        }

        var p = DashboardModel.prototype;

        p.init = function () {
            var s = this;

            s.data = {};

            s.state = {
                busy: false,
                ready: false
            };

            s.update = eventStream();
        };

        // Getters

        p.getData = function () {
            var s = this;
            return s.data;
        };

        // Setters

        p.setData = function (data) {
            var s = this;
            s.data = data;
            return s;
        };

        // Actions

        p.cancelReq = function () {
            var s = this;

            if (s.stateReqObj) {
                s.stateReqObj.$cancelRequest();
                s.stateReqObj = undefined;
            }

            return s;
        };

        p.load = function () {
            var s = this,
                error = s.onError.bind(s),
                success = s.onSuccess.bind(s);

            if (!s.state.ready && !s.state.busy) {
                s.cancelReq();
                s.state.busy = true;
                s.stateReqObj = dashboardSvc.get(success, error);
            }

            return s;
        };

        p.onError = function (data) {
            var s = this;
            s.state.busy = false;
            logw("DashboardModel: Data call failed!", data);
            return s;
        };

        p.onSuccess = function (resp) {
            var s = this;

            if (resp.isError) {
                s.onError(resp);
                return s;
            }

            s.state.ready = true;
            s.state.busy = false;
            s.setData(resp).publish(resp);
        };

        p.publish = function (data) {
            var s = this;
            s.update.publish(data);
            return s;
        };

        p.reload = function () {
            var s = this;
            s.reset().load();
            return s;
        };

        p.subscribe = function () {
            var s = this;
            return s.update.subscribe.apply(s.update, arguments);
        };

        // Assertions

        p.isReady = function () {
            var s = this;
            return s.state.ready;
        };

        p.reset = function () {
            var s = this;
            s.state.ready = false;
            return s;
        };

        return new DashboardModel();
    }

    angular
        .module("settings")
        .factory("dashboardModel", [
            "eventStream",
            "dashboardSvc",
            factory
        ]);
})(angular);
