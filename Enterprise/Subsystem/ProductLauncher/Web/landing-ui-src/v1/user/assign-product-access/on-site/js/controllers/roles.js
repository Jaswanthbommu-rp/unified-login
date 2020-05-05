//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function OnSiteRolesCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, dataModel, userDetailsModel, pubsub, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);
            gridPagination.setGrid(grid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 30
            });

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
            vm.updateWatch = pubsub.subscribe("onsite.roles-radio", vm.updateRecords);
        };

        vm.isActive = function () {
            return dataModel.isActive();
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
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;
                    });
                }

                var roleData = vm.sortData(resp);
                gridPagination.setData(roleData).goToPage({
                    number: 0
                });
                dataModel.setRoles(roleData);
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
            return $filter("orderBy")(resp.records, "title");
        };

        vm.updateRecords = function (record) {
            var roleList = dataModel.getRoles();

            roleList.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageOnSiteProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            grid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            grid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("OnSiteRolesCtrl", [
            "$scope",
            "$filter",
            "onSiteRolesSvc",
            "rpGridModel",
            "onSiteRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "onSiteDataModel",
            "userDetailsModel",
            "pubsub",
            "routeSecurity",
            OnSiteRolesCtrl
        ]);
})(angular);
