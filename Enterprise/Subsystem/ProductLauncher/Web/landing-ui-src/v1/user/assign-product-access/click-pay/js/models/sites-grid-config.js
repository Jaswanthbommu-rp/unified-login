// Sites Aside Grid Config Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function () {
            return [
                {

                    key: "isAssigned",
                    type: "select",
                    idKey: "id"
                },
                {
                    key: "name",
                    type: "text"
                },
                {
                    key: "llcName",
                    type: "text"
                }


            ];
        };

        model.getHeaders = function () {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: true
                },
                {
                    key: "name",
                    text: "Property",
                },
                {
                    key: "llcName",
                    text: "LLC",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "isAssigned",
                    value: "",
                    type: "menu",
                    size: "small",
                    options: [
                        {
                            value: "",
                            name: "All"
                        },
                        {
                            value: true,
                            name: "Selected"
                        },
                        {
                            value: false,
                            name: "Not Selected"
                        }
                    ]
                },
                {
                    key: "name",
                    type: "text",
                    placeholder: "Find by Property"
                },
                {
                    key: "llcName",
                    type: "text",
                    placeholder: "Find by LLC"
                }
            ];
        };

        model.getTrackSelectionConfig = function () {
            var config = {},
                columns = model.get();

            columns.forEach(function (column) {
                if (column.type == "select") {
                    config.idKey = column.idKey;
                    config.selectKey = column.key;
                }
            });

            return config;
        };

        return model;
    }
    angular
        .module("settings")
        .factory("CPSiteAsideGrigConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
