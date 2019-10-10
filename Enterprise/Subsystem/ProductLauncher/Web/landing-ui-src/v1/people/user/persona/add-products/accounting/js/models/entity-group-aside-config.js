//  Accounting Entity Group Aside Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [
            {
                key: "name",
                type: "text"
            }, {
                key: "state",
                type: "text"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "name",
                    text: "Entity"
                }, {
                    key: "state",
                    text: "State",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "name",
                    type: "text",
                    placeholder: "Entity Name"
                },
                {
                    key: "state",
                    type: "text",
                    placeholder: "State"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("AEntityGroupAsideGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
