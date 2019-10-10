//  Change User Type Confirmation Controller

(function (angular, undefined) {
    "use strict";

    function ChangeUserTypeConfirmationCtrl($scope, model) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.confirm = function () {
            model.publish({
            	status: "confirmed"
            });
        };

        vm.confirmDemoteAdmin = function () {
            model.publish({
                status: "confirmedDemoteAdmin"
            });
        };

        vm.confirmPromoteRegular = function () {
            model.publish({
                status: "confirmedPromoteRegular"
            });
        };

        vm.confirmRegularWithEmail = function () {
            model.publish({
                status: "confirmedRegularWithEmail"
            });
        };

        vm.confirmExternalWithEmail = function () {
            model.publish({
                status: "confirmedExternalWithEmail"
            });
        };

        vm.confirmPromoteExternal = function () {
            model.publish({
                status: "confirmedPromoteExternal"
            });
        };

        vm.confirmDemoteAdminToExternal = function () {
            model.publish({
                status: "confirmedDemoteAdminToExternal"
            });
        };

        vm.confirmRegularExternal = function () {
            model.publish({
                status: "confirmedRegularExternal"
            });
        };        
        

        vm.decline = function () {
            model.publish({
            	status: "declined"
            });
        };

        vm.chgSuperToRegular = function () {
        	return model.chgSuperToRegular();
        };

        vm.chgToNoEmail = function () {
        	return model.chgToNoEmail();
        };

        vm.chgRegularToSuper = function () {
        	return model.chgRegularToSuper();
        };

        vm.chgNoEmailToRegular = function () {
        	return model.chgNoEmailToRegular();
        };

        vm.chgNoEmailToExternal = function () {
            return model.chgNoEmailToExternal();
        };        

        vm.chgNoEmailToSuper = function () {
            return model.chgNoEmailToSuper();
        };

        vm.chgExternalToSuper = function () {
            return model.chgExternalToSuper();
        };

        vm.chgExternalToRegular = function () {
            return model.chgExternalToRegular();
        };

        vm.chgSuperToExternal = function () {
            return model.chgSuperToExternal();
        };
        
        vm.chgRegularToExternal = function () {
            return model.chgRegularToExternal();
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
        .controller("ChangeUserTypeConfirmationCtrl", ["$scope", "changeUserTypeConfirmationModel", ChangeUserTypeConfirmationCtrl]);
})(angular);
