//  Aside Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "description",
                type: "text"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "description",
                    text: "Right",
                }]
            ];
        };

        model.getFilters = function() {
            return [

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
        .factory("userMgmtRightsAsideGrigConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);