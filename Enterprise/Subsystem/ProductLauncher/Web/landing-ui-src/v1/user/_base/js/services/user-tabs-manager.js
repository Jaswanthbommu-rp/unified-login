//  User Tabs Manager Service

(function (angular, undefined) {
    "use strict";

    function UserTabsManager(timeout, userTabs) {
        var svc = this;

        svc.tabs = {};
        svc.resp = [];
        svc.errorCount = 0;
        svc.successCount = 0;
        svc.tabWatch = angular.noop;
        svc.saveErrorCallback = angular.noop;
        svc.saveSuccessCallback = angular.noop;

        svc.init = function () {
            svc.tabWatch();
            timeout(svc.initActiveTab, 200);
            svc.tabsList = userTabs.getTabsList();
            svc.tabWatch = userTabs.subscribe(svc.onTabChange);
        };

        // Getters

        svc.getErrorTabName = function () {
            var name;

            angular.forEach(svc.tabs, function (tab) {
                if (!name && tab.ctrl.hasError()) {
                    name = tab.name;
                }
            });

            return name;
        };

        svc.getInvalidTab = function () {
            var tab;

            angular.forEach(svc.tabs, function (item) {
                if (!tab && !item.ctrl.isValid()) {
                    tab = item;
                }
            });

            return tab;
        };

        svc.getInvalidTabName = function () {
            return svc.getInvalidTab().name;
        };

        // Setters

        svc.setSaveErrorCallback = function (callback) {
            svc.saveErrorCallback = callback;
        };

        svc.setSaveSuccessCallback = function (callback) {
            svc.saveSuccessCallback = callback;
            return svc;
        };

        // Actions

        svc.activateErrorTab = function () {
            var tabName = svc.getErrorTabName();
            userTabs.activateTab(tabName);
            return svc;
        };

        svc.activateInvalidTab = function () {
            var tab = svc.getInvalidTab();
            userTabs.activateTab(tab.name);

            if (tab.ctrl.showErrors) {
                tab.ctrl.showErrors();
            }

            return svc;
        };

        svc.initActiveTab = function () {
            var tab = userTabs.getActiveTab();
            svc.tabs[tab.id].ctrl.onTabActive();
            return svc;
        };

        svc.onSaveError = function (data) {
            svc.errorCount++;
            svc.resp.push(data);

            if (svc.errorCount + svc.successCount == svc.tabCount) {
                svc.saveErrorCallback(svc.resp);
            }
        };

        svc.onSaveSuccess = function (data) {
            svc.successCount++;
            svc.resp.push(data);

            if (svc.errorCount + svc.successCount == svc.tabCount) {
                svc.saveSuccessCallback(svc.resp);
            }
        };

        svc.onTabChange = function (tab) {
            var method1 = "onTabActive",
                method2 = "onTabInactive";

            angular.forEach(svc.tabs, function (item) {
                var same = item.name == tab.id,
                    method = same ? method1 : method2;

                if (item.ctrl[method]) {
                    item.ctrl[method]();
                }
            });
        };

        svc.processData = function () {
            if (svc.allTabsValid()) {
                svc.saveData();
            }
            else {
                svc.activateInvalidTab();
            }
        };

        svc.register = function (tab) {
            svc.tabs[tab.name] = tab;
        };

        svc.remove = function (tabName) {
            delete svc.tabs[tabName];
        };

        svc.resetCounts = function () {
            svc.tabCount = 0;
            svc.errorCount = 0;
            svc.successCount = 0;
            return svc;
        };

        svc.resetTabs = function () {
            angular.forEach(svc.tabs, function (tab) {
                tab.ctrl.reset();
            });
        };

        svc.saveData = function () {
            var error = svc.onSaveError,
                success = svc.onSaveSuccess;

            svc.resp = [];
            svc.resetCounts();
logc("svc.tabs",svc.tabs);
            angular.forEach(svc.tabs, function (tab) {
                if (tab.ctrl.hasSaveFn()) {
                    svc.tabCount++;
                    tab.ctrl.saveData().then(success, error);
                }
            });
        };

        // Assertions

        svc.allTabsValid = function () {
            var valid = true;

            angular.forEach(svc.tabs, function (tab) {
                valid = valid && tab.ctrl.isValid();
            });

            return valid;
        };

        // Reset

        svc.reset = function () {
            svc.tabs = {};
            svc.tabWatch();
            svc.saveErrorCallback = angular.noop;
            svc.saveSuccessCallback = angular.noop;
        };
    }

    angular
        .module("settings")
        .service("userTabsManager", [
            "timeout",
            "userTabsModel",
            UserTabsManager
        ]);
})(angular);
