//  User Tabs Model

(function (angular, undefined) {
    "use strict";

    function factory(tabsMenu, tabsData) {
        function UserTabsModel() {
            var s = this;
            s.init();
        }

        var p = UserTabsModel.prototype;

        p.init = function () {
            var s = this,
                onChange = s.onChange.bind(s);

            s.tabsList = [];
            s.initActiveTab();
            s.tabsMenu = tabsMenu();
            s.changeWatch = s.tabsMenu.subscribe("change", onChange);
        };

        // Getters

        p.getActiveTab = function () {
            var s = this;
            return s.activeTab;
        };

        p.getActiveTabId = function () {
            var s = this;
            return s.activeTab.id;
        };

        p.getActiveTabUrl = function () {
            var s = this;
            return s.activeTab.templateUrl;
        };

        p.getTabsList = function () {
            var s = this;
            return s.tabsList;
        };

        p.getTabsMenu = function () {
            var s = this;
            return s.tabsMenu;
        };

        // Setters

        p.setTabs = function (keys) {
            var s = this,
                tabsList = [];

            keys.forEach(function (key) {
                tabsList.push(tabsData[key]);
            });

            s.tabsList = tabsList;
            s.tabsMenu.setData(tabsList);

            return s;
        };

        // Actions

        p.activateTab = function (tabKey) {
            var tab,
                s = this,
                events = s.tabsMenu.getEvents();

            angular.forEach(tabsData, function (item, key) {
                item.isActive = key == tabKey;

                if (item.isActive) {
                    tab = item;
                }
            });

            events.publish("change", tab);

            return s;
        };

        p.initActiveTab = function () {
            var s = this;

            angular.forEach(tabsData, function (tab) {
                if (!s.activeTab && tab.isActive) {
                    s.activeTab = tab;
                }
            });
        };

        p.onChange = function (tab) {
            var s = this;
            s.activeTab = tab;
            return s;
        };

        p.subscribe = function (callback) {
            var s = this;
            return s.tabsMenu.subscribe("change", callback);
        };

        // Assertions

        p.hasMultipleTabs = function () {
            var s = this;
            return s.tabsList.length > 1;
        };

        p.reset = function () {
            var s = this;
            s.tabsMenu.setData([]);
        };

        return new UserTabsModel();
    }

    angular
        .module("settings")
        .factory("userTabsModel", [
            "rpScrollingTabsMenuModel",
            "userTabsData",
            factory
        ]);
})(angular);
