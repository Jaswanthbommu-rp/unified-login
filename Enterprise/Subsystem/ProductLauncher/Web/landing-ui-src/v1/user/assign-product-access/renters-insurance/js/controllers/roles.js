//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function RentersInsuranceRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, RentInsDataModel, userDetailsModel, tabsModel, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);
            gridPagination.setGrid(grid);
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

            vm.updateWatch = pubsub.subscribe("ri.roles-radio", vm.updateRecords);
        };

        vm.isActive = function () {
            return RentInsDataModel.isActive();
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageRentersInsuranceProductAccess;
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                grid.busy(true);
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
            grid.busy(false);
            resp.data = vm.sortData(resp);
            if (resp.data && resp.data.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.data.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;
                    });
                }

                gridPagination.setData(resp.data).goToPage({
                    number: 0
                });

                RentInsDataModel.setRoles(resp.data);
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

        vm.sortData = function (resp) {
            return $filter("orderBy")(resp.data, "name");
        };

        vm.updateRecords = function (record) {
            vm.dataReq.data.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.updateWatch();
            grid.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            gridTransform.destroy();
            gridPagination.destroy();
            gridTransform = undefined;
            gridPagination = undefined;
            grid = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("RentersInsuranceRolesGridCtrl", [
            "$scope",
            "$filter",
            "RentersInsuranceRolesSvc",
            "rpGridModel",
            "rentersInsuranceRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "rentersInsuranceDataModel",
            "userDetailsModel",
            "RentersInsuranceTabsModel",
            "routeSecurity",
            RentersInsuranceRolesGridCtrl
        ]);
})(angular);
