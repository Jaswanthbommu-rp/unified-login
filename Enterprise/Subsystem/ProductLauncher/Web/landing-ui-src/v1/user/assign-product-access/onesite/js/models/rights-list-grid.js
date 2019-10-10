//  Rights Aside Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [
            {
                key: "centerName",
                type: "text"
            }, {
                key: "description",
                type: "text"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "centerName",
                    text: "Product Center"
                }, {
                    key: "description",
                    text: "Right",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "centerName",
                    type: "text",
                    placeholder: "Find by Product Center"
                },
                {
                    key: "description",
                    type: "text",
                    placeholder: "Find by Right"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("OSRightsAsideGrigConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
