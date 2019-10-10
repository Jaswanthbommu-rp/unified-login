//  PLP Login User Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        function ElmsLoginUserModel() {
            var s = this;
            s.init();
        }

        var p = ElmsLoginUserModel.prototype;

        p.init = function () {
            var s = this;
            s.data = {
                userEmailAddress: ""
            };
        };


        // Getters
        p.getUserEmailAddress = function() {
            var s = this;
            return s.data.userEmailAddress;
        };

        // Setters

        p.setUserEmailAddress = function(userEmailAddress) {
            var s = this;
            s.data.userEmailAddress = userEmailAddress;
        };

        // Actions

        p.flushData = function () {
            var s = this;
            s.data = {
                userEmailAddress: ""
            };
            return s;
        };

        // Destroy

        p.reset = function () {
            var s = this;
            s.data = {};
        };

        return new ElmsLoginUserModel();
    }

    angular
        .module("settings")
        .factory("elmsLoginUserModel", [
            factory
        ]);
})(angular);
