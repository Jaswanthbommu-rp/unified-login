//  User Controller

(function (angular, undefined) {
    "use strict";

    function UserCtrl($scope, $location, $params, view, userTabs, tabsManager, session, pubsub, security, persona, chkEmailModel, productDataSync, panelTemplateModel) {
        var vm = this;

        vm.init = function () {
            vm.view = view;
            vm.security = security;
            vm.disableContent = false;
            vm.tabsList = userTabs.getTabsList();
            vm.tabsMenu = userTabs.getTabsMenu();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.profileWatch = pubsub.subscribe("up.user-details-disable", vm.setState);
        };

        // Actions

        vm.cancel = function () {
            if (session.getRealPageId() === $params.realPageId) {
                //$location.path("../home");
				window.location.href = "../home";
            }
            else {
                //$location.path("/people/users");
				window.location.href = "../people/users";
            }

        };

        vm.save = function () {
            if(!chkEmailModel.getIsBusy()){
                tabsManager.processData();
            }
        };

        vm.setState = function (value) {
            vm.disableContent = value;
        };
        // Assertions

        vm.hasAccess = function () {
            var allowed = true,
                calledFrom = $params.link;

            if (security.isAllowed("viewUser") && calledFrom !== "ManageProfile") {
                allowed = false;
            }

            if (security.isAllowed("viewUser") && security.isAllowed("editPassWord")) {
                allowed = true;
            }

            if (persona.hasViewOnlySupportToolAccess()){
                allowed = false;
            }

            return allowed;
        };

        vm.hasMultipleTabs = function () {
            return userTabs.hasMultipleTabs();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.profileWatch();
            tabsManager.reset();
            productDataSync.reset();
            panelTemplateModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UserCtrl", [
            "$scope",
            "$location",
            "$stateParams",
            "userViewModel",
            "userTabsModel",
            "userTabsManager",
            "userSessionModel",
            "pubsub",
            "routeSecurity",
            "personaDetails",
            "chkEmailModel",
            "productDataSyncManager",
            "productTemplateModel",
            UserCtrl
        ]);
})(angular);
