//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function VendCompRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, VendCompDataModel, userDetailsModel, pubsub, security) {
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

            vm.accessTypeWatch = pubsub.subscribe("vc.access-type-change", vm.loadData);
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
            return VendCompDataModel.isActive();
        };

        vm.loadData = function (myAccess) {
            if (persona.isReady() && vm.isActive()) {
                rolesGrid.busy(true);
                if (myAccess == "all") {
                    myAccess = "Client";
                }
                else if (myAccess == "group" || myAccess == "property" || myAccess === true) {
                    myAccess = "Property";
                }
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    accessType: myAccess
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            if (resp.records && resp.records.length > 0) {
                gridPagination.setData(resp.records || []).goToPage({
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

                rolesGrid.busy(false);
                VendCompDataModel.setRoles(resp.records);
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
            return !persona.data.hasManageVendorComplianceProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            rolesGrid.destroy();
            vm.accessTypeWatch();
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
        .controller("VendCompRolesGridCtrl", [
            "$scope",
            "$filter",
            "VendCompRolesSvc",
            "rpGridModel",
            "VendCompRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "vendorComplianceDataModel",
            "userDetailsModel",
            "pubsub",
            "routeSecurity",
            VendCompRolesGridCtrl
        ]);
})(angular);
