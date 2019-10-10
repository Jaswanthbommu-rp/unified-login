//  Manage Profile Tab Manager Service

(function (angular, undefined) {
    "use strict";

    function MpTabsManager($rootScope, timeout, pubsub, tabsData, tabsMenu, aside, userProfileModel, session, dashboard) {
        var svc = this,
            triggerID = "mpTabsManager";

        svc.tabs = {};
        svc.activeTabs = {};

        svc.init = function () {
            svc.initActiveTab();
            svc.reqCount = 0;
            svc.reqErrorCount = 0;
            svc.reqSuccessCount = 0;

            svc.updateWatch = pubsub.subscribe("mp.update", svc.onUpdate);
            svc.cancelWatch = pubsub.subscribe("mp.cancel", svc.onCancel);
            svc.tabWatch = tabsMenu.subscribe("change", svc.initActiveTab);
            svc.appWatch = $rootScope.$on("rpAppStateChange", svc.onAppStateChange);
        };

        // Getters

        svc.getTabState = function (id) {
            return tabsData.getTabById(id);
        };

        // Assertions

        svc.allTabsAreValid = function () {
            var count = 0;

            angular.forEach(svc.tabs, function (item, id) {
                if (item.ctrl.isDirty() && !item.ctrl.isValid()) {
                    count++;
                }
            });

            return count === 0;
        };

        // Actions

        svc.focusErrorTab = function () {
            var found = false;

            angular.forEach(svc.tabs, function (item, id) {
                if (!found && item.ctrl.hasSaveError()) {
                    found = true;
                    svc.focusTab(item, id);
                }
            });

            return svc;
        };

        svc.focusInvalidTab = function () {
            var found = false;

            angular.forEach(svc.tabs, function (item, id) {
                if (!found && !item.ctrl.isValid() && item.ctrl.isDirty()) {
                    found = true;
                    svc.focusTab(item, id);
                    timeout(item.ctrl.focusInvalidField, 50);
                }
            });
        };

        svc.focusTab = function (item, id) {
            var tab = tabsData.getTabById(id);
            tabsMenu.activate(tab);
            item.ctrl.setSubmitted();
        };

        svc.initActiveTab = function () {
            var ctrl,
                id = tabsData.getActiveTab().id;

            if (!svc.activeTabs[id]) {
                svc.activeTabs[id] = 1;

                if (svc.tabs[id].ctrl.onTabActive) {
                    svc.tabs[id].ctrl.onTabActive();
                }
            }
        };

        svc.onAppStateChange = function (ev, evData) {
            if (!ev.defaultPrevented && evData.triggerID == triggerID) {
                evData.onContinue();
            }
        };

        svc.onCancel = function () {
            $rootScope.$emit("rpAppStateChange", {
                triggerID: triggerID,
                onContinue: svc.onCancelContinue
            });
        };

        svc.onCancelContinue = function () {
            aside.hide();
        };

        svc.onUpdate = function () {
            if (svc.reqCount !== 0) {
                return;
            }
            else if (svc.allTabsAreValid()) {
                svc.updateAllTabs();
            }
            else {
                svc.focusInvalidTab();
            }
        };

        svc.onUpdateError = function () {
            svc.reqErrorCount++;

            if (svc.reqCount == svc.reqSuccessCount + svc.reqErrorCount) {
                svc.focusErrorTab().resetCounts();
            }
        };

        svc.onUpdateSuccess = function () {
            svc.reqSuccessCount++;

            if (svc.reqCount === svc.reqSuccessCount) {
                aside.hide();
                session.reload();
                svc.resetCounts();
                dashboard.reload();
                userProfileModel.publish(true);
            }
        };

        svc.registerTab = function (tab) {
            svc.tabs[tab.id] = {
                ctrl: tab.ctrl
            };
        };

        svc.resetCounts = function () {
            svc.reqCount = 0;
            svc.reqErrorCount = 0;
            svc.reqSuccessCount = 0;
        };

        svc.reset = function () {
            svc.tabWatch();
            svc.appWatch();
            svc.updateWatch();
            svc.cancelWatch();

            angular.forEach(svc.tabs, function (val) {
                val.ctrl.destroy();
            });

            svc.tabs = {};
            svc.reqCount = 0;
            svc.activeTabs = {};
        };

        svc.updateAllTabs = function () {
            angular.forEach(svc.tabs, function (item, id) {
                if (item.ctrl.isDirty()) {
                    svc.reqCount++;
                    item.ctrl.onUpdate()
                        .then(svc.onUpdateSuccess, svc.onUpdateError);
                }
            });

            if (svc.reqCount === 0) {
                aside.hide();
            }
        };
    }

    angular
        .module("settings")
        .service("mpTabsManager", [
            "$rootScope",
            "timeout",
            "pubsub",
            "mpTabsData",
            "mpTabsMenu",
            "mpAside",
            "userProfileModel",
            "userSessionModel",
            "dashboardModel",
            MpTabsManager
        ]);
})(angular);
