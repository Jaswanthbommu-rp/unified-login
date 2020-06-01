//  Companies Grid Tab Controller
(function (angular, undefined) {
    "use strict";

    function ProductCompaniesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, pubsub, persona, productDataModel, userDetailsModel, security, syncMgr, companiesSvc, switchConfig) {
        var vm = this,
            hasViewUserAccess,
            allCompanies,
            companiesGrid = gridModel(),
            companiesGridTransform = gridTransformSvc(),
            companiesGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            userLoginName = "",
            activeCompanies = [],
            inactiveCompanies = [];

        vm.init = function () {
            vm.companySelect = "company";
            vm.productId = 0;
            vm.activeCompanies = activeCompanies;
            vm.inactiveCompanies = inactiveCompanies;
            vm.allCompanies = false;
            vm.isShowCompanies = true;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");

            vm.companiesGrid = companiesGrid;
            companiesGridTransform.watch(companiesGrid);
            vm.config = syncMgr.getProductGridConfig($scope.$parent.productId, "Companies");

            companiesGrid.setConfig(vm.config);
            companiesGridPagination.setGrid(companiesGrid);
            $scope.companiesGridPagination = companiesGridPagination;
            companiesGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.productCompanyWatch = $scope.$watch(vm.isActive, vm.loadData);

            pubsub.subscribe("ppanel.property-radio", vm.updateCompanyRecords);
            vm.gridAllWatch = companiesGrid.subscribe("selectAll", vm.selectAllCompanies);
            vm.gridSelectionWatch = companiesGrid.subscribe("selectChange", vm.updateMultiSelectCompanyRecords);
            vm.filterData = companiesGrid.subscribe("filterBy", vm.filter.bind(vm));
            vm.updateGridWatch = pubsub.subscribe("pplpropertygroup.updateGrids", vm.updateGrid);
        };

        vm.productSelected = function (obj) {
            vm.productId = obj.productId;
            $scope.productId = obj.productId;
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        vm.filter = function (filterBy) {
            if (vm.companySelect === 'active') {
                vm.filteredRecords = $filter("filter")(vm.activeCompanies, filterBy);
            }
            else if (vm.companySelect === 'inactive') {
                vm.filteredRecords = $filter("filter")(vm.inactiveCompanies, filterBy);
            }
            else {
                vm.filteredRecords = $filter("filter")(vm.allCompaniesData, filterBy);
            }

            companiesGridPagination.setData(vm.filteredRecords).goToPage({
                number: 0
            });
        };

        vm.isReady = function () {
            return productDataModel.isCompanyGridActive();
        };

        vm.isActive = function () {
            return productDataModel.isCompanyGridActive();
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;

            companiesGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var CompanyData = syncMgr.getProductCompaniesData(productId);

                if (CompanyData === undefined) {
                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        productId: productId,
                        userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName()
                    };

                    vm.dataPropReq = companiesSvc.get(params, vm.setCompanyData);
                }
                else {
                    vm.loadGridData(productId);
                }
            }
        };

        vm.setCompanyData = function (resp) {
            companiesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                var pdata = syncMgr.setCompanyList(resp.records, $scope.$parent.productId);

                if (resp.additional && resp.additional.allCompanies) {
                    syncMgr.updateCompanyAllProperties($scope.$parent.productId, true);
                    vm.allCompanies = true;
                }
                vm.loadGridData($scope.$parent.productId);
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

        vm.loadGridData = function (productId) {
            companiesGrid.busy(false);
            var companySelect = "company";
            var compData = syncMgr.getProductCompaniesData(productId);

            if (compData && compData.length > 0) {
                if (vm.hasViewOnlyAccess()) {
                    compData.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false,
                            radname: "company"
                        });
                        item.disabled = true;
                    });
                }

                compData.forEach(function (item) {
                    angular.extend(item, {
                        radname: "company",
                        productId: productId,
                        originalProperty: item.isAssigned
                    });
                });

                if (syncMgr.isProductAllCompanies(productId)) {
                    companySelect = "allCompanies";
                    vm.allCompanies = true;
                }

                vm.companySelect = companySelect;
                vm.allCompaniesData = compData;
                companiesGridPagination.setData(compData).goToPage({
                    number: 0
                });
            }

            return vm;
        };

        vm.selectionAll = function (bool) {
            vm.companySelect = "company";
            if (bool) {
                vm.companySelect = 'allCompanies';
            }

            syncMgr.allPropertiesSync($scope.$parent.productId, bool);            
            vm.companiesGrid.updateSelected();
            vm.resetProperties();
        };

        vm.selectAllCompanies = function (val) {
            if (vm.filteredRecords !== undefined) {
                vm.filteredRecords.forEach(function (item) {
                    item.isAssigned = val;
                });

                syncMgr.updateAllCompanies($scope.$parent.productId, vm.filteredRecords);
            }
            else {
                vm.allCompaniesData.forEach(function (item) {
                    item.isAssigned = val;
                });

                syncMgr.updateAllCompanies($scope.$parent.productId, vm.allCompaniesData);
            }
        };

        vm.updateCompanyRecords = function (record) {
            if (record) {
                var companiesData = syncMgr.selectedCompanySync(record.productId, record);
            }
        };

        vm.updateMultiSelectCompanyRecords = function (record) {
            if (record) {
                var companiesData = syncMgr.multiSelectedCompanySync(record.productId, record);
            }
        };

        vm.updateGrid = function () {
            vm.companiesGrid.updateSelected();
        };

        vm.resetDataModel = function () {
            vm.resetCompanies();
        };

        vm.resetCompanies = function () {
            vm.allCompanies = false;
            if (vm.companySelect === 'allCompanies') {
                vm.allCompanies = true;
            }

            syncMgr.updateProductAllProperties($scope.$parent.productId, vm.allCompanies);

            if (vm.companySelect == "active") {
                companiesGridPagination.setData(vm.activeCompanies).goToPage({
                    number: 0
                });
                vm.companiesGrid.filtersModel.reset();
            }
            else if (vm.companySelect == "inactive") {
                companiesGridPagination.setData(vm.inactiveCompanies).goToPage({
                    number: 0
                });
                vm.companiesGrid.filtersModel.reset();
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.productCompanyWatch();
            vm.gridAllWatch();
            vm.gridSelectionWatch();
            vm.updateGridWatch();
            if (vm.dataPropReq) {
                vm.dataPropReq.$cancelRequest();
            }
            vm.isShowCompanies = true;
            companiesGrid.destroy();
            companiesGridTransform.destroy();
            companiesGridPagination.destroy();
            companiesGrid = undefined;
            companiesGridTransform = undefined;
            companiesGridPagination = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductCompaniesGridCtrl", [
            "$scope",
            "$filter",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "productPanelDataModel",
            "userDetailsModel",
            "routeSecurity",
            "productDataSyncManager",
            "ProductCompaniesSvc",
            "rpSwitchConfig",
            ProductCompaniesGridCtrl
        ]);
})(angular);
