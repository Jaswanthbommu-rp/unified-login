//  Single User Activity Controller

(function (angular, undefined) {
    "use strict";

    function SingleUserActivityLinkCtrl($scope, aside, userInfoModel) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showAside = function (record) {
            var userLoginName = "",
                userRealpageID = "";
            userLoginName = (record.toUserLoginName != null) ? record.toUserLoginName : record.fromUserLoginName;
            userRealpageID = (record.toUserLoginName != null) ? record.toUserRealpageId : record.fromUserRealpageId;
            userInfoModel.setLoginName(userLoginName);
            userInfoModel.setRealpageID(userRealpageID);
            aside.show();
        };

        vm.destroy = function () {
            vm.destWatch();
            aside = undefined;
            userInfoModel = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("SingleUserActivityLinkCtrl", [
            "$scope",
            "alSingleUserActivityAside",
            "singleUserInfoModel",
            SingleUserActivityLinkCtrl
        ]);
})(angular);
