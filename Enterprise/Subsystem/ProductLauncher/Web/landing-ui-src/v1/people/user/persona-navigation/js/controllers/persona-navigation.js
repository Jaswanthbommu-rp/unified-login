
//  Persona Navigation Controller

(function (angular) {
    "use strict";

    function PersonaNavigationCtrl($scope, $filter, moment, userStates, manageUserModel,
            userFormData, userFormModel, userFormState, personaScrollTabs, personaModel) {

        var vm = this;

        vm.init = function() {
            vm.destroyWatch = $scope.$on("$destroy", vm.destroy);
            vm.additionalPersonaWatch = $scope.$on("rpAdditionalPersona", vm.additionalPersona);
            vm.initPersonaWatch = $scope.$on("rpInit:personaList", vm.initPersonas);

            vm.formState = userFormState.state;
            vm.personas = userFormData.personas;

            vm.personaScrollConfig = personaScrollTabs.init();
            vm.personaScrollConfig.subscribe("change", vm.switchPersona);
            vm.activePersona = null;

            vm.pageState = manageUserModel.getState();
            vm.isReadOnly = (vm.pageState == userStates.VIEW_USER);
        };

        vm.initPersonas = function(evt, personas) {
            vm.destroyInitPersonaWatch();
            angular.forEach(personas, vm.addExistingPersona);
        };

        vm.additionalPersona = function() {
            vm.destroyAddPersonaWatch();

            if(vm.pageState == userStates.ADD_USER) { //assign default tab for add user
                vm.addNewPersona($filter("personaNavigationText")("initial_persona_name"));    
            }
            
            vm.addNewPersona();

            userFormState.setHasAddedPersona(true);
        };

        vm.addNewPersona = function(label) {
            var tabConfig = personaScrollTabs.addNewTab(false, label),
                persona = personaModel({
                    tabID: tabConfig.id,
                    name: tabConfig.text,
                    defaultName: tabConfig.text,

                    startDate: moment()
                });

            userFormModel.addPersona(persona);
            personaScrollTabs.activateTab(tabConfig);
        };

        vm.addExistingPersona = function(persona) {
            var tabConfig = personaScrollTabs.addNewTab(false, persona.getName());

            persona.setData({
                tabID: tabConfig.id,
                defaultName: persona.getName()
            });
            
            userFormModel.addPersona(persona);
            personaScrollTabs.activateTab(tabConfig);
        };

        vm.switchPersona = function(tab) {
            vm.activePersona = userFormModel.getPersona(tab.id);
        };

        vm.updateTabName = function(evt) {
            //TODO update tab name
            // logc("udpateTabName: %s", event.tabName);
        };

        vm.updatePersona = function(evt) {
            if(evt.persona) {
                var lastPersona = userFormModel.getPersona(evt.persona.data.tabID);
                lastPersona.setData(evt.persona.data);
                lastPersona.setDirty(true);
            }
        };

        vm.destroyInitPersonaWatch = function() {
            if(vm.initPersonaWatch) {
                vm.initPersonaWatch();
                vm.initPersonaWatch = undefined;
            }
        };

        vm.destroyAddPersonaWatch = function() {
            if(vm.additionalPersonaWatch) {
                vm.additionalPersonaWatch();
                vm.additionalPersonaWatch = undefined;
            }
        };

        vm.destroy = function() {
            vm.destroyWatch();
            vm.destroyWatch = undefined;

            vm.destroyInitPersonaWatch();
            vm.destroyAddPersonaWatch();
            personaScrollTabs.destroy();
            
            vm.personas = undefined;
            vm.formState = undefined;
            vm.personaScrollConfig = undefined;
            vm.activePersona = undefined;
            vm = undefined;
        };


        vm.init();
    }

    angular
        .module("settings")
        .controller("PersonaNavigationCtrl", [
            "$scope",
            "$filter",
            "moment",
            "userStates",
            "manageUserModel",
            "manageUserFormData",
            "manageUserFormModel",
            "manageUserFormState",
            "personaScrollTabs",
            "personaModel",
            PersonaNavigationCtrl
        ]);
})(angular);