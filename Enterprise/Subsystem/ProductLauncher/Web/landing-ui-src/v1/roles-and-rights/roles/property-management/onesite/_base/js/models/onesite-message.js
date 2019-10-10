//  Onesite Status Message Model

(function(angular, undefined) {
    "use strict";

    function factory($filter) {
        function OnesiteStatusMsgModel() {
            var s = this;
            s.init();
        }

        var p = OnesiteStatusMsgModel.prototype;

        p.init = function() {
            var s = this;

            s.data = {
                msg: "",
                title: "",
                success: true,
                detailErrorMsgs: []
            };

            s.dataCopy = angular.copy(s.data);
        };

        // Setters

        p.setData = function(data) {
            var s = this;

            s.data.success = data.success;
            s.data.msg = data.msg;
            s.data.title = data.title;

            return s;
        };

        // Assertions

        // Reset

        p.reset = function() {
            var s = this;
            s.data = angular.copy(s.dataCopy);
        };

        return new OnesiteStatusMsgModel();
    }

    angular
        .module("settings")
        .factory("onesiteStatusMsgModel", ["$filter", factory]);
})(angular);