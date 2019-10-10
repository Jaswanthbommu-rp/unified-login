//  App Switcher Data

(function (angular, undefined) {
    "use strict";

    function AppSwitcherData(productsData, familyModel, menuModel) {
        var svc = this,
            excludedProducts = [],
            productFilter = "appSwitcher";

        //excluding AO Benchmarking (34) and EasyLMS (36)
        excludedProducts = [34, 36];

        svc.prodWatch = angular.noop;

        svc.bind = function () {
            svc.prodWatch();
            svc.prodWatch = productsData.subscribe(svc.setData);
        };

        svc.setData = function () {
            var newFamilies = [],
                families = productsData.getFamilies();

            families.forEach(function (item) {
                var solns = item.getSolutions(productFilter),
                    family = familyModel(item.getData());

                if (solns.length) {
                    newFamilies.push(family);
                    family.setSolutions(solns);
                }
            });

            menuModel.setData({
                families: newFamilies,
                solutions: productsData.getSolutions(productFilter)
            });
        };
    }

    angular
        .module("settings")
        .service("appSwitcherData", [
            "productsDataModel",
            "productFamilyModel",
            "rpGhAppSwitcherMenuModel",
            AppSwitcherData
        ]);
})(angular);
