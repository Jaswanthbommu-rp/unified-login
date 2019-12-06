//  Investment Analytics Markets Controller

(function (angular, undefined) {
    "use strict";

    function InvestmentAnalyticsMarketsCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, IADataModel, userDetailsModel, pubsub, switchConfig, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            userLoginName = "",
            userRealPageId =  "00000000-0000-0000-0000-000000000000";

        vm.init = function () {
            vm.grid = grid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            vm.allProperties = false;
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
            vm.gridAllWatch = grid.subscribe("selectAll", vm.selectAllMarkets);
            vm.updateGridWatch = pubsub.subscribe("IAM.updateGrids", vm.updateGrid);
            vm.aoUserUpdateWatch = pubsub.subscribe("settings.aoUserUpdate", vm.updateLoad);
        };

        vm.updateLoad = function (userRealPageId) {            
            logc("Prop userRealPageId", userRealPageId);
            userRealPageId = userRealPageId;
            vm.loadData();
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };

        vm.isActive = function () {
            return IADataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && IADataModel.isActive()) {
                grid.busy(true);
                var companyData = IADataModel.getCompanies();
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    selectedCompanies: [0],
                    productName: "MA",
                    userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
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
                IADataModel.setPropertyGroups(resp.records);
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

        vm.selectAllMarkets = function (val) {
            IADataModel.setAllMarkets(vm.dataReq.records, val);
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridAllWatch();
            vm.updateGridWatch();
            vm.aoUserUpdateWatch();
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
        .controller("InvestmentAnalyticsMarketsCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationPropertyGroupSvc",
            "rpGridModel",
            "investmentAnalyticsMarketsGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "investmentAnalyticsDataModel",
            "userDetailsModel",
            "pubsub",
            "rpSwitchConfig",
            "routeSecurity",
            InvestmentAnalyticsMarketsCtrl
        ]);
})(angular);
