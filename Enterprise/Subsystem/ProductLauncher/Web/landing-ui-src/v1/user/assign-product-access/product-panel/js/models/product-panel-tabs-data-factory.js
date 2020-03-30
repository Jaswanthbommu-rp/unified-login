//  User Tabs Model

(function (angular, undefined) {
    "use strict";

    function factory(tabsMenu) {
        function ProductPanelTabsModel() {
            var s = this;
            s.init();
        }

        var p = ProductPanelTabsModel.prototype;

        p.init = function () {
            var s = this,
                onChange = s.onChange.bind(s);

            s.tabsList = [];
           // s.initActiveTab();
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

        p.setTabMenuData = function (data) {
            var s = this;
            s.tabsMenu.setData(s.tabsList);
            return s;
        };
        // Setters

        p.setTabs = function (tabsData) {
            var s = this,
                tabsList = [];

            s.tabsList = tabsData;
            s.tabsMenu.setData(tabsList);

            return s;
        };

        // Actions
         p.activateTab = function (tabKey) {
            var tab,
                s = this,
                events = s.tabsMenu.getEvents();

            s.tabsList.forEach(function (item){
              if (item.id === tabKey){
                item.isActive = true;
              }
            });

            return s;
        };


        p.initActiveTab = function () {
            var s = this;

             s.tabsList.forEach(function (tab){
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

        // Assertions

        p.hasMultipleTabs = function () {
            var s = this;
            return s.tabsList.length > 1;
        };

        p.reset = function () {
            var s = this;
            s.tabsMenu.setData([]);
        };

        return new ProductPanelTabsModel();
    }

    angular
        .module("settings")
        .factory("productPanelTabsModel", [
            "rpScrollingTabsMenuModel",
            factory
        ]);
})(angular);
