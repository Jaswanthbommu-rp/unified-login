//  Rights Grid Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, security, persona) {
        var model = gridConfig();

        model.get = function () {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "id"
            }, {
                key: "name"
            }, {
                key: "city"
            }, {
                key: "state"
            }];
        };

        model.getHeaders = function () {
            return [
                    [{
                    key: "isAssigned",
                    type: "select",
                    enabled: model.isSelectAllEnabled()
                    }, {
                    key: "name",
                    text: "Property",
                    }, {
                        key: "city",
                        text: "City",
                    }, {
                    key: "state",
                    text: "State",
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
                    placeholder: "Filter by Property Name"
                },
                {
                    key: "city",
                    type: "text",
                    placeholder: "Filter by City"
                },
                {
                    key: "state",
                    type: "text",
                    placeholder: "Filter by State"
                }
            ];
        };

        model.isSelectAllEnabled = function () {
            var isEnabled = !security.isAllowed("viewUser");
            if (isEnabled  && persona.isReady())
            {
                isEnabled = persona.data.hasManageOneSiteProductAccess;
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
        .factory("osPropertiesGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
