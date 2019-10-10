//  People Users Controller

(function (angular) {
    "use strict";

    function PeopleUsersCtrl($scope, $filter, $timeout, $window, ENV, usersFilterData, usersFilterModel, usersFilterConfig, usersGridModel, usersGridActions, userListSvc, rpWatchList, productsDataModel, notifSvc, security, exportMenu, dataExportSvc, persona, userTypes) {
        var vm = this,
            actions = {
                activate: "activate",
                deactivate: "deactivate",
                lock: "lock",
                unlock: "unlock"
            };

        vm.init = function () {
            vm.security = security;
            vm.watchList = rpWatchList();
            vm.watchList.add($scope.$on("$destroy", vm.destroy));

            vm.filter = vm.initForm();
            vm.grid = usersGridModel.init();
            vm.selectAll = false;

            usersGridActions.setSrc(vm);

            vm.exportMenu = exportMenu(vm);
        };

        vm.initForm = function () {
            //TODO get available product and properties
            vm.initFilterDropdowns();

            usersFilterModel.setData(usersFilterData);
            usersFilterConfig.setMethodsSrc(vm);

            return {
                config: usersFilterConfig,
                model: usersFilterData,
                isDisplayPanel: false
            };
        };

        vm.initFilterDropdowns = function () {
            vm.getUserTypeOptions();
            //usersFilterConfig.setOptions("userType", usersFilterModel.getUserTypes());
            usersFilterConfig.setOptions("accountStatus", usersFilterModel.getAccountStatus());
            usersFilterConfig.setOptions("lockStatus", usersFilterModel.getLockStatus());
            vm.setFilterProducts(productsDataModel.getData());
            vm.sessionWatch = productsDataModel.subscribe(vm.setFilterProducts);
        };

        //getters
        vm.getUserTypeOptions = function () {
            var reqData = {
                roleTypeName: "user role"
            };

            vm.userTyepReq = userTypes.get(reqData, vm.initUserTypeOptions);
        };

        vm.initUserTypeOptions = function (resp) {
            usersFilterConfig.setOptions("userType", usersFilterModel.setUserTypeOptions(resp.data));
        };

        vm.setFilterProperties = function (response) { //TODO update depending on response when available
            var records = [];
            if (response && response.records && response.records.length > 0) {
                records = response.records;
            }

            records.unshift({
                propertyId: "",
                propertyName: $filter("userListText")("users_all_properties")
            });
            usersFilterConfig.setOptions("properties", records);
        };

        vm.setFilterProducts = function (records) {
            records.unshift({
                productId: "",
                showInUserListFilter: "true",
                productName: $filter("userListText")("users_all_products")
            });

            usersFilterConfig.setOptions("products", records);
        };

        vm.filterGrid = function () {
            usersGridModel.filterList(usersFilterData);
        };

        vm.filterByName = function (name) {
            usersGridModel.filterByName(name, usersFilterData);
        };

        vm.applyFilter = function () {
            usersGridModel.filterList(usersFilterData);
        };

        vm.hideMoreFilters = function () {
            vm.filter.isDisplayPanel = false;
            vm.resetFilter();
        };

        vm.resetFilter = function () {
            usersFilterModel.reset();
            usersGridModel.filterList(usersFilterData);
        };

        vm.resetNoResultsFilter = function () {
            $timeout(function () {
                if (usersGridModel.getData().totalRecords === 0) {
                    vm.resetFilter();
                }
            }, 500);
        };

        vm.cloneUser = function (record) {
        };

        vm.lockUser = function (record) {
            usersGridModel.setIsGridLoading();
            userListSvc.updateUser({
                    statusTypeName: "Locked",
                    realPageId: record.realPageId
                })
                .then(vm.callbackHolder)
                .catch(vm.userStatusError)
                .finally(vm.enableActionBtn);
        };

        vm.unlockUser = function (record) {
            usersGridModel.setIsGridLoading();
            userListSvc.updateUser({
                    statusTypeName: "Unlocked",
                    realPageId: record.realPageId
                })
                .then(vm.callbackHolder)
                .catch(vm.userStatusError)
                .finally(vm.enableActionBtn);
        };

        vm.activateUser = function (record) {
            usersGridModel.setIsGridLoading();
            userListSvc.updateUser({
                    statusTypeName: "Active",
                    realPageId: record.realPageId
                })
                .then(vm.callbackHolder)
                .catch(vm.userStatusError)
                .finally(vm.enableActionBtn);
        };

        vm.deactivateUser = function (record) {
            usersGridModel.setIsGridLoading();
            userListSvc.updateUser({
                    statusTypeName: "Disabled",
                    realPageId: record.realPageId
                })
                .then(vm.callbackHolder)
                .catch(vm.userStatusError)
                .finally(vm.enableActionBtn);
        };

        vm.activateSelectedUsers = function () {
            if (vm.grid.gridModel.hasSelectionChanges()) {
                usersGridModel.setIsGridLoading();
                userListSvc.batchUpdateUsers({
                        userLoginStatusType: "Active",
                        updateType: vm.selectAll === "all" ? "AllRecords" : "Batch"
                    }, usersGridModel.updatedUserLogins(vm.selectAll))
                    .then(vm.callbackHolder)
                    .catch(vm.userStatusError)
                    .finally(vm.enableActionBtn);
            }
        };

        vm.deactivateSelectedUsers = function () {
            if (vm.grid.gridModel.hasSelectionChanges()) {
                usersGridModel.setIsGridLoading();
                userListSvc.batchUpdateUsers({
                        userLoginStatusType: "Disabled",
                        updateType: vm.selectAll === "all" ? "AllRecords" : "Batch",
                    }, usersGridModel.updatedUserLogins(vm.selectAll))
                    .then(vm.callbackHolder)
                    .catch(vm.userStatusError)
                    .finally(vm.enableActionBtn);
            }
        };

        vm.lockSelectedUsers = function () {
            if (vm.grid.gridModel.hasSelectionChanges()) {
                usersGridModel.setIsGridLoading();
                userListSvc.batchUpdateUsers({
                        userLoginStatusType: "Locked",
                        updateType: vm.selectAll === "all" ? "AllRecords" : "Batch"
                    }, usersGridModel.updatedUserLogins(vm.selectAll))
                    .then(vm.callbackHolder)
                    .catch(vm.userStatusError)
                    .finally(vm.enableActionBtn);
            }
        };

        vm.unlockSelectedUsers = function () {
            if (vm.grid.gridModel.hasSelectionChanges() && usersGridModel.hasSelectedLockedUsers()) {
                usersGridModel.setIsGridLoading();
                userListSvc.batchUpdateUsers({
                        userLoginStatusType: "Unlocked",
                        updateType: vm.selectAll === "all" ? "AllRecords" : "Batch"
                    }, usersGridModel.updatedUserLogins(vm.selectAll))
                    .then(vm.callbackHolder)
                    .catch(vm.userStatusError)
                    .finally(vm.enableActionBtn);
            }
        };

        vm.resendInvites = function () {
            if (vm.grid.gridModel.hasSelectionChanges()) {
                usersGridModel.setIsGridLoading();
                userListSvc.resendInvitation({}, usersGridModel.getUserLoginsForEmailResent())
                    .then(vm.callbackHolder)
                    .catch(vm.userStatusError)
                    .finally(vm.enableActionBtn);
            }
        };

        vm.callbackHolder = function (response) {
            vm.grid.load();
            vm.resetNoResultsFilter();

        };

        vm.userStatusError = function () {
            notifSvc.error($filter("userListText")("user_status_error_msg"));
        };

        vm.enableActionBtn = function () {
            usersGridModel.setIsGridLoading(false);
        };

        vm.toggleSelect = function (flag) {
            if (flag === true) {
                vm.selectAll = true;
            }
            else {
                vm.selectAll = false;
            }
        };

        vm.toggleSelectAll = function () {
            usersGridModel.updateSelectAll(vm.selectAll);
        };

        vm.getExportData = function (query) {
            var paramData = usersGridModel.currentFilter;
            paramData.dataFormat = query.fileType;
            var promise = dataExportSvc.getList(paramData).$promise;
            promise.then(vm.processExportData, vm.onExportError);
            return promise;
        };

        vm.processExportData = function (resp) {
            resp.fileContent = resp.data;
            delete resp.data;
        };

        vm.onExportError = function () {
            logc("There was an error with the user list export.");
        };

        vm.getCustomColState = function () {
             return {
                "display-custom": usersGridModel.getIsCustomColVisible()
            };
        };

        vm.hasViewOnlySupportToolAccess = function () {
            return persona.hasViewOnlySupportToolAccess();
        };

        vm.hasImportUsersAccess = function () {
            return persona.hasImportUsersAccess();
        };        

        vm.openImportUsers = function() {
            var url = ENV.ulmtUrl + "#/import-users";
            $window.open(url);
        };

        vm.destroy = function () {
            vm.sessionWatch();
            vm.watchList.destroy();
            vm.watchList = undefined;

            vm.exportMenu.destroy();

            vm.grid.destroy();
            vm.grid = undefined;

            vm.filter.model = undefined;
            vm.filter.config = undefined;
            vm.filter = undefined;
            vm = undefined;

            //clear filters
            usersFilterData.isLocked = "";
            usersFilterData.name = "";
            usersFilterData.product = "";
            usersFilterData.property = "";
            usersFilterData.status = "";
            usersFilterData.userType = "";
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("PeopleUsersCtrl", [
            "$scope",
            "$filter",
            "$timeout",
            "$window",
            "ENV",
            "usersListFilterData",
            "usersListFilterModel",
            "usersListFormConfig",
            "userListGridModel",
            "userListGridActions",
            "userListSvc",
            "rpWatchList",
            "productsDataModel",
            "notificationService",
            "routeSecurity",
            "userListExportMenuConfig",
            "exportData",
            "personaDetails",
            "userTypeOptionsSvc",
            PeopleUsersCtrl
        ]);
})(angular);
