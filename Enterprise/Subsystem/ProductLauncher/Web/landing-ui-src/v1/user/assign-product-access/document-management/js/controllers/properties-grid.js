//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function DMPropertiesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, DocMgmtDataModel, userDetailsModel, pubsub, userStatus, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            roleType = "Site Name",
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
        };

        vm.isActive = function () {
            return DocMgmtDataModel.isActive();
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

                DocMgmtDataModel.setProperties(resp.records);
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
            return !persona.data.hasManageDocumentManagementProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
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
        .controller("DMPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "DMAdditionalDataSvc",
            "rpGridModel",
            "docMgmtPropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "documentManagementDataModel",
            "userDetailsModel",
            "pubsub",
            "userStatusModel",
            "routeSecurity",
            DMPropertiesGridCtrl
        ]);
})(angular);
