(function (angular) {
    "use strict";

    function UserLookupCtrl($scope, $filter, userLookupModel, userLookupSvc, userModel, layoutModel) {
    	var vm = this;

    	vm.init = function () {
            userModel.reset();
            vm.model = userLookupModel;
            vm.userLookupCtrlWatch = $scope.$on("$destroy", vm.destroy);
    	};

        vm.submit = function (form) {
            userLookupModel.clearError();
            
            if (form.$valid) {
                userLookupModel.setSubmitBtnDisabled();
                userLookupSvc.lookupUser(userLookupModel.form.username)
                    .then(vm.setData, vm.displayError)
                    .finally(vm.setSubmitBtnEnabled);
            } else {
                form.$setSubmitted();
            }
        };

        vm.setSubmitBtnEnabled = function () {
            userLookupModel.setSubmitBtnDisabled(false);
        };

        vm.displayError = function (error) {
            userLookupModel.setError(
                $filter("userLookupText")("lookup_system_err_contact_admin"));
        };

        vm.setData = function (data) {
            if (data.isUserExist === true && data.isError === false) {
                userModel.setEnterpriseLoginName(data.enterpriseUserName);
                userModel.setActivityToken(data.activityToken);
                userModel.setSecurityQuestions(data.securityQuestions);
                vm.redirectToForgotPassword();
            } else if (data.isUserExist === false || data.isError === true) {
                userLookupModel.setError(data.errorReason);
            }
        };

        vm.redirectToForgotPassword = function () {
            layoutModel.setActiveState("forgot-password");
        };

        vm.destroy = function () {
            vm.userLookupCtrlWatch();
            vm.model.reset();
            vm = undefined;
        };

    	vm.init();
    }

    angular
    	.module("identity")
    	.controller("UserLookupCtrl", [
            "$scope",
            "$filter",
            "userLookupModel",
            "userLookupSvc",
            "userModel",
            "layoutModel",
            "appLangTranslate",
    		UserLookupCtrl
    	]);
})(angular);