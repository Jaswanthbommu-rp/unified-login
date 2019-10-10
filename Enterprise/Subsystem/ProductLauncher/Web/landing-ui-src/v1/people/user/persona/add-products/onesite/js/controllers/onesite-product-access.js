//  OnesiteProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function OnesiteProductAccess($scope, model, tabsMenu, tabsData, OSDataModel) {
        var vm = this;
        var panelName = "OneSite";

        vm.init = function() {
            vm.panelName = panelName;
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.destroy = function() {
            model.reset();
            tabsData.reset();
            vm.destWatch();
            OSDataModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("OnesiteProductAccess", [
            "$scope",
            "sideMenuConfig",
            "osTabsMenu",
            "osTabsData",
            "OnesiteDataModel",
            OnesiteProductAccess
        ]);
})(angular);
