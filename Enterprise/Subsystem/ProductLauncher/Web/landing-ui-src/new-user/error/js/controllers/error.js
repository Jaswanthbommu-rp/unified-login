//  Error Controller

(function (angular) {
    "use strict";

    function ErrorCtrl($scope, $filter) {
        var vm = this;

        vm.init = function () {
        	vm.validateCtrlWatch = $scope.$on("$destroy", vm.destroy);
        	vm.errorMsg = $filter("errorText")("system_err_contact_admin");
        };

        vm.destroy = function () {
        	vm.validateCtrlWatch();
        	vm = undefined;
        };

        vm.init();
    }

    angular
        .module("new-user")
        .controller("ErrorCtrl", [
            "$scope",
            "$filter",
            ErrorCtrl
        ]);
})(angular);
