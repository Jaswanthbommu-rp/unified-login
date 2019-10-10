//  Validate Controller

(function (angular) {
    "use strict";

    function ValidateCtrl($scope, $state, $stateParams, $filter, userModel, validateUserSvc) {
        var vm = this;

        vm.init = function () {
        	userModel.reset();

        	vm.userModel = userModel;
        	vm.errorMsg = "";

        	if (vm.checkUserParams()) {
	        	validateUserSvc.validateUser($stateParams)
	                .then(vm.setData, vm.displayError);
        	} else {
        		vm.displayError();
        	}

        	vm.validateCtrlWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.checkUserParams = function () {
        	if ($stateParams.newUserRegistrationToken && $stateParams.enterpriseUserName) {
        		return true;
        	} else {
        		return false;
        	}
        };

        vm.setData = function (data) {
        	if (data.validateUserToken && data.isError === false) {
	        	userModel.setEnterpriseUserName(data.enterpriseUserName);
	        	userModel.setActivityToken(data.validateUserToken);
	        	userModel.setIsValidated(true);

	        	vm.redirectToSetPassword();
        	} else {
        		userModel.setIsValidated(false);

        		if (data.isError === true && data.errorReason) {
        			vm.errorMsg = data.errorReason;
        		} else {
        			vm.displayError();
        		}
        	}
        };

        vm.displayError = function () {
            vm.errorMsg = $filter("validationText")("system_err_contact_admin");
        };

        vm.redirectToSetPassword = function () {
        	$state.go("set-password", $stateParams, { location: "replace" });
        };

        vm.destroy = function () {
        	vm.validateCtrlWatch();
        	vm = undefined;
        };

        vm.init();
    }

    angular
        .module("new-user")
        .controller("ValidateCtrl", [
        	"$scope",
        	"$state",
        	"$stateParams",
            "$filter",
        	"userModel",
        	"validateUserSvc",
            "appLangTranslate",
        	ValidateCtrl
        ]);
})(angular);
