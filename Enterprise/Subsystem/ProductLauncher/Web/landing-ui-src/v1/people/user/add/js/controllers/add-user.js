//  Add User Controller

(function (angular) {
    "use strict";

    function AddUserCtrl($scope, $location, $filter, $window, userFormData, userFormModel, userFormState,
            manageUserModel, userStates, addUserSvc, rpWatchList, notifsSvc, personaScrollTabs, personaModel, productsOverallData) {
        var vm = this;

        vm.init = function () {
            vm.watchList = rpWatchList();
            vm.watchList.add($scope.$on("$destroy", vm.destroy));
            vm.additionalPersonaWatch = $scope.$on("rpManageUserAdditionalPersona", vm.additionalPersona);
            
            manageUserModel.setState(userStates.ADD_USER);
            userFormModel.setData(userFormData);

            vm.formModel = userFormModel;
            vm.formState = userFormState.state;

            vm.personaScrollConfig = personaScrollTabs.init();
            //vm.personaScrollConfig.subscribe("change", vm.switchPersona);
            vm.activePersona = null;

            vm.personas = userFormData.personas;

            vm.initUserDetailsPanel();
            vm.initPersona();
        };

        vm.initUserDetailsPanel = function() {
            vm.userDetailsPanelState = 0; //set to active            
            $scope.$broadcast("rpInit:userDetails");
        };

        vm.additionalPersona = function (evt) {
            evt.stopPropagation();
            $scope.$broadcast("rpAdditionalPersona");

            vm.destroyPersonaWatch();
        };

        vm.initPersona = function() {
            var tabConfig = personaScrollTabs.addNewTab(true, $filter("addUserText")("default_persona_name")),
                persona = personaModel({
                    tabID: tabConfig.id,
                    name: tabConfig.name,
                    defaultName: tabConfig.name
                });

            userFormModel.addPersona(persona);
            vm.activePersona = userFormModel.getPersona(tabConfig.id);
        };

        vm.createUser = function () {
            var form = userFormState.state.userDetailsForm;
                form.confirmPassword.$validate();

            if (form.$valid) {
                userFormState.setIsLoading();
                angular.extend(userFormData,productsOverallData.getData());
                addUserSvc.createNewUser(userFormData)
                    .then(vm.processAddUserForm)
                    .finally(vm.enableSubmitBtn);
            } else {
                form.$setSubmitted();
            }
        };

        vm.cancel = function () {
            //TODO add checker are you sure? 
            $location.path(manageUserModel.getRedirectLink());
        };

        vm.processAddUserForm = function (response) {
            if (response.isError) {
                notifsSvc.notify({
                    text: response.errorReason,
                    buttons: {
                        sticker: false
                    },
                    type: "error"
                });
            }
            else {
                var notice = notifsSvc.notify({
                    text: response.userStatus,
                    buttons: {
                        sticker: false
                    },
                    type: "success"
                });

                //display linking of products
                userFormState.showProductsLinking();
            }
        };

        vm.enableSubmitBtn = function () {
            userFormState.setIsLoading(false);
        };

        vm.destroyPersonaWatch = function () {
            if (vm.additionalPersonaWatch) {
                vm.additionalPersonaWatch();
                vm.additionalPersonaWatch = undefined;
            }
        };

        vm.destroy = function () {
            vm.watchList.destroy();
            vm.watchList = undefined;

            vm.destroyPersonaWatch();

            userFormModel.reset();
            userFormState.reset();

            vm.formState = undefined;
            vm = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AddUserCtrl", [
            "$scope",
            "$location",
            "$filter",
            "$window",
            "manageUserFormData",
            "manageUserFormModel",
            "manageUserFormState",
            "manageUserModel",
            "userStates",
            "addUserSvc",
            "rpWatchList",
            "notificationService",
            "personaScrollTabs",
            "personaModel",
            "productsOverallDataModel",
            AddUserCtrl
        ]);
})(angular);
