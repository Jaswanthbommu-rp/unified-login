//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductTab6GridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, persona, pubsub, productDataModel, userDetailsModel, security, syncMgr, roleSvc, pgSvc) {
        var vm = this,
            tab6Grid = gridModel(),
            tab6GridTransform = gridTransformSvc(),
            tab6GridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            assignedRoleId = 0,
            userLoginName = "";

        vm.init = function () {
            vm = this;
            vm.tab6Grid = tab6Grid;
            vm.assignedRoleId = 0;
            vm.diqAreas = [];

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            tab6GridTransform.watch(tab6Grid);
            logc("syncMgr", syncMgr);
            if ($scope.$parent.productId == 30) {
                tab6Grid.setConfig(syncMgr.getProductGridConfig(34, "BenchmarkingRole"));
            }
            else if ($scope.$parent.productId == 47) {
                tab6Grid.setConfig(syncMgr.getProductGridConfig(47, "Areas"));
            }

            tab6GridPagination.setGrid(tab6Grid);
            $scope.tab6GridPagination = tab6GridPagination;
            tab6GridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isReady, vm.loadData);

            vm.gridAllWatch = tab6Grid.subscribe("selectAll", vm.selectionAll);
            vm.gridSelectionWatch = tab6Grid.subscribe("selectChange", vm.updateMultiSelectRoleRecords);
            vm.filterData = tab6Grid.subscribe("filterBy", vm.filter.bind(vm));

        };

        vm.filter = function (filterBy) {
            if ($scope.$parent.productId !== 30) {
                vm.filteredRecords = $filter("filter")(vm.diqAreas, filterBy);
            }
        };

        vm.isActive = function () {
            return productDataModel.isRoleGridActive();
        };

        vm.isReady = function () {
            return productDataModel.isRoleGridActive(); //productDataModel.isActive();
        };


        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;
            if ($scope.$parent.productId == 30) {
                productId = 34; //BenchMark
            }

            tab6Grid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var data = syncMgr.getTab6ProductData(productId);
                logc("bmroleData", data, productId);
                if (data === undefined) {
                    if (productId == 34) {
                        var params1 = {
                            userPersonaId: userDetailsModel.getPersonaId(),
                            editorPersonaId: persona.getId(),
                            partyId: persona.data.organization.partyId,
                            productId: productId,
                            userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
                        };

                        vm.dataTabReq = roleSvc.get(params1, vm.setTabData);
                    }
                    else if (productId == 47) {
                        var params2 = {
                            userPersonaId: userDetailsModel.getPersonaId(),
                            editorPersonaId: persona.getId(),
                            productId: productId,
                            userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
                        };

                        vm.dataTabReq = pgSvc.get(params2, vm.setTabData);
                    }
                }
                else {
                    //syncMgr.setPropertyGridActive(true);
                    vm.loadGridData(productId);
                }
            }
        };


        vm.loadGridData = function (productId) {
            //var productId = $scope.$parent.productId;
            vm.diqAreas = [];
            tab6Grid.busy(false);
            var data = syncMgr.getTab6ProductData(productId);
            logc("syncMgr", syncMgr, data);
            if (data && data.length > 0) {
                data.forEach(function (item) {
                    angular.extend(item, {
                        disableSelection: vm.hasViewOnlyAccess(),
                        radname: "tab6",
                        productId: productId
                    });
                });

                if (productId == 47) {
                    data.map(function (area) {
                        if (area.groupType === 'area') {
                            vm.diqAreas.push(area);
                        }
                    });
                    tab6GridPagination.setData(vm.diqAreas).goToPage({
                        number: 0
                    });
                }
                else {
                    tab6GridPagination.setData(data).goToPage({
                        number: 0
                    });
                }

            }

            return vm;
        };

        vm.setTabData = function (resp) {
            tab6Grid.busy(false);
            var productId = $scope.$parent.productId;
            if ($scope.$parent.productId == 30) {
                productId = 34; //BenchMark
            }
            if (resp.records && resp.records.length > 0) {
                var rdata = syncMgr.setTab6DataList(resp.records, productId);
                vm.loadGridData(productId);
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

        vm.selectionAll = function (bool) {
            if ($scope.$parent.productId == 30) {
                syncMgr.allTab6DataSync(34, bool);
            }
            else if ($scope.$parent.productId !== 47) {
                syncMgr.allTab6DataSync($scope.$parent.productId, bool);
            }
            vm.tab6Grid.updateSelected();
        };

        vm.updateRoleRecords = function (record) {
            tab6Grid.busy(true);
            syncMgr.selectedTab6DataSync(record.productId, record);
            tab6Grid.busy(false);
        };

        vm.updateMultiSelectRoleRecords = function (record) {
            if (record) {
                syncMgr.multiSelectTab6DataSync(record.productId, record);
            }
        };

        vm.destroy = function () {
            logc("destroy called");
            vm.destWatch();
            vm.personaWatch();
            vm.activeWatch();
            vm.gridAllWatch();
            vm.gridSelectionWatch();
            // vm.productSelectTypeWatch();
            if (vm.dataTabReq) {
                vm.dataTabReq.$cancelRequest();
            }
            tab6Grid.destroy();
            tab6GridTransform.destroy();
            tab6GridPagination.destroy();
            tab6Grid = undefined;
            tab6GridTransform = undefined;
            tab6GridPagination = undefined;
            // vm.productRoleSelectedWatch();
            //vm = undefined;
            //$scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductTab6GridCtrl", [
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
            "productDataSyncManager",
            "productRolesSvc",
            "productPropertyGroupSvc",
            ProductTab6GridCtrl
        ]);
})(angular);
