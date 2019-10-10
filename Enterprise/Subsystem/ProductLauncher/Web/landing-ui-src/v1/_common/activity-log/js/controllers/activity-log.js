//  User Request Status Message Controller

(function (angular, undefined) {
    "use strict";

    function ActivityLogCtrl($scope, $stateParams, moment, payloadModel, activitySvc, activityLogFormConfig, filterOptions, filterDataFac, tabs, helpData, timeout, session, exportMenu, dataExportSvc, gridModel, gridConfig, gridPaginationModel) {
        var vm = this,
            grid = gridModel(),
            gridPagination = gridPaginationModel();

        vm.init = function () {
            vm.grid = grid;
            grid.setConfig(gridConfig);
            gridPagination.setGrid(grid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 100
            });
            vm.payloadModel = payloadModel;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.filter = vm.initForm();
            vm.activityLog = {};
            vm.singleUser = true;
            vm.register();
            vm.loadData();

            vm.exportMenu = exportMenu(vm);
        };

        vm.initForm = function () {
            activityLogFormConfig.init();
            filterOptions.setData(filterDataFac());
            vm.initFilterDropdowns();
            activityLogFormConfig.setMethodsSrc(vm);

            return {
                config: activityLogFormConfig,
                model: filterDataFac(),
                isDisplayPanel: false
            };
        };

        vm.initFilterDropdowns = function () {
            activityLogFormConfig.setOptions("activities", filterOptions.getActivityTypes());
            activityLogFormConfig.setOptions("daterange", filterOptions.getDateRanges());
            activityLogFormConfig.setOptions("sortby", filterOptions.getSortFilters());
        };

        vm.register = function () {
            tabs.register({
                ctrl: vm,
                name: "activityLog"
            });
        };

        vm.onTabActive = function () {
            var helpWidget = document.querySelector('raul-unified-help');
            helpWidget.helpPageId = "activity";
            vm.active = true;
            return vm;
        };

        vm.onTabInactive = function () {
            vm.active = false;
            return vm;
        };

        vm.onFilterData = function (filterBy) {
            // model.filterData(filterBy);
        };

        vm.filterByKeyword = function (keyword) {
            vm.payloadModel.buildKeywordPayload(keyword);
            vm.loadData();
        };

        vm.filterByActivities = function (activityValue) {
            vm.payloadModel.buildActivityPayload(activityValue);
            vm.loadData();
        };

        vm.filterByDates = function (daterange) {
            vm.payloadModel.setDateRange(daterange);
            vm.loadData();
        };

        vm.sortByFilter = function (filterItem) {
            vm.payloadModel.setSortOrderPayload(filterItem);
            vm.loadData();
        };

        vm.hasActivities = function () {
            return vm.model.hasActivities();
        };

        vm.loadData = function () {
            grid.busy(true);
            var params = {};
            var payload = vm.payloadModel.buildPayload(vm.singleUser);
            vm.dataReq = activitySvc.getActivitiesForUser(payload).then(vm.setData);
        };

        vm.setData = function (resp) {
            grid.busy(false);
            resp.data.forEach(function(item) {
                item.activityDate = vm.setActivityDate(item.applicationTimestamp);
            });
            gridPagination.setData(resp.data).goToPage({
                number: 0
            });
        };

        vm.setActivityDate = function(activityDate) {
            if (activityDate) {
                activityDate = moment(activityDate).toDate();
            }
            else {
                activityDate = "";
            }
            return activityDate;
        };

        vm.showErrors = function () {
            vm.form.$setSubmitted();
            timeout($scope.focusInvalidField.focus, 100);
        };

        vm.getExportData = function (query) {
            var exportFlag = true;
            var paramData = vm.payloadModel.buildPayload(vm.singleUser, exportFlag);
            paramData.dataFormat = query.fileType;
            var promise = dataExportSvc.getActivity(paramData).$promise;
            promise.then(vm.processExportData, vm.onExportError);
            return promise;
        };

        vm.processExportData = function (resp) {
            resp.fileContent = resp.data;
            delete resp.Resource;
        };

        vm.onExportError = function () {
            logc("There was an error with the activity log export.");
        };

        vm.onUpdateError = function (resp) {
            vm.saveError = true;
            vm.updateDeferred.reject({
                success: false,
                tabName: "activityLog"
            });
        };

        vm.onUpdateSuccess = function (resp) {
            vm.saveError = false;
            vm.form.$setUntouched();

            if (!resp.isError) {
                vm.updateDeferred.resolve({
                    success: true,
                    tabName: "activityLog"
                });
            }
            else {
                vm.updateDeferred.reject({
                    success: false,
                    tabName: "activityLog"
                });
            }
        };

        vm.editingSelf = function () {
            return session.getRealPageId() === $stateParams.realPageId;
        };
        // Assertions

        vm.hasSaveFn = function () {
            return false;
        };

        vm.isDirty = function () {
            return false;
        };

        vm.isValid = function () {
            return true;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.payloadModel.reset();
            vm.filter.model = undefined;
            vm.filter.config = undefined;
            vm.filter = undefined;
            grid.destroy();
            gridPagination.destroy();
            vm = undefined;
            grid = undefined;
            $scope = undefined;
            gridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ActivityLogCtrl", [
            "$scope",
            "$stateParams",
            "moment",
            "activityLogPayloadModel",
            "activityLogSvc",
            "activityLogFormConfig",
            "activityLogFilterOptions",
            "activityLogFilterData",
            "userTabsManager",
            "rpGhHelpData",
            "timeout",
            "userSessionModel",
            "activityLogExportMenuConfig",
            "activityLogExportData",
            "rpGridModel",
            "activityLogGridConfig",
            "rpGridPaginationModel",
            ActivityLogCtrl
        ]);
})(angular);
