

(function (angular) {
    "use strict";

    function factory() {
        var model = {};
        model.status = false;

        model.getIsBusy = function () {
            return model.status;
        };
        
        model.setIsBusy = function (bool) {
            model.status = bool;
        };
        
        model.reset = function () {
            model = {};
        };

        return model;
    }

    angular
        .module("settings")
        .factory("chkEmailModel", [
            factory
        ]);
})(angular);
