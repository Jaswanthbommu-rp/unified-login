//  Property Group Info Icon Controller

(function (angular, undefined) {
    "use strict";

    function BIPropertyGroupInfoIconCtrl($scope, aside, groupModel) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showAside = function (record) {
            groupModel.setName(record.groupName);
            groupModel.setPropertyGroupID(record.groupId);
            aside.show();
        };

        vm.destroy = function () {
            vm.destWatch();
            aside = undefined;
            groupModel = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("BIPropertyGroupInfoIconCtrl", [
            "$scope",
            "biPropertiesListAside",
            "biPropertyGroupModel",
            BIPropertyGroupInfoIconCtrl
        ]);
})(angular);
