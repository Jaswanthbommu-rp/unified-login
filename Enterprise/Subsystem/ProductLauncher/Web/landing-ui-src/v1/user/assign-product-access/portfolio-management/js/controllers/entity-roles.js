//  Grid Edit Role Controller

(function (angular, undefined) {
    "use strict";

    var logged;

    function PortfolioManagementEntityRoleCtrl($scope, $filter, model, sync, pmDataModel) {
        var vm = this;

        vm.init = function () {
            vm.recordData = $scope.record;
            vm.model = vm.setRoleModel(vm.recordData.id);
            vm.readyWatch = $scope.$watch(vm.isReady, vm.setData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isReady = function () {
            return pmDataModel.isReady();
        };

        vm.setRoleModel = function (entityId) {
            return model(sync.getTranslatedRoleList(entityId));
        };

        vm.setData = function () {
            var entityData = sync.selectedRoleSync(vm.recordData.id, vm.model.data.groups[0].options);
            pmDataModel.setEntityRoles(entityData.roleList);
            vm.readyWatch();
        };

        vm.hasSelections = function (list) {
            var count = 0;

            list.forEach(function (item) {
                if (item.value) {
                    count++;
                }
            });

            return count !== 0;
        };

        vm.destroy = function () {
            vm.model.destroy();
            vm.destWatch();
            vm.changeWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("PortfolioManagementEntityRoleCtrl", [
            "$scope",
            "$filter",
            "portfolioManagementEntityRolesDropdownModel",
            "pmSyncManager",
            "portfolioManagementDataModel",
            PortfolioManagementEntityRoleCtrl
        ]);
})(angular);
