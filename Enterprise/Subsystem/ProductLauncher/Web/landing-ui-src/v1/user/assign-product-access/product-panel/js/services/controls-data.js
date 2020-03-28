// Controls Service

(function(angular) {
    "use strict";

    function ProductControlsSvc($resource, ENV) {
        var url, params,actions;
        url = "https://ulapi-dev.realpage.com/v2/UserMgmt/ProductAccess";
        //url = "https://www-local2.realpage.com/apicore/v2/UserMgmt/ProductAccess";

        params = {
            productId: "@productId"
        };

         actions = {
            get: {
                method: "GET",
                cancellable: true
            }
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("productControlsSvc", [
            "$resource",
            "ENV",
            ProductControlsSvc
        ]);
})(angular);


// (function (angular) {
//     "use strict";

//     function productControlsSvc($resource, ENV) {
//         var params, svc = {};
//         //var url = ENV.landingAPI + "api/products/easylms";
//         var url = "https://ulapi-dev.realpage.com/v2/UserMgmt/ProductAccess";

//         svc.getProductControlsData = function (parameter) {
//             params = parameter;
//             return $resource(url,params).get().$promise
//                 .then(svc.formatResponse);
//         };

//         //adapter for API response
//         svc.formatResponse = function (response) {
//             var newResponse = {};

//             if (response.status.success === false) {
//                 newResponse.isError = true;
//                 newResponse.errorReason = response.status.errorMsg;
//                 newResponse.errorCode = response.status.errorCode;
//             }
//             else {
//                 newResponse = response;
//             }

//             return newResponse;
//         };

//         return svc;
//     }

//     angular
//         .module("settings")
//         .factory("productControlsSvc", [
//             "$resource",
//             "ENV",
//             productControlsSvc
//         ]);
// })(angular);
