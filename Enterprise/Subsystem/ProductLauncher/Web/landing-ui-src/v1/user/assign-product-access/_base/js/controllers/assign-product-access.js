//  Assign Product Access Controller

(function (angular, undefined) {
    "use strict";

    function AssignProductAccessCtrl($scope, model, templates, productModelsSvc, productTemplateModel) {
        var vm = this;

        vm.init = function () {
            vm.active = {};
            vm.list = templates.getList();
            if (productTemplateModel.isReady()) {
                vm.registerModels();
            }
            else {
                vm.productPanelWatch = productTemplateModel.subscribe(vm.registerModels);
            }
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.registerModels = function () {
            var listModels = productModelsSvc.getProductModels();
            //logc("listModels", listModels);
            angular.forEach(listModels, function (data) {
              model.register(data);
            });
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
        .controller("AssignProductAccessCtrl", [
            "$scope",
            "assignProductAccessModel",
            "productAccessTemplates",
            "registerProductModels",
            "productTemplateModel",
            AssignProductAccessCtrl
        ]);
})(angular);
