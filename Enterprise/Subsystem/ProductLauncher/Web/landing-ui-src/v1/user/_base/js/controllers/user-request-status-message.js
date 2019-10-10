//  User Request Status Message Controller

(function (angular, undefined) {
    "use strict";

    function UserReqStatusMsgCtrl($scope, $params, $location, model, modal, session) {
        var vm = this;

        vm.init = function () {
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.dismissModal = function () {
            modal.hide();
            if (session.getRealPageId() === $params.realPageId) {
                window.location.href ="../home";
            }
            else {
                if (model.onGoToUsers()) {
                    window.location.href = "../people/users";
                }
            }

        };

        vm.destroy = function () {
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UserReqStatusMsgCtrl", [
            "$scope",
            "$stateParams",
            "$location",
            "userReqStatusMsgModel",
            "userReqStatusMsgModal",
            "userSessionModel",
            UserReqStatusMsgCtrl
        ]);
})(angular);
