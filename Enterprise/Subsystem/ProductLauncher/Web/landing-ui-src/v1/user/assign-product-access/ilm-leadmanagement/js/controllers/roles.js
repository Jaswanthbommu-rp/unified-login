//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ILMLeadManagementRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, ILMLMDataModel, userDetailsModel, pubsub, security) {
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
        };

        vm.isActive = function () {
            return ILMLMDataModel.isActive();
        };

        vm.loadData = function (myAccess) {
            if (persona.isReady() && vm.isActive()) {
                rolesGrid.busy(true);
                var params = {
                    productType: "LeadManagement",
                    subjectPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            rolesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                var rolesData = $filter("orderBy")(resp.records, "name");

                gridPagination.setData(rolesData || []).goToPage({
                    number: 0
                });

                ILMLMDataModel.setRoles(rolesData);
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
            return !persona.data.hasManageILMLeadManagemementProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            rolesGrid.destroy();
            //vm.accessTypeWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
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
        .controller("ILMLeadManagementRolesGridCtrl", [
            "$scope",
            "$filter",
            "ProductRolesSvc",
            "rpGridModel",
            "ILMLeadManagementRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "ilmLeadManagementDataModel",
            "userDetailsModel",
            "pubsub",
            "routeSecurity",
            ILMLeadManagementRolesGridCtrl
        ]);
})(angular);