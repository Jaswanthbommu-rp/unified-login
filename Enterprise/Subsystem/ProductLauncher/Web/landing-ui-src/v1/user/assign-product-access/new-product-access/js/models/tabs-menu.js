(function (angular, undefined) {
    "use strict";

    function factory(tabsMenuModel, tabsData, model) {
        return tabsMenuModel().setData(model.getTabsData());
    }

    angular
        .module("settings")
        .factory("tabsMenu", [
            "rpScrollingTabsMenuModel",
            "tabsData",
            "newProductAccessModel",
            factory
        ]);
})(angular);
