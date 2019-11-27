//  User Status Model

(function (angular, undefined) {
    "use strict";

    function factory(eventStream) {
        function UserStatusModel() {
            var s = this;
            s.init();
        }

        var p = UserStatusModel.prototype;

        p.init = function () {
            var s = this;
            s.statusId = 0;
            s.loginName = "";
            s.update = eventStream();
        };

        // Setters

        p.setStatusId = function (id) {
            var s = this;
            s.statusId = id;
            s.update.publish(id);
            return s;
        };

        p.setLoginName= function (name) {
            var s = this;
            s.loginName = name;
            return s;
        };

        // Getters
        p.getStatusId = function () {
            var s = this;
            return s.statusId;
        };

        p.getLoginName= function () {
            var s = this;
            return s.loginName;
        };
        // Actions

        p.subscribe = function (callback) {
            var s = this;
            return s.update.subscribe(callback);
        };

        // Assertions

        p.isRegularUser = function () {
            var s = this;
            return s.statusId !== 402;
        };

        p.isRegularUserNoEmail = function () {
            var s = this;
            return s.statusId === 404;
        };

        p.isSuperUser = function () {
            var s = this;
            return s.statusId === 402;
        };

        p.loginNameIsEmail = function () {
            var s = this;
            return s.statusId !== 404;
        };

        p.isExternalUser = function () {
            var s = this;
            return s.statusId === 405;
        };

        // Reset

        p.reset = function () {
            var s = this;
            s.statusId = 0;
            return s;
        };

        return new UserStatusModel();
    }

    angular
        .module("settings")
        .factory("userStatusModel", ["eventStream", factory]);
})(angular);
