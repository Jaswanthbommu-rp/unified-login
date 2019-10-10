//  Performance Analytics Property Grid Config Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, security, persona) {
        var model = gridConfig();

        model.get = function () {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "propertyId"
            }, {
                key: "propertyName",
                type: "text"
            }, {
                key: "statecode",
                type: "texy"
            }];
        };

        model.getHeaders = function () {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: model.isSelectAllEnabled()
                }, {
                    key: "propertyName",
                    text: "Property",
                }, {
                    key: "statecode",
                    text: "State"
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "isAssigned",
                    type: "menu",
                    value: "",
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
                    key: "propertyName",
                    type: "text",
                    placeholder: "Filter by Property Name"
                },
                {
                    key: "statecode",
                    type: "text",
                    placeholder: "Filter by State"
                }
            ];
        };

        model.isSelectAllEnabled = function () {
            var isEnabled = !security.isAllowed("viewUser");
            if (isEnabled  && persona.isReady())
            {
                isEnabled = persona.data.hasManageAssetOptimizationProductAccess;
            }
            return isEnabled;
        };

         model.personaWatch = angular.noop;
         model.personaWatch();
         model.personaWatch = persona.subscribe(model.isSelectAllEnabled);

        return model;
    }
    angular
        .module("settings")
        .factory("paCompanyPropertyGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
