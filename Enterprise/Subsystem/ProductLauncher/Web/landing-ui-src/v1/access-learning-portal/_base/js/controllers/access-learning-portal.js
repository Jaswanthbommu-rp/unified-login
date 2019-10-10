//   Product Learning Portal Modal Controller

(function (angular, undefined) {
    "use strict";

    function PlpModalCtrl($scope, $window, modalStates, svc, formConfig, formModel) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.modalStates = modalStates;
            vm.formConfig = formConfig;
            vm.formModel = formModel;
        };

        vm.submitEmailForm = function () {
            if ($scope.plpEmailForm.$valid) {
                vm.checkUserStatus();
            }
            else {
                vm.showFormError();
            }
        };

        vm.showFormError = function () {
            $scope.plpEmailForm.$setSubmitted();
        };

        vm.checkUserStatus = function () {
            var params = {
                userName: formModel.data.userEmailAddress,
                createUser: false
            };
            svc.verifyPLPLogin(params).then(vm.verifyPLPResponse);
        };

        vm.createUserInPLP = function () {
            svc.createPLPLogin().then(vm.verifyPLPResponse);
        };

        vm.verifyPLPResponse = function (response) {
            if (response.isError) {
                if (response.errorCode === "ProductLearningPortal.PLPUrl.5" ||
                    response.errorCode === "ProductLearningPortal.PLPUrl.4") {
                    vm.showNextModal();
                }
            }
            else {
                $scope.$hide();
                $window.open(response.data.url, "plptab");
            }
        };

        vm.showNextModal = function () {
            modalStates.showModal(modalStates.getNextModal());
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.modalStates.reset();
            vm.modalStates = undefined;
            vm.formModel.reset();
            vm.formModel = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("PlpModalCtrl", [
            "$scope",
            "$window",
            "alpModalStates",
            "accessLearningPortalSvc",
            "plpLoginFormConfig",
            "plpLoginUserModel",
            PlpModalCtrl
        ]);
})(angular);