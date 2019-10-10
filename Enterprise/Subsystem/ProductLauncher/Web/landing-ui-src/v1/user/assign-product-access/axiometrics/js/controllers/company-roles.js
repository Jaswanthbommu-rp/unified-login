//  DropDownRole Controller

(function (angular, undefined) {
    "use strict";

    var logged;

    function AXMCompanyRoleCtrl($scope, $filter, model, sync, AXMDataModel) {
        var vm = this;

        vm.init = function () {
            vm.recordData = $scope.$parent.$parent.record;
            vm.model = vm.setRoleModel(vm.recordData.companyId);
            vm.changeWatch = vm.model.dropdown.subscribe("change", vm.onChange);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.onChange = function (data) {
            var options = [];
            var selectedOptionsData = vm.model.onChange(data);
            options = selectedOptionsData.selectedOptions.changed;

            var companyData = sync.selectedRoleSync(vm.recordData.companyId, data.option);
            AXMDataModel.setCompanyRoles(companyData.roleList);

        };

        vm.setRoleModel = function (companyId) {
            return model(sync.getTranslatedRoleList(companyId));
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
        .controller("AXMCompanyRoleCtrl", [
            "$scope",
            "$filter",
            "axmRolesDropdownModel",
            "axmSyncManager",
            "axmDataModel",
            AXMCompanyRoleCtrl
        ]);
})(angular);
