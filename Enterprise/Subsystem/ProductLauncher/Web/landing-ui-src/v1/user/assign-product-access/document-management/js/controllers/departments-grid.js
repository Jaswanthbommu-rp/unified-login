//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function DMDepartmentsGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, DocMgmtDataModel, userDetailsModel, pubsub, userStatus, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            roleType = "Department";

        vm.init = function () {
            vm.grid = grid;
            vm.dataErrorReason = $filter("productPanelText")("panelError.generic");
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
        };

        vm.isActive = function () {
            return DocMgmtDataModel.isActive();
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageDocumentManagementProductAccess;
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                grid.busy(true);
                var userPersonaId = userDetailsModel.getPersonaId();
                var params = {
                    userPersonaId: userPersonaId,
                    editorPersonaId: persona.getId(),
                    roleId: DocMgmtDataModel.getRoleID(roleType)
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                DocMgmtDataModel.setDepartments(resp.records);
            }
            if (resp.isError) {
                vm.isDataError = true;
                if (resp.errorReason !== "") {
                    vm.dataErrorReason = resp.errorReason;
                }
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            grid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            vm = undefined;
            grid = undefined;
            $scope = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("DMDepartmentsGridCtrl", [
            "$scope",
            "$filter",
            "DMAdditionalDataSvc",
            "rpGridModel",
            "docMgmtDepartmentsGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "documentManagementDataModel",
            "userDetailsModel",
            "pubsub",
            "userStatusModel",
            "routeSecurity",
            DMDepartmentsGridCtrl
        ]);
})(angular);
