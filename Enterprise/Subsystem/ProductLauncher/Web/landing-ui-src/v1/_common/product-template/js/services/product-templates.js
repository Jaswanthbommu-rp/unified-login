(function (angular, undefined) {
    "use strict";

    function factory($resource, ENV) {
        return $resource(ENV.landingAPI + "apicore/v2/UserMgmt/ListActiveProductPage");
    }

    angular
        .module("settings")
        .factory("productTemplatesSvc", ["$resource", "ENV", factory]);
})(angular);
