//  Persona Component

(function (angular) {
    "use strict";

    function PersonaComponent($scope, productPanelConfig, personaFormConfig, personaProducts, userFormState) {
        var ctrl = this;

        ctrl.$onInit = function() {
            // logc("INIT: Persona Tab %s", ctrl.model.data.tabID);
            ctrl.updateWatch = $scope.$on("rpUpdate:personas", ctrl.updatePersona);

            ctrl.persona = angular.copy(ctrl.model);
            ctrl.isDisplayDetails = angular.copy(ctrl.displayDetails);

            ctrl.productPanelState = [0, 1]; //by default open all panels
            ctrl.productPanelConfig = productPanelConfig;

            ctrl.formConfig = personaFormConfig;
            ctrl.formConfig.setMethodsSrc(ctrl);

            ctrl.formState = userFormState.state;
            ctrl.form = null;
        };

        ctrl.$onChanges = function(model) {
            if(model.displayDetails) {
                ctrl.isDisplayDetails = angular.copy(ctrl.displayDetails);
            }
            if(model.model) {
                ctrl.persona = angular.copy(ctrl.model);
            }
        };

        ctrl.$onDestroy = function() {
            // logc("DESTROY: Persona Tab %s", ctrl.model.data.tabID);

            if(ctrl.form.$dirty) {
                ctrl.updatePersona();
                personaProducts.reset();
            }

            if(ctrl.updateWatch) {
                ctrl.updateWatch();
                ctrl.updateWatch = undefined;
            }

            ctrl.formConfig = undefined;
            ctrl.persona = undefined;
            ctrl.productPanelState = undefined;
            ctrl.productPanelConfig = undefined;

            ctrl = undefined;
            // logc("--------------");
        };

        ctrl.isActivePanel = function(panelIndex) {
            return ctrl.productPanelState.indexOf(panelIndex) != -1;
        };

        ctrl.updatePersona = function() {
            // logc(personaProducts.data.productList);
            // logc(personaProducts.data.families);

            ctrl.onUpdatePersona({
                $event: {
                    persona: ctrl.persona
                }
            });
        };

        ctrl.onStartDateChange = function(date) {
            if(date) {
                personaFormConfig.endDate.minDate(date);
            }
        };

        ctrl.onEndDateChange = function(date) {
            if(date) {
                personaFormConfig.startDate.maxDate(date);
            }
        };

        ctrl.onPersonaNameChange = function(currentValue) {
            ctrl.onUpdateTabName({
                $event: {
                    tabName: currentValue
                }
            });
        };

    }

    angular
        .module("settings")
        .component("rpPersona", {
            templateUrl: "people/user/persona/base/templates/persona-tab.html",
            bindings: {
                model: "<",
                isReadOnly: "<",
                displayDetails: "<",
                onUpdateTabName: "&",
                onUpdatePersona: "&"
            },
            controller: [
                "$scope",
                "productPanelConfig",
                "managePersonaFormConfig",
                "personaProducts",
                "manageUserFormState",
                PersonaComponent,
                
            ]
        });
        
})(angular);
