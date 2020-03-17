//  Info Icon Controller

(function(angular, undefined) {
    "use strict";

    function InfoIconCtrl($scope, aside, asideModel) {
        var vm = this;

        vm.init = function() {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showAside = function(record) {
            asideModel.setName(record.name);
            asideModel.setRoleID(record.id);
            aside.show();
        };

        vm.destroy = function() {
            vm.destWatch();
            aside = undefined;
            asideModel = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("InfoIconCtrl", [
            "$scope",
            "asideModal",
            "asideModel",
            InfoIconCtrl
        ]);
})(angular);