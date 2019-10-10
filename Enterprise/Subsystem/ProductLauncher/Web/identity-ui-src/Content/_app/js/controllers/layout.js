//  Layout Controller

(function (angular) {
    "use strict";

    function LayoutCtrl($scope, Model, layoutModel, identitySvc, rpCookie) {
        var vm = this;

        vm.init = function () {
            vm.saveTimezone();

            Model.copyrightDate = vm.getCopyrightYear();
            $scope.model = Model;
            $scope.layout = layoutModel;
            identitySvc.init();
        };

        vm.getCopyrightYear = function() {
            var copyrightDate = new Date();
            return copyrightDate.getFullYear();
        };

        vm.saveTimezone = function() {
            var offsetTz = -(new Date().getTimezoneOffset()/60);
            rpCookie.create("timezone", offsetTz);
        };

        vm.init();
    }

    angular
        .module("identity")
        .controller('LayoutCtrl', [
            "$scope",
            "Model",
            "layoutModel",
            "identitySvc",
            "rpCookie",
            LayoutCtrl
        ]);
})(angular);
