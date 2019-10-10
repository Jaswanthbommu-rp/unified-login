//  Unified Amenities Tabs Menu Model

(function (angular, undefined) {
    "use strict";

    function factory(tabsMenuModel, tabsData) {
        return tabsMenuModel().setData(tabsData.getList());
    }

    angular
        .module("settings")
        .factory("uaTabsMenu", [
            "rpScrollingTabsMenuModel",
            "uaTabsData",
            factory
        ]);
})(angular);
