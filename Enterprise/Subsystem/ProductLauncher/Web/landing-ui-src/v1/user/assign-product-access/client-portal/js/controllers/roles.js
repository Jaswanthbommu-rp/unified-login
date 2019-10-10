//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ClientPortalRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, pubsub, ClientPortalDataModel, userDetailsModel, security) {
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
            pubsub.subscribe("cp.roles-radio", vm.updateRoleRecords);
        };

        vm.isActive = function () {
            return ClientPortalDataModel.isActive();
        };

        vm.loadData = function () {
            rolesGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.updateRoleRecords = function (record) {
            var rolesData = ClientPortalDataModel.getRoles();

            rolesData.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.setData = function (resp) {
            rolesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
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
                ClientPortalDataModel.setRoles(resp.records);
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
            return !persona.data.hasManageClientPortalProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
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
        .controller("ClientPortalRolesGridCtrl", [
            "$scope",
            "$filter",
            "clientPortalRolesSvc",
            "rpGridModel",
            "clientPortalRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "pubsub",
            "clientPortalDataModel",
            "userDetailsModel",
            "routeSecurity",
            ClientPortalRolesGridCtrl
        ]);
})(angular);
