//  Role Info Icon Controller

(function (angular, undefined) {
    "use strict";

    function UARoleInfoIconCtrl($scope, aside, rightsModel) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showAside = function (record) {
            rightsModel.setName(record.name);
            rightsModel.setRoleID(record.id);
            aside.show();
        };

        vm.destroy = function () {
            vm.destWatch();
            aside = undefined;
            rightsModel = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UARoleInfoIconCtrl", [
            "$scope",
            "uaRightsListAside",
            "unifiedAmenitiesRightsModel",
            UARoleInfoIconCtrl
        ]);
})(angular);
