//   Product Learning Portal Modal Controller

(function (angular, undefined) {
    "use strict";

    function ELMSModalCtrl($scope, $window, modalStates, svc, formConfig, formModel) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.modalStates = modalStates;
            vm.formConfig = formConfig;
            vm.formModel = formModel;
        };

        vm.submitEmailForm = function () {
            if ($scope.elmsEmailForm.$valid) {
                vm.checkUserStatus();
            }
            else {
                vm.showFormError();
            }
        };

        vm.showFormError = function () {
            $scope.elmsEmailForm.$setSubmitted();
        };

        vm.checkUserStatus = function () {
            var params = {
                userName: formModel.data.userEmailAddress,
                createUser: false
            };
            svc.verifyELMSLogin(params).then(vm.verifyELMSResponse);
        };

        vm.contactAdminModal = function () {
            modalStates.showModal("contactAdmin");
        };

        vm.verifyELMSResponse = function (response) {
            if (response.isError) {
                if (response.errorCode === "ProductEasyLMS.PELMSUrl.6" ||
                    response.errorCode === "ProductEasyLMS.PELMSUrl.8") {
                    vm.showNextModal();
                }
            }
            else {
                $scope.$hide();
                $window.open(response.data.url, "elmstab");
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
        .controller("ELMSModalCtrl", [
            "$scope",
            "$window",
            "elmsModalStates",
            "accessEasyLMSSvc",
            "elmsLoginFormConfig",
            "elmsLoginUserModel",
            ELMSModalCtrl
        ]);
})(angular);
