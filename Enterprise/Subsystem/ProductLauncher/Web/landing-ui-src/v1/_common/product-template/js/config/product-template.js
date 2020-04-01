//  Init User Session

(function (angular) {
    "use strict";

    function config(productTemplateModel) {
        productTemplateModel.loadProductTemplates();
    }

    angular
        .module("settings")
        .run(["productTemplateModel", config]);
})(angular);
