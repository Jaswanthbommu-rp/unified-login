//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ARolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, ADataModel, userDetailsModel, security, switchModel, pubsub) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.rolesGrid = rolesGrid;
            vm.rolesError = $filter("productPanelText")("panelError.generic");
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
            vm.allRolesWatch = pubsub.subscribe("Acct.allRolesChange", vm.clearGridSelections);
            vm.gridAllWatch = rolesGrid.subscribe("selectAll", vm.selectAllRoles);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
        };

        vm.isActive = function () {
            return ADataModel.isActive();
        };

        vm.clearGridSelections = function () {
             //clear selections, if theres any
             vm.rolesGrid.selectAll(false);
             vm.rolesGrid.updateSelected();
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

        vm.setData = function (resp) {
            if (resp.records && resp.records.length > 0) {
                switchModel.setRoles(resp.records);
                rolesGrid.busy(false);
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                ADataModel.setRoles(resp.records);
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

        vm.isAccountingAdmin = function () {
            return switchModel.getIsAccountingAdmin();
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageAccountingProductAccess;
        };

        vm.selectAllRoles = function (val) {
            ADataModel.setAllRoles(vm.dataReq.records, val);
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridAllWatch();
            vm.allRolesWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            rolesGrid.destroy();
            rolesGridTransform.destroy();
            gridPagination.destroy();
            rolesGrid = undefined;
            rolesGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ARolesGridCtrl", [
            "$scope",
            "$filter",
            "ARolesSvc",
            "rpGridModel",
            "ARolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "AccountingDataModel",
            "userDetailsModel",
            "routeSecurity",
            "ASwitchModel",
            "pubsub",
            ARolesGridCtrl
        ]);
})(angular);
