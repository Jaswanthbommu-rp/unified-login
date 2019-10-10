//  Products Nav Model

(function (angular) {
    "use strict";

    function factory(tabsMenu) {
        var tabs = [
            {
                id: "01",
                text: "All Products",
                sref: "products.all"
            },
            {
                id: "02",
                text: "Favorites",
                sref: "products.favorites"
            }
        ];

        var model = {
            tabs: tabs
        };

        model.getMenu = function () {
            return tabsMenu(tabs);
        };

        return model;
    }

    angular
        .module("settings")
        .factory("productsTabsMenuModel", ["rpScrollingTabsMenuModel", factory]);
})(angular);
