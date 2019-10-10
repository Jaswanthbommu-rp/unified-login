//  Single User Info Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        model.getLoginName = function () {
            return model.userLoginName;
        };

        model.getRealpageID = function () {
            return model.RealpageID;
        };

        model.setLoginName = function (loginName) {
            model.userLoginName = loginName;
        };

        model.setRealpageID = function (realpageID) {
            model.RealpageID = realpageID;
        };

        model.reset = function () {
            model = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("singleUserInfoModel", [
            factory
        ]);
})(angular);
