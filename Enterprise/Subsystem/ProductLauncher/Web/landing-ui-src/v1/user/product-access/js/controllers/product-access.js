//  Product Access Controller

(function (angular, undefined) {
    "use strict";

    function ProductAccessCtrl($scope, $params, model, timeout, tabs, modal, accessNotDefinedModel, helpData, security, productPanelModel) {
        var vm = this;

        vm.init = function () {
           // vm.register();
            vm.security = security;
            vm.disableProductTab = false;

            if (productPanelModel.isReady()) {
                vm.register();
            }
            else {
                vm.productPanelWatch = productPanelModel.subscribe(vm.register);
            }
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        // Actions
        vm.editingSelf = function () {
            return model.editingSelf();
        };

        vm.isUserWithViewEditPwdRightOnly = function () {
            return security.isAllowed("viewUser") && security.isAllowed("editPassWord");
        };

        vm.hasNoAccessToProducts = function (){
            return vm.editingSelf() && !security.isAllowed("viewUser") ;
        };

        vm.onTabActive = function () {
            var helpWidget = document.querySelector('omnibar-unified-help');
            helpWidget.helpQuery = 'pg=ul-productAccess&vr=40&scrver=350';
            vm.active = true;
            model.setActive();
            return vm;
        };

        vm.onTabInactive = function () {
            vm.active = false;
            return vm;
        };

        vm.register = function () {
            tabs.register({
                ctrl: vm,
                name: "productAccess"
            });
        };

        vm.showErrors = function () {
            modal.show();
            accessNotDefinedModel.setList(model.getIncompleteSolutionsList());
        };

        // Assertions

        vm.isDirty = function () {

        };

        vm.isValid = function () {
            return model.isValid();
        };

        vm.hasSaveFn = function () {
            return false;
        };

        // Destroy

        vm.destroy = function () {
            model.reset();
            vm.destWatch();
            tabs.remove("productAccess");
            vm.productPanelWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductAccessCtrl", [
            "$scope",
            "$stateParams",
            "productAccessModel",
            "timeout",
            "userTabsManager",
            "accessNotDefinedModal",
            "accessNotDefinedModel",
            "rpGhHelpData",
            "routeSecurity",
            "productTemplateModel",
            ProductAccessCtrl
        ]);
})(angular);
