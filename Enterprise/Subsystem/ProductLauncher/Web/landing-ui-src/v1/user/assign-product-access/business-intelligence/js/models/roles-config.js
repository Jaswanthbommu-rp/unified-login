//  BusinessIntelligence Roles Grid Config Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, security, persona) {
        var model = gridConfig();

        model.get = function () {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "name"
            }, {
                key: "displayName",
                type: "text",
            }];
        };

        model.getHeaders = function () {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: model.isSelectAllEnabled()
                }, {
                    key: "displayName",
                    text: "Role",
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
                    key: "displayName",
                    type: "text",
                    placeholder: "Filter by Role Name"
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
        .factory("BusinessIntelligenceRolesGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            "personaDetails",
            factory
        ]);
})(angular);
