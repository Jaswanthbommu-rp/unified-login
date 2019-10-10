//  Validate Controller

(function (angular) {
    "use strict";

    function ValidateTokenCtrl($scope, $state, $stateParams, $filter, userModel, validateTokenSvc) {
        var vm = this;

        vm.init = function () {
        	userModel.reset();

        	vm.userModel = userModel;
        	vm.errorMsg = "";

        	if (vm.checkUserParams()) {
	        	validateTokenSvc.validate($stateParams)
	                .then(vm.setData, vm.displayError);
        	} else {
        		vm.displayError();
        	}

        	vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.checkUserParams = function () {
        	if ($stateParams.validateUserToken && $stateParams.enterpriseUserName) {
        		return true;
        	} else {
        		return false;
        	}
        };

        vm.setData = function (data) {
            userModel.setTokenData(data);
            if (userModel.checkUserToken()) {
	        	userModel.setEnterpriseUserName(data.enterpriseUserName);
	        	userModel.setActivityToken(data.validateUserToken);
	        	userModel.setIsValidated(true);

	        	vm.redirectToSetPassword();
        	} else {
        		userModel.setIsValidated(false);

        		if (userModel.isDataError()) {
        		    vm.errorMsg = userModel.getErrorReason();
        		} else {
        			vm.displayError();
        		}
        	}
        };

        vm.displayError = function () {
            vm.errorMsg = $filter("validationTokenText")("system_err_contact_admin");
        };

        vm.redirectToSetPassword = function () {
        	$state.go("set-password", $stateParams, { location: "replace" });
        };

        vm.destroy = function () {
            vm.destWatch();
        	vm = undefined;
        };

        vm.init();
    }

    angular
        .module("new-user")
        .controller("ValidateTokenCtrl", [
        	"$scope",
        	"$state",
        	"$stateParams",
            "$filter",
        	"userModel",
        	"validateTokenSvc",
        	ValidateTokenCtrl
        ]);
})(angular);
