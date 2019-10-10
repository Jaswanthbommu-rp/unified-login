//  Access Not Defined Controller

(function (angular, undefined) {
    "use strict";

    function AccessNotDefinedCtrl($scope, model, productAccess) {
        var vm = this;

        vm.init = function () {
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.destroy = function () {
            model.reset();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AccessNotDefinedCtrl", [
            "$scope",
            "accessNotDefinedModel",
            "productAccessModel",
            AccessNotDefinedCtrl
        ]);
})(angular);
