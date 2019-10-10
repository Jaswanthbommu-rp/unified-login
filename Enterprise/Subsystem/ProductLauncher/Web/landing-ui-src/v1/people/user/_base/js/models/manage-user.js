
// Manage User Model

(function (angular) {
    "use strict";

    function factory() {
        var data = {},
            model = {};

        model.init = function(userType) {
            data.redirectLink = "/people/users";
            data.userType = userType;

            model.data = data;

            return model;
        };

        model.setState = function(state) {
            data.pageState = state;
        };

        model.getRedirectLink = function() {
            return data.redirectLink;
        };

        model.getState = function() {
            return data.pageState;
        };

        model.reset = function() {
            data = {};
        };       

        return model;
    }

    angular
        .module("appName")
        .factory("manageUserModel", [factory]);

})(angular);