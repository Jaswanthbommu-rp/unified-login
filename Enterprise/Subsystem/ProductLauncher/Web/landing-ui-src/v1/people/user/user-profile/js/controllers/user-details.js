//  Add User Controller

(function (angular) {
    "use strict";

    function ManageUserDetailsCtrl($scope, $filter, $tooltip, $popover, $document,
            userFormData, userFormState, userDetailsFormConfig, 
            userTypes, permissionsModel, rpWatchList, newUserPasswordState, moment) {
        var vm = this;

        vm.init = function () {
            // logc("INIT: User Details");
            vm.watchList = rpWatchList();
            vm.watchList.add($scope.$on("$destroy", vm.destroy));

            vm.initWatch = $scope.$on("rpInit:userDetails", vm.initUserDetails);

            userDetailsFormConfig.setMethodsSrc(vm);

            vm.userData = userFormData;
            vm.formConfig = userDetailsFormConfig;
            vm.formState = userFormState.state;
            vm.formState.allowUserSwitch = true;

            //password validation
            vm.passwordValid = false;
            vm.confirmPasswordValid = false;
            vm.formState.password = newUserPasswordState.init();

            vm.updateBasedOnUserType(userTypes.REGULAR.id);
        };

        vm.initUserDetails = function() {
            vm.destroyInitWatch();
            vm.updateBasedOnUserType(userFormData.userType);
        };

        vm.onStartDateChange = function(startDate) {
            if(startDate) {
                userDetailsFormConfig.endDate.minDate(startDate);

                var dateToday = moment();
                if(startDate.isAfter(dateToday, "day")) { //date set is in the future
                    vm.toggleIsEnabledFormAccess(false);
                    userFormData.isEnabled = true;                    
                } else {
                    vm.toggleIsEnabledFormAccess(true);
                }

            }
        };

        vm.onEndDateChange = function(endDate) {
            if(endDate) {
                userDetailsFormConfig.startDate.maxDate(endDate);

                var dateToday = moment();
                if(endDate.isSameOrBefore(dateToday, "day")) { //date set is today or in the past
                    vm.toggleIsEnabledFormAccess(false);
                    userFormData.isEnabled = true;                    
                } else {
                    vm.toggleIsEnabledFormAccess(true);
                }
            }
        };    


        vm.addPersona = function() {
            $scope.$emit("rpManageUserAdditionalPersona");
        };

        vm.toggleIsEnabledFormAccess = function(flag) {
            var isEnabled = flag;
            if(flag === undefined) {
                isEnabled = !userFormState.state.allowUserSwitch;
            }

            userFormState.state.allowUserSwitch = isEnabled;
        };

        vm.updateBasedOnUserType = function(val) {
            var permissions = permissionsModel.get(val);

            vm.updatePasswordRequirement(val);
            vm.updateUsernameLabel(permissions.hasUsernameOnly);
            vm.updateAddPersonaDisplay(permissions.hasAddPersonaBtn);
            vm.updateEnterpriseRoleDisplay(permissions.hasEnterpriseRole);
            vm.updateNotificatonEmailDisplay(permissions.hasNotificationEmail);
        };

        vm.updatePasswordRequirement = function(val) {
            if(val === 404){
                userDetailsFormConfig.updatePasswordRequired("password", true);
                userDetailsFormConfig.updatePasswordRequired("confirmPassword", true);
            }else{
                userDetailsFormConfig.updatePasswordRequired("password", false);
                userDetailsFormConfig.updatePasswordRequired("confirmPassword", false);
            }
        };

        vm.updateUsernameLabel = function(isUsernameOnly) {
            if(isUsernameOnly) {
                userDetailsFormConfig.changeLabel("username", $filter("userDetailsText")("user_detail_username"), $filter("userDetailsText")("err_username_required"));
            } else {
                userDetailsFormConfig.changeLabel("username", $filter("userDetailsText")("username_email"), $filter("userDetailsText")("err_useremail_required"));
            }
        };

        vm.updateAddPersonaDisplay = function(flag) {
            userDetailsFormConfig.setVisibility("addPersona", flag);
        };

        vm.updateEnterpriseRoleDisplay = function(flag) {
            userDetailsFormConfig.setVisibility("enterpriseRole", flag);
        };

        vm.updateNotificatonEmailDisplay = function(flag) {
            userDetailsFormConfig.setVisibility("notificationEmail", flag);
        };

        vm.validatePassword = function(value) {
            if((value === null || value === "")) { //no password is acceptable, return true. except when user type is reg user (no email)
                newUserPasswordState.isPasswordValid(value); //still validate to update checklist
                vm.userData.confirmPassword = null; //added so the confirmPassword will remain not validated
                vm.passwordValid = true;
            } else {
                if (value !== "" && vm.userData.confirmPassword === null) {
                    vm.userData.confirmPassword = null; //added so the confirmPassword will remain not validated
                } else {
                    vm.userData.confirmPassword = ""; //added so the confirmPassword will validate
                }
                vm.passwordValid = newUserPasswordState.isPasswordValid(value);    
            }

            return vm.passwordValid;
        };

        vm.checkUsernamePassword = function (value) {
            if((value !== null && value !== "") && value === vm.userData.username){
                vm.passwordValid = false;
            }
            return vm.passwordValid;
        };

        vm.matchPassword = function(value) {
            var passwordVal = userFormData.password;

            if(passwordVal && passwordVal === value) {
                vm.confirmPasswordValid = true;
            } else if(!passwordVal && !value || (passwordVal && !value)) { //not required if password is not set
                vm.confirmPasswordValid = true;
            } else {
                vm.confirmPasswordValid = false;
            }

            return vm.confirmPasswordValid;
        };
        
        vm.destroyInitWatch = function() {
            if(vm.initWatch) {
                vm.initWatch();
                vm.initWatch = undefined;
            }
        };

        vm.destroy = function () {
            vm.watchList.destroy();
            vm.watchList = undefined;

            vm.destroyInitWatch();

            vm.userData = undefined;
            vm.formConfig = undefined;
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
            "$tooltip",
            "$popover",
            "$document",
            "manageUserFormData",
            "manageUserFormState",
            "userDetailsFormConfig",
            "userTypes",
            "manageUserPermissionsModel",
            "rpWatchList",
            "newUserPasswordState",
            "moment",
            ManageUserDetailsCtrl
        ]);
})(angular);
