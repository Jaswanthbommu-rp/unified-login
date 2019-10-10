//  Access Note Defined Model

(function (angular, undefined) {
    "use strict";

    function factory(modalModel) {
        return modalModel("user/assign-product-access/access-not-defined/templates/index.html");
    }

    angular
        .module("settings")
        .factory("accessNotDefinedModal", ["rpModalModel", factory]);
})(angular);
