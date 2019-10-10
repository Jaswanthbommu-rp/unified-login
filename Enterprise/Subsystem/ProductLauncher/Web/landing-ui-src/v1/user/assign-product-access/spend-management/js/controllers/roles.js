//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function SMRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, SMDataModel, userDetailsModel, security) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.rolesGrid = rolesGrid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            rolesGridTransform.watch(rolesGrid);
            rolesGrid.setConfig(gridConfig);
            gridPagination.setGrid(rolesGrid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
            vm.updateWatch = pubsub.subscribe("cc.roles-radio", vm.updateRecords);
        };

        vm.isActive = function () {
            return SMDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                rolesGrid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.updateRecords = function (record) {
            vm.dataReq.records.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.setData = function (resp) {
            rolesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;
                    });
                }

                SMDataModel.setRoles(resp.records);
            }
            if (resp.isError) {
                vm.isDataError = true;
                if (resp.errorReason !== "") {
                    vm.dataErrorReason = resp.errorReason;
                }
                else {
                    vm.dataErrorReason = genericDataErrorReason;
                }
            }
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageSpendManagementProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.updateWatch();
            vm.personaWatch();
            rolesGrid.destroy();
            gridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            rolesGridTransform.destroy();

            vm = undefined;
            $scope = undefined;
            rolesGrid = undefined;
            gridPagination = undefined;
            rolesGridTransform = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("SMRolesGridCtrl", [
            "$scope",
            "$filter",
            "SMRolesSvc",
            "rpGridModel",
            "SMRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "spendManagementDataModel",
            "userDetailsModel",
            "routeSecurity",
            SMRolesGridCtrl
        ]);
})(angular);
