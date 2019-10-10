//  Add User Controller

(function (angular) {
    "use strict";

    function ManageUserDetailsCtrl($scope, $filter, userFormData, userFormModel, userFormState, userDetailsFormConfig, userTypes, rpWatchList) {
        var vm = this;

        vm.init = function () {
            vm.watchList = rpWatchList();
            vm.watchList.add($scope.$on("$destroy", vm.destroy));

            userDetailsFormConfig.setMethodsSrc(vm);

            vm.userData = userFormData;
            vm.formConfig = userDetailsFormConfig;
            vm.formModel = userFormModel;
            vm.formState = userFormState.state;
        };

        vm.onStartDateChange = function(date) {
            if(date) {
                userDetailsFormConfig.endDate.minDate(date);
            }
        };

        vm.onEndDateChange = function(date) {
            if(date) {
                userDetailsFormConfig.startDate.maxDate(date);
            }
        };        

        vm.updateBasedOnUserType = function(val) {
            vm.updateUsernameLabel(val);

            //TODO update for other user types.
        };

        vm.updateUsernameLabel = function(userType) {
            if(userType == 2 || userType == 3) {
                vm.formConfig.changeLabel("username", $filter("userDetailsText")("user_detail_username"));
            } else {
                vm.formConfig.changeLabel("username", $filter("userDetailsText")("username_email"));
            }
        };

        vm.addPersona = function() {
            $scope.$emit("rpManageUserAdditionalPersona");
        };


        vm.destroy = function () {
            vm.watchList.destroy();
            vm.watchList = undefined;

            vm.userData = undefined;
            vm.formConfig = undefined;
            vm.formModel = undefined;
            vm.formState = undefined;
            vm = undefined;
        };

        

        vm.init();
    }

    angular
        .module("settings")
        .controller("ManageUserDetailsCtrl", [
            "$scope",
            "$filter",
            "manageUserFormData",
            "manageUserFormModel",
            "manageUserFormState",
            "userDetailsFormConfig",
            "userTypes",
            "rpWatchList",
            ManageUserDetailsCtrl
        ]);
})(angular);
