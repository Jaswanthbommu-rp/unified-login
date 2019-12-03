//  BusinessIntelligence Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function BusinessIntelligenceRolesGridCtrl($scope, $filter, companyRoleDataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, BIDataModel, userDetailsModel, pubsub, statusModel, security) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            userLoginName = "",
            userRealPageId =  "00000000-0000-0000-0000-000000000000";

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
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadCompanyRoleData);
            vm.aoUserUpdateWatch = pubsub.subscribe("settings.aoUserUpdate", vm.updateLoad);

            if (persona.isReady()) {
                vm.loadCompanyRoleData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadCompanyRoleData);
            }
            vm.gridAllWatch = rolesGrid.subscribe("selectAll", vm.selectAllRoles);
        };

        vm.updateLoad = function (userRealPageId) {
            logc("Roles statusModel", statusModel);
            logc("Roles userRealPageId", userRealPageId);
            userRealPageId = userRealPageId;
            vm.loadCompanyRoleData(statusModel.BICompanyId);
        };

        vm.isActive = function () {
            return BIDataModel.isActive();
        };

        vm.loadCompanyRoleData = function () {
            if (persona.isReady() && BIDataModel.isActive()) {
                rolesGrid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    productName: "BI",
                    userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = companyRoleDataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            rolesGrid.busy(false);
            if (resp.records && resp.records.length == 1) {
                var bidata = resp.records[0];

                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    bidata.roles.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                gridPagination.setData(bidata.roles).goToPage({
                    number: 0
                });
                //pubsub.publish("BIPG.CompanyData", bidata.companyId);
                statusModel.setBICompanyId(bidata.companyId);
                BIDataModel.setCompanyRoles(bidata);
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
            return !persona.data.hasManageAssetOptimizationProductAccess;
        };

        vm.selectAllRoles = function (val) {
            BIDataModel.setAllRoles(vm.dataReq.records[0].roles, val);
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridAllWatch();
            vm.activeWatch();
            vm.aoUserUpdateWatch();
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
        .controller("BusinessIntelligenceRolesGridCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationRolesSvc",
            "rpGridModel",
            "BusinessIntelligenceRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "businessIntelligenceDataModel",
            "userDetailsModel",
            "pubsub",
            "aOStatusModel",
            "routeSecurity",
            BusinessIntelligenceRolesGridCtrl
        ]);
})(angular);
