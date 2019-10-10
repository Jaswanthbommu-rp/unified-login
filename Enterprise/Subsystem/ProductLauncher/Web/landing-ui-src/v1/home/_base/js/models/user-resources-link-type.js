//  User Resource Link Types Model

(function (angular, undefined) {
    "use strict";

    function factory() {
        var types = {
            "prod14": {
                type: "url"
            },
            "prod1": {
                type: "url"
            },
            "prod19": {
                type: "method",
                methodName: "showPLPModal"
            },
            "prod21": {
                type: "url"
            },
            "prod24": {
                type: "url"
            },
            "prod25": {
                type: "url"
            },
            "prod26": {
                type: "url"
            },
            "prod27": {
                type: "url"
            },
            "prod28": {
                type: "url"
            },
            "prod35": {
                type: "url"
            },
            "prod39": {
                type: "url"
            },
            "prod43": {
                type: "url"
            },
            "prod45": {
                type: "url"
            }
        };

        Object.freeze(types);

        return types;
    }

    angular
        .module("settings")
        .factory("userResourceLinkTypes", [factory]);
})(angular);
