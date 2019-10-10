//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function MCRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, MCDataModel, userDetailsModel, security) {
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

            vm.updateWatch = pubsub.subscribe("mc.roles-radio", vm.updateRecords);
        };

        vm.isActive = function () {
            return MCDataModel.isActive();
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
            if (resp.records && resp.records.length > 0) {
                rolesGrid.busy(false);
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;
                    });
                }

                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });
                MCDataModel.setRoles(resp.records);
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
            return !persona.data.hasManageMarketingCenterProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.updateWatch();
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
        .controller("MCRolesGridCtrl", [
            "$scope",
            "$filter",
            "MCRolesSvc",
            "rpGridModel",
            "MCRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "MarketingCenterDataModel",
            "userDetailsModel",
            "routeSecurity",
            MCRolesGridCtrl
        ]);
})(angular);
