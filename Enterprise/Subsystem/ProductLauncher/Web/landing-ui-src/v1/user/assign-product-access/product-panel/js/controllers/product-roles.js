//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductRolesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, persona, pubsub, productDataModel, userDetailsModel, security, configModel, syncMgr, roleSvc) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            roleGridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            //$scope.rolesGrid = gridModel();
            vm.rolesGrid = rolesGrid;
           // vm.productId = 0;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            rolesGridTransform.watch(rolesGrid);
            // rolesGrid.setConfig(gridConfig);
            // vm.config = configModel.getGridConfig().length > 1 ? configModel.getGridConfig()[1] : configModel.getGridConfig()[0];
            vm.config = configModel.getConfig("Roles");
            rolesGrid.setConfig(vm.config.gridConfig);

            roleGridPagination.setGrid(rolesGrid);
            $scope.roleGridPagination = roleGridPagination;
            roleGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isReady, vm.loadData);

           // pubsub.subscribe("product.ProductRoleData", vm.setData);
            // if (persona.isReady()) {
            //     vm.loadData();
            // }
            // else {
            //     vm.personaWatch = persona.subscribe(vm.loadData);
            // }
            pubsub.subscribe("ppanel.role-radio", vm.updateRoleRecords);
        };

        vm.isActive = function () {
            return true;//productDataModel.isActive();
        };

        vm.isReady = function () {
            return productDataModel.isRoleGridActive();//productDataModel.isActive();
        };

        vm.updateRoleRecords = function (record) {
            var rolesData = syncMgr.selectedRoleSync(record.productId, record);
        };

         vm.loadData = function () {
            var productId = $scope.$parent.productId;
             rolesGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                  var roleData = syncMgr.getProductRolesData(productId);
                 // logc("propertyData",propertyData,productId);
                  if (roleData === undefined){

                    var params = {
                         userPersonaId: userDetailsModel.getPersonaId(),
                         editorPersonaId: persona.getId(),
                         partyId: persona.data.organization.partyId,
                         productId: productId
                    };

                    vm.dataRoleReq = roleSvc.get(params, vm.setRolesData);
                  }
                  else {
                    //syncMgr.setPropertyGridActive(true);
                    vm.loadGridData(productId);
                  }
            }
        };

        vm.setRolesData = function (resp) {
           // var productId = $scope.$parent.productId;
            if (resp.records && resp.records.length > 0){
               // logc("setPropertyData",resp.records, vm.productId);
                var rdata = syncMgr.setRoleList(resp.records, $scope.$parent.productId);
                //syncMgr.setPropertyGridActive(true);
                vm.loadGridData($scope.$parent.productId);
             }
        };


        vm.loadGridData = function (productId) {
            //var productId = $scope.$parent.productId;
            var roleData = syncMgr.getProductRolesData(productId);
            if (roleData && roleData.length > 0) {
               // vm.productId = productId;
                if (security.isAllowed("viewUser") ) {
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
                            radname: "role",
                            productId: productId
                        });
                });

                roleGridPagination.setData(roleData).goToPage({
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
            vm.rolesGrid.busy(false);
            return vm;
        };

        vm.setData = function (productId) {
            logc("$scope.rolesGrid", $scope.rolesGrid);
            vm.rolesGrid = $scope.rolesGrid;
            vm.rolesGrid.busy(true);
            var roleData = syncMgr.getProductRolesData(productId);
            //gridPagination = $scope.gridPagination;

            if (roleData && roleData.length > 0) {
               // vm.productId = productId;
                if (security.isAllowed("viewUser") ) {
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
                            radname: "role",
                            productId: productId
                        });
                });
                logc("gridPagination.grid",roleGridPagination);
                // if (gridPagination === undefined){
                //      // rolesGrid.setConfig(vm.config);
                //      // gridPagination.setGrid(rolesGrid);
                //      gridPagination = $scope.gridPagination;
                //      logc("pubsub roleData", rolesGrid, gridPagination, vm.config);
                // }
                rolesGridTransform.watch(rolesGrid);
            // rolesGrid.setConfig(gridConfig);
                var config = configModel.getGridConfig().length > 1 ? configModel.getGridConfig()[1] : configModel.getGridConfig()[0];
                rolesGrid.setConfig(config);

                roleGridPagination.setGrid(rolesGrid);
                $scope.roleGridPagination = roleGridPagination;
                roleGridPagination.setConfig({
                    recordsPerPage: 25
                });
                roleGridPagination.setData(roleData).goToPage({
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
            vm.rolesGrid.busy(false);
            return vm;
        };

        // vm.isUserHasManageProductAccess = function () {
        //     return !persona.data.hasManageClientPortalProductAccess;
        // };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.activeWatch();
            if (vm.dataRoleReq) {
                vm.dataRoleReq.$cancelRequest();
            }
            rolesGrid.destroy();
            rolesGridTransform.destroy();
            roleGridPagination.destroy();
            rolesGrid = undefined;
            rolesGridTransform = undefined;
            roleGridPagination = undefined;
           // vm.productRoleSelectedWatch();
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
            "productRolesSvc",
            ProductRolesGridCtrl
        ]);
})(angular);
