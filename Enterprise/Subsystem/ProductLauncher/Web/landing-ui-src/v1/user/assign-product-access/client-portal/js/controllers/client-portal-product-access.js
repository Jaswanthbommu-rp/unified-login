//  VendCompProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function ClientPortalProductAccessCtrl($scope, $filter, tabsMenu, tabsData, ClientPortalProductModel, pubsub) {
        var vm = this;

        vm.init = function() {
            vm.panelName = $filter("productPanelText")("panelName.clientPortal");
            vm.productDisabled = false;
            vm.productAccessWatch = pubsub.subscribe("pa.regUserNoEmailNotAllowed", vm.setProductDisabled);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };


        vm.isActive = function () {
            return ClientPortalProductModel.isActive();
        };

        vm.setChanged = function () {
            ClientPortalProductModel.setChanged();
        };

        vm.setProductDisabled = function (value) {
            vm.productDisabled = value;
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.productAccessWatch();
            vm.productDisabled = undefined;
            tabsData.reset();
            ClientPortalProductModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ClientPortalProductAccessCtrl", [
            "$scope",
            "$filter",
            "clientPortalTabsMenu",
            "clientPortalTabsData",
            "clientPortalDataModel",
            "pubsub",
            ClientPortalProductAccessCtrl
        ]);
})(angular);
