//  Favorites Service

(function (angular) {
    "use strict";

    function favoritesSvc($resource, ENV) {
        var url, actions, params;

        url = ENV.landingAPI + "api/personas/products/:productId/productSettings";

        actions = {
            update: {
                method: "PUT"
            }
        };

        params = {
            value: "@value",           //  true ? 1 : 0
            name: "IsFavorite",
            productId: "@productId"
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("favoritesSvc", ["$resource", "ENV", favoritesSvc]);
})(angular);
