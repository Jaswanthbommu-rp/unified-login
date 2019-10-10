//  Grid Edit Role Controller

(function (angular, undefined) {
    "use strict";

    var logged;

    function PACompanyBMRoleCtrl($scope, $filter, pubsub, model, sync, paDataModel) {
        var vm = this;

        vm.init = function () {
            vm.paDataModel = paDataModel;
            vm.recordData = $scope.record;
            vm.benchMarkAccess = false;

            vm.dropDowndata = {
                title: "",
                options: []
            };
            vm.model = model(vm.dropDowndata);

            vm.changeWatch = vm.model.dropdown.subscribe("change", vm.onChange);
            vm.activeWatch = $scope.$watch(vm.isBMReady, vm.setBMRoleModel);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isBMReady = function () {
            return paDataModel.isReady() && paDataModel.isBenchmarkDataReady();
        };

        vm.onChange = function (data) {
            var options = [],
                companyData;

            if (data.selected.length == 0) {
                if (data.option) {
                    options.push(data.option);
                }
                companyData = sync.selectedBMRoleSync(vm.recordData.companyId, options);
            }
            else {
                companyData = sync.selectedBMRoleSync(vm.recordData.companyId, data.selected);
            }

            paDataModel.setCompanyBMRoles(companyData.bmRoleList);
            paDataModel.setChanged();
        };


        vm.setBMRoleModel = function (value) {
            if (value) {
                vm.benchMarkAccess = value;
                vm.model.setData(sync.getTranslatedBMRoleList(vm.recordData.companyId));
                vm.setBMRoleData();
                vm.activeWatch();
            }

            return vm;
        };

        vm.setBMRoleData = function () {
            var companydata = sync.selectedBMRoleSync(vm.recordData.companyId, vm.model.data.groups[1].options);
            paDataModel.setCompanyBMRoles(companydata.bmRoleList);
        };

        vm.hasBMAccess = function () {
            return vm.benchMarkAccess;
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
        .controller("PACompanyBMRoleCtrl", [
            "$scope",
            "$filter",
            "pubsub",
            "performanceAnalyticsRolesDropdownModel",
            "paSyncManager",
            "performanceAnalyticsDataModel",
            PACompanyBMRoleCtrl
        ]);
})(angular);
