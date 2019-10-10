//  integMkt ProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function IntegMktProductAccess($scope, model, tabsMenu, tabsData, IMDataModel) {
        var vm = this;
        var panelName = "IntegMkt";

        vm.init = function() {
            vm.panelName = panelName;
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function() {
            return IMDataModel.isActive();
        };

        vm.setChanged = function() {
            IMDataModel.setChanged();
        };

        vm.destroy = function() {
            model.reset();
            tabsData.reset();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("IntegMktProductAccess", [
            "$scope",
            "integMktSideMenuConfig",
            "integMktTabsMenu",
            "integMktTabsData",
            "integMktDataModel",
            IntegMktProductAccess
        ]);
})(angular);