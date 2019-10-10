//  Single User Activity Controller

(function(angular, undefined) {
    "use strict";

    function ALSingleUserActivityAsideCtrl($scope, $window, moment, aside, activitySvc, payloadModel, gridModel, gridConfig,
        gridTransformSvc, gridPaginationModel, userInfoModel, userProfileSvc, profileModel, exportMenu, dataExportSvc) {
        var vm = this,
            asideGrid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            payload = {};

        vm.init = function() {
            vm.title = "User Activity Details";
            vm.asideGrid = asideGrid;
            asideGrid.setConfig(gridConfig);
            gridPagination.setGrid(asideGrid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.payloadModel = payloadModel;
            vm.profileModel = profileModel;

            vm.loginName = userInfoModel.getLoginName();
            vm.realpageId = userInfoModel.getRealpageID();
            vm.profileLink = "#/user/" + vm.realpageId + "/UserList/edit";
            vm.loadProfileData();
            vm.loadData();

            vm.exportMenu = exportMenu(vm);

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            return vm;
        };

        vm.loadData = function() {
            asideGrid.busy(true);
            var params = {},
                realpageIDPayload = {};
            payload = vm.payloadModel.buildPayload(vm.singleUser);
            realpageIDPayload = {
                "name": "RealpageId",
                "value": vm.realpageId
            };
            payload.activitySearchCriteria.push(realpageIDPayload);
            activitySvc.getActivitiesForUser(payload).then(vm.setData);
        };

        vm.loadProfileData = function() {
            userProfileSvc.get(vm.realpageId, vm.setProfileData);
        };

        vm.setData = function(resp) {
            asideGrid.busy(false);
            // vm.model.setData(resp.data);
            resp.data.forEach(function(item) {
                item.activityDate = vm.setActivityDate(item.applicationTimestamp);
            });
            if (resp.data && resp.data.length > 0) {
                gridPagination.setData(resp.data).goToPage({
                    number: 0
                });
            }
        };

        vm.setProfileData = function(resp) {
            vm.profileModel.setData(resp);
        };

        vm.setActivityDate = function(activityDate) {
            if (activityDate) {
                activityDate = moment(activityDate).toDate();
            } else {
                activityDate = "";
            }
            return activityDate;
        };

        vm.getExportData = function (query) {
            var exportFlag = true;
            // var paramData = vm.payloadModel.buildPayload(vm.singleUser, exportFlag);
            var paramData = payload;
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

        vm.editProfile = function() {
            $window.location.href = vm.profileLink;
            vm.cancel();
        };

        vm.cancel = function() {
            aside.hide();
        };

        vm.hasValidPhoneNumber = function(telecomData) {
            return vm.profileModel.hasValidPhoneNumber(telecomData);
        };

        vm.loginNameIsEmail = function() {
            return vm.profileModel.loginNameIsEmail();
        };

        vm.destroy = function() {
            vm.destWatch();
            asideGrid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            vm.profileModel.reset();
            vm = undefined;
            $scope = undefined;
            asideGrid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ALSingleUserActivityAsideCtrl", [
            "$scope",
            "$window",
            "moment",
            "alSingleUserActivityAside",
            "activityLogSvc",
            "activityLogPayloadModel",
            "rpGridModel",
            "activityLogGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "singleUserInfoModel",
            "userActivityLogProfileDataSvc",
            "userActivityLogProfileModel",
            "activityLogAsideExportMenuConfig",
            "activityLogExportData",
            ALSingleUserActivityAsideCtrl
        ]);
})(angular);