//  Grid Edit Role Controller

(function (angular, undefined) {
    "use strict";

    var logged;

    function PACompanyRoleCtrl($scope, $filter, pubsub, model, sync, paDataModel) {
        var vm = this;

        vm.init = function () {
            vm.paDataModel = paDataModel;
            vm.recordData = $scope.record;
            vm.model = vm.setRoleModel(vm.recordData.companyId);
            vm.readyWatch = $scope.$watch(vm.isReady, vm.setData);
            vm.changeWatch = vm.model.dropdown.subscribe("change", vm.onChange);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isReady = function () {
            return paDataModel.isReady();
        };

        vm.onChange = function (data) {
            var options = [],
                companyData;

            if (data.selected.length == 0) {
                companyData = sync.deselectAllcompanyRoles(vm.recordData.companyId);
            }
            else {
                companyData = sync.selectedRoleSync(vm.recordData.companyId, data.selected);
            }

            paDataModel.setCompanyRoles(companyData.roleList);
            paDataModel.setChanged();
        };


        vm.setRoleModel = function (companyId) {
            return model(sync.getTranslatedRoleList(companyId));
        };

        vm.setData = function () {
            var companydata = sync.selectedRoleSync(vm.recordData.companyId, vm.model.data.groups[0].options);
            paDataModel.setCompanyRoles(companydata.roleList);
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
        .controller("PACompanyRoleCtrl", [
            "$scope",
            "$filter",
            "pubsub",
            "performanceAnalyticsRolesDropdownModel",
            "paSyncManager",
            "performanceAnalyticsDataModel",
            PACompanyRoleCtrl
        ]);
})(angular);
