//  Bind  Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (templateModel.isProductExists(1)) {
            productAccess.register({
                model: model,
                key: "soln101",
                product: "1"
            });
        }

        if (templateModel.isProductExists(6)) {
            productAccess.register({
                model: model,
                key: "soln305",
                product: "6"
            });
        }

        if (templateModel.isProductExists(10)) {
            productAccess.register({
                model: model,
                key: "soln302",
                product: "10"
            });
        }

        if (templateModel.isProductExists(14)) {
            productAccess.register({
                model: model,
                key: "soln501",
                product: "14"
            });
        }

        if (templateModel.isProductExists(9)) {
            productAccess.register({
                model: model,
                key: "soln303",
                product: "9"
            });
        }

        if (templateModel.isProductExists(3)) {
            productAccess.register({
                model: model,
                key: "soln503",
                product: "3"
            });
        }

        if (templateModel.isProductExists(23)) {
            productAccess.register({
                model: model,
                key: "soln307",
                product: "23"
            });
        }

        if (templateModel.isProductExists(15)) {
            productAccess.register({
                model: model,
                key: "soln204",
                product: "15"
            });
        }

        if (templateModel.isProductExists(17)) {
            productAccess.register({
                model: model,
                key: "soln201",
                product: "17"
            });
        }

        if (templateModel.isProductExists(26)) {
            productAccess.register({
                model: model,
                key: "soln107",
                product: "26"
            });
        }

        if (templateModel.isProductExists(39)) {
            productAccess.register({
                model: model,
                key: "soln505",
                product: "39"
            });
        }

        if (templateModel.isProductExists(40)) {
            productAccess.register({
                model: model,
                key: "soln308",
                product: "40"
            });
        }

        if (templateModel.isProductExists(41)) {
            productAccess.register({
                model: model,
                key: "soln309",
                product: "41"
            });
        }


        if (templateModel.isProductExists(29)) {
            productAccess.register({
                model: model,
                key: "soln402",
                product: "29"
            });
        }

        if (templateModel.isProductExists(30)) {
            productAccess.register({
                model: model,
                key: "soln403",
                product: "30"
            });
        }

        if (templateModel.isProductExists(31)) {
            productAccess.register({
                model: model,
                key: "soln404",
                product: "31"
            });
        }


        if (templateModel.isProductExists(32)) {
            productAccess.register({
                model: model,
                key: "soln401",
                product: "32"
            });
        }

        if (templateModel.isProductExists(51)) {
            productAccess.register({
                model: model,
                key: "soln407",
                product: "51"
            });
        }

        if (templateModel.isProductExists(52)) {
            productAccess.register({
                model: model,
                key: "soln408",
                product: "52"
            });
        }

        if (templateModel.isProductExists(53)) {
            productAccess.register({
                model: model,
                key: "soln409",
                product: "53"
            });
        }

        if (templateModel.isProductExists(54)) {
            productAccess.register({
                model: model,
                key: "soln410",
                product: "54"
            });
        }
        logc("productAccess", productAccess);
    }

    angular
        .module("settings")
        .run(["productPanelDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
