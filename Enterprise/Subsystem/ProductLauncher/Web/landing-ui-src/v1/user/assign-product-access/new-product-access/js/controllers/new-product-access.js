//  Access Not Defined Controller

(function (angular, undefined) {
    "use strict";

    function NewProductAccessCtrl($scope, model, data, tabsMenu) {
        var vm = this;

        vm.init = function () {
            console.log("controller init");
            vm.model = model;
            vm.model.setData(data);
            console.log("tabsMenu", tabsMenu);
            vm.pageTitle = vm.model.getPageTitle();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.destroy = function () {
            model.reset();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("NewProductAccessCtrl", [
            "$scope",            
            "newProductAccessModel",
            "DataModelNew",
            "tabsMenu",
            NewProductAccessCtrl
        ]);
})(angular);
