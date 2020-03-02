//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductRolesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, persona, pubsub, productDataModel, userDetailsModel, security, configModel, syncMgr) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.rolesGrid = rolesGrid;
            vm.productId = "";
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            rolesGridTransform.watch(rolesGrid);
            // rolesGrid.setConfig(gridConfig);
            vm.config = configModel.getGridConfig().length > 1 ? configModel.getGridConfig()[1] : configModel.getGridConfig()[0];
            rolesGrid.setConfig(vm.config);

            gridPagination.setGrid(rolesGrid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            //vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            vm.productRoleSelectedWatch = pubsub.subscribe("product.ProductRoleData", vm.setData);
            // if (persona.isReady()) {
            //     vm.loadData();
            // }
            // else {
            //     vm.personaWatch = persona.subscribe(vm.loadData);
            // }
            //pubsub.subscribe("cp.roles-radio", vm.updateRoleRecords);
        };

        vm.isActive = function () {
            return true;//productDataModel.isActive();
        };

        // vm.loadData = function (productId) {
        //     rolesGrid.busy(true);
        //     if (persona.isReady() && vm.isActive()) {
        //         var params = {
        //             userPersonaId: userDetailsModel.getPersonaId(),
        //             editorPersonaId: persona.getId()
        //         };

        //         vm.activeWatch();
        //         vm.personaWatch();
        //         vm.dataReq = dataSvc.get(params, vm.setData);
        //     }
        // };

        vm.updateRoleRecords = function (record) {
            var rolesData = productDataModel.getRoles();

            rolesData.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.setData = function (productId) {
            vm.rolesGrid.busy(false);
            var roleData = syncMgr.getProductRolesData(productId);

            if (roleData && roleData.length > 0) {
                vm.productId = productId;
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    roleData.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false,
                            radname: "role"
                        });
                        item.disabled = true;
                    });
                }

                roleData.forEach(function (item) {
                        angular.extend(item, {
                            radname: "role"
                        });
                });

                gridPagination.setData(roleData).goToPage({
                    number: 0
                });
                //productDataModel.setRoles(resp.records);
            }
            // if (resp.isError) {
            //     vm.isDataError = true;
            //     if (resp.errorReason !== "") {
            //         vm.dataErrorReason = resp.errorReason;
            //     }
            //     else {
            //         vm.dataErrorReason = genericDataErrorReason;
            //     }
            // }
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
            vm.productRoleSelectedWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductRolesGridCtrl", [
            "$scope",
            "$filter",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "pubsub",
            "productPanelDataModel",
            "userDetailsModel",
            "routeSecurity",
            "ConfigModel",
            "productDataSyncManager",
            ProductRolesGridCtrl
        ]);
})(angular);
