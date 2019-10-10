//  Manage Profile Tabs Menu

(function (angular, undefined) {
    "use strict";

    function factory(tabsMenuModel, tabsData) {
        var model = tabsMenuModel();

        model.setData(tabsData.getList()).disableUnsavedChangesCheck();

        return model;
    }

    angular
        .module("settings")
        .factory("mpTabsMenu", [
            "rpScrollingTabsMenuModel",
            "mpTabsData",
            factory
        ]);
})(angular);
