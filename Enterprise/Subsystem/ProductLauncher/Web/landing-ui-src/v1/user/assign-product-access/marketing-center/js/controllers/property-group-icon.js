//  marketing Center Property Group Icon Controller

(function (angular, undefined) {
    "use strict";

    function MCPropertyGroupIconCtrl($scope, aside, detailsModel) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showModal = function (name) {
            detailsModel.setName(name);
        	aside.show();
        };

        vm.destroy = function () {
            vm.destWatch();
            aside = undefined;
            detailsModel = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("MCPropertyGroupIconCtrl", [
            "$scope", 
            "MCPropertyGroupDetailsAside", 
            "MCPropertyGroupDetailsModel", 
            MCPropertyGroupIconCtrl]);
})(angular);
