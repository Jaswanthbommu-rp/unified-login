//  Products Controller

(function (angular, undefined) {
    "use strict";

    function ProductsCtrl($scope, $filter, timeout, pubsub, model, productsData, form, menuModel, formConfig, externalLinks, dashboard) {
        var vm = this;

        vm.init = function () {
            if (productsData.isReady()) {
                vm.setData();
                vm.productWatch = angular.noop;
            }
            else {
                vm.productWatch = productsData.subscribe(vm.setData);
            }

            vm.form = form;
            vm.model = model;
            vm.formConfig = formConfig;
            formConfig.setMethodsSrc(vm);
            vm.tabsMenu = menuModel.getMenu();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);

            vm.realpageMainUrl = externalLinks.realpageMain;
        };

        vm.onFamilyFilterChange = function () {
            form.prodSolution = "";
            timeout(vm.updateDisplay);
        };

        vm.onSolnFilterChange = function () {
            timeout(vm.updateDisplay);
        };

        vm.onSearchFilterChange = function () {
            timeout(vm.updateDisplay);
        };

        vm.updateDisplay = function () {
            model.updateDisplay(form);
        };

        vm.setData = function (data) {
            model.setFamilies(productsData.getFamilies());
            model.setSolutions(productsData.getSolutions());

            vm.solutionOptions = model.getSolutionFilterOptions();

            formConfig
                .setOptions("prodSolution", vm.solutionOptions)
                .setOptions("prodFamily", model.getFamilyFilterOptions());
        };

        vm.filterSolutionOptions = function (options) {
            if (!form.prodFamily) {
                return options;
            }

            var filter = {
                famId: form.prodFamily
            };

            return $filter("filter")(vm.solutionOptions, filter);
        };

        vm.destroy = function () {
            model.reset();
            vm.destWatch();
            vm.productWatch();
            vm.favChangeWatch();

            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductsCtrl", [
            "$scope",
            "$filter",
            "timeout",
            "pubsub",
            "productsModel",
            "productsDataModel",
            "productsFilterForm",
            "productsTabsMenuModel",
            "productsFilterFormConfig",
            "externalLinks",
            "dashboardModel",
            ProductsCtrl
        ]);
})(angular);
